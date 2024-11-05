using System.Globalization;
using CsvHelper;
using Grpc.Core;
using GrpcCrudBoilerplate.Contracts;
using GrpcCrudBoilerplate.Services.Order;
using GrpcCrudBoilerplate.v1;
using Microsoft.AspNetCore.Authorization;
using GrpcCrudBoilerplate.Infrastructure.Authorization;

namespace GrpcCrudBoilerplate.GrpcServices.v1;

[Authorize]
public class OrderImportService : OrderImporter.OrderImporterBase
{
    private readonly ILogger<OrderImportService> _logger;
    private readonly IOrderService _orderService;

    public OrderImportService(ILogger<OrderImportService> logger, IOrderService orderService)
    {
        _logger = logger;
        _orderService = orderService;
    }

    /// <summary>
    /// Import orders from a CSV stream (streaming version)
    /// </summary>
    /// <param name="requestStream"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    [RequirePermission("Order.Create")]
    public override async Task<ImportOrderResponse> ImportOrderCsvStream(IAsyncStreamReader<ImportOrderRequest> requestStream, ServerCallContext context)
    {
        if (requestStream == null)
            throw new ArgumentException("requestStream cannot be null");

        var orders = new List<OrderDto>();

        await foreach (var request in requestStream.ReadAllAsync())
        {
            if (request == null)
                throw new ArgumentException("request cannot be null");

            if (string.IsNullOrEmpty(request.CsvContent))
                throw new ArgumentException("CsvContent cannot be null");

            using var reader = new StringReader(request.CsvContent);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

            orders.AddRange(csv.GetRecords<OrderDto>());
        }

        // insert orders to db
        await _orderService.CreateOrders(orders);

        _logger.LogInformation("Imported {Count} orders", orders.Count);

        // Here you would typically save the orders to a database
        // For this example, we'll just return the count

        return new ImportOrderResponse { ImportedCount = orders.Count };

    }

    /// <summary>
    /// Import orders from a CSV string
    /// </summary>
    /// <param name="request"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    [RequirePermission("Order.Create")]
    public override async Task<ImportOrderResponse> ImportOrderCsv(ImportOrderRequest request, ServerCallContext context)
    {
        if (request == null)
            throw new ArgumentException("request cannot be null"); ;

        if (string.IsNullOrEmpty(request.CsvContent))
            throw new ArgumentException("CsvContent cannot be null");

        var orders = new List<OrderDto>();

        using var reader = new StringReader(request.CsvContent);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

        orders.AddRange(csv.GetRecords<OrderDto>());

        // insert orders to db
        await _orderService.CreateOrders(orders);

        _logger.LogInformation("Imported {Count} orders (non-streaming)", orders.Count);

        return new ImportOrderResponse { ImportedCount = orders.Count };
    }

    /// <summary>
    /// Export orders to a CSV file
    /// </summary>
    /// <param name="request"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    [RequirePermission(["Order.ViewAny", "Order.ViewOwn"])]
    public override async Task<ExportOrderResponse> ExportOrderCsv(ExportOrderRequest request, ServerCallContext context)
    {

        // Get orders with items for a specific date range
        var startDate = request.StartDate?.ToDateTime() ?? DateTime.UtcNow.AddDays(-7);
        var endDate = request.EndDate?.ToDateTime() ?? DateTime.UtcNow;

        var orders = await _orderService.RetrieveOrders(startDate, endDate, includeItems: false);

        using var writer = new StringWriter();
        using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

        csv.WriteRecords(orders);

        var csvContent = writer.ToString();

        _logger.LogInformation("Exported {Count} orders", orders.Count);

        return await Task.FromResult(new ExportOrderResponse { CsvContent = csvContent });
    }

    /// <summary>
    /// Export orders to a CSV file (streaming version)
    /// </summary>
    /// <param name="request"></param>
    /// <param name="responseStream"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    [RequirePermission(["Order.ViewAny", "Order.ViewOwn"])]
    public override async Task ExportOrderCsvStream(ExportOrderRequest request, IServerStreamWriter<ExportOrderResponse> responseStream, ServerCallContext context)
    {
        // Get orders with items for a specific date range
        var startDate = request.StartDate?.ToDateTime() ?? DateTime.UtcNow.AddDays(-7);
        var endDate = request.EndDate?.ToDateTime() ?? DateTime.UtcNow;

        var orders = await _orderService.RetrieveOrders(startDate, endDate, includeItems: false);

        using var writer = new StringWriter();
        using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

        csv.WriteHeader<OrderDto>();
        await csv.NextRecordAsync();

        foreach (var order in orders)
        {
            csv.WriteRecord(order);
            await csv.NextRecordAsync();

            var csvContent = writer.ToString();
            writer.GetStringBuilder().Clear();

            await responseStream.WriteAsync(new ExportOrderResponse { CsvContent = csvContent });
        }

        _logger.LogInformation("Exported {Count} orders (streaming)", orders.Count);
    }
}

