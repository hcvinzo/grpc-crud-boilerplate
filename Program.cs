using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using System.Reflection;
using Serilog;
using Microsoft.EntityFrameworkCore;
using GrpcCrudBoilerplate.Services.Authentication;
using GrpcCrudBoilerplate.GrpcServices.v1;
using GrpcCrudBoilerplate.Mappings;
using GrpcCrudBoilerplate.Services.Order;
using GrpcCrudBoilerplate.Infrastructure.Authorization;
using GrpcCrudBoilerplate.Services.Authorization;

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    //.WriteTo.File("logs/GrpcCrudBoilerplate-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .Enrich.With<CorrelationIdEnricher>()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .WriteTo.File("logs/GrpcCrudBoilerplate-.txt", rollingInterval: RollingInterval.Day,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {CorrelationId} {Message:lj}{NewLine}{Exception}")
    .CreateLogger();
try
{
    Log.Information("Starting web application");

    var builder = WebApplication.CreateBuilder(args);

    // Add Serilog
    builder.Host.UseSerilog();

    // Configure Kestrel to use both HTTP/1.1 and HTTP/2
    builder.WebHost.ConfigureKestrel(options =>
    {
        options.ListenLocalhost(5000, o => o.Protocols = HttpProtocols.Http1AndHttp2);
        options.ListenLocalhost(5001, o =>
        {
            o.Protocols = HttpProtocols.Http2;
            o.UseHttps();
        });
    });

    // Add services to the container.
    builder.Services.AddGrpcReflection();
    builder.Services.AddSingleton<IJwtTokenService, JwtTokenService>();

    // Add DbContext
    builder.Services.AddDbContext<GrpcCrudBoilerplate.DataContext.AppDbContext>(options =>
    options.UseInMemoryDatabase("TestDb"));  // In-memory database

    // Add exception interceptor
    builder.Services.AddGrpc(options =>
    {
        options.Interceptors.Add<GrpcCrudBoilerplate.Infrastructure.ExceptionHandling.ExceptionInterceptor>();
        options.Interceptors.Add<RequirePermissionInterceptor>();
    });

    builder.Services.AddAutoMapper(typeof(MappingProfile));
    builder.Services.AddMemoryCache();
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = builder.Configuration["Jwt:Issuer"],
                ValidAudience = builder.Configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key is not configured")))
            };
        });
    builder.Services.AddAuthorization();

    builder.Services.AddGrpc().AddJsonTranscoding();
    builder.Services.AddGrpcSwagger();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "Grpc Crud Example", Version = "v1" });

        var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        c.IncludeXmlComments(xmlPath);
        c.IncludeGrpcXmlComments(xmlPath, includeControllerXmlComments: true);

        c.AddServer(new OpenApiServer { Url = "http://localhost:5000", Description = "HTTP/1.1 and HTTP/2" });
        c.AddServer(new OpenApiServer { Url = "https://localhost:5001", Description = "HTTPS (HTTP/2)" });

        c.DocumentFilter<GrpcCrudBoilerplate.GrpcDocumentFilter>();
    });

    // Add Services
    builder.Services.AddScoped<IOrderService, GrpcCrudBoilerplate.Services.Order.OrderService>();
    builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
    builder.Services.AddScoped<IAuthorizationService, AuthorizationService>();

    builder.Services.AddHttpContextAccessor();
    builder.Services.AddScoped<IUserContext, UserContext>();

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    app.UseCorrelationId();

    app.UseAuthentication();
    app.UseAuthorization();

    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Grpc Crud Example V1");
        //c.RoutePrefix = string.Empty;
        c.EnableTryItOutByDefault();
    });

    // Map gRPC service
    app.MapGrpcService<GreeterService>();
    app.MapGrpcService<OrderImportService>();
    app.MapGrpcService<GrpcCrudBoilerplate.GrpcServices.v1.OrderService>();
    app.MapGrpcService<AuthService>();
    app.MapGrpcReflectionService();

    // Add this line to serve the Swagger UI at the root
    app.MapGet("/", () => Results.Redirect("/swagger"));

    app.Use(async (context, next) =>
    {
        if (context.Request.Path.StartsWithSegments("/swagger"))
        {
            context.Response.Headers.Append("Cache-Control", "no-cache, no-store, must-revalidate");
            context.Response.Headers.Append("Pragma", "no-cache");
            context.Response.Headers.Append("Expires", "0");
        }
        await next();
    });

    // Seed initial data
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<GrpcCrudBoilerplate.DataContext.AppDbContext>();
        GrpcCrudBoilerplate.DataContext.SeedInitialData.Initialize(context);
    }

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}