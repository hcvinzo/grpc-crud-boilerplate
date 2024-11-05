using AutoMapper;
using GrpcCrudBoilerplate.DataContext.Entities;
using GrpcCrudBoilerplate.Contracts;

namespace GrpcCrudBoilerplate.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Order -> OrderDto mapping
        CreateMap<Order, OrderDto>()
            .ForMember(dest => dest.CreatorUsername,
                opt => opt.MapFrom(src => src.Creator != null ? src.Creator.Username : null))
            .ForMember(dest => dest.UpdaterUsername,
                opt => opt.MapFrom(src => src.Updater != null ? src.Updater.Username : null))
            .ForMember(dest => dest.Items,
                opt => opt.MapFrom(src => src.Items));

        // OrderDto -> Order mapping
        CreateMap<OrderDto, Order>()
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.Items,
                opt => opt.MapFrom(src => src.Items));

        // OrderItem -> OrderItemDto mapping
        CreateMap<OrderItem, OrderItemDto>();

        // OrderItemDto -> OrderItem mapping
        CreateMap<OrderItemDto, OrderItem>()
            .ForMember(dest => dest.Order, opt => opt.Ignore());
    }
}

