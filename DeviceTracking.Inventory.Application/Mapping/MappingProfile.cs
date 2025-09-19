using AutoMapper;
using DeviceTracking.Inventory.Shared.DTOs;
using DeviceTracking.Inventory.Shared.Entities;

namespace DeviceTracking.Inventory.Application.Mapping;

/// <summary>
/// AutoMapper profile for entity-DTO mappings
/// </summary>
public class MappingProfile : Profile
{
    /// <summary>
    /// Constructor - define all mappings
    /// </summary>
    public MappingProfile()
    {
        // Inventory Item mappings
        CreateMap<CreateInventoryItemDto, InventoryItem>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.LastMovement, opt => opt.Ignore())
            .ForMember(dest => dest.Transactions, opt => opt.Ignore());

        CreateMap<UpdateInventoryItemDto, InventoryItem>()
            .ForMember(dest => dest.PartNumber, opt => opt.Condition(src => src.PartNumber != null))
            .ForMember(dest => dest.Description, opt => opt.Condition(src => src.Description != null))
            .ForMember(dest => dest.Barcode, opt => opt.Condition(src => src.Barcode != null))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        CreateMap<InventoryItem, InventoryItemDto>()
            .ForMember(dest => dest.Location, opt => opt.MapFrom(src => src.Location))
            .ForMember(dest => dest.Supplier, opt => opt.MapFrom(src => src.Supplier));

        CreateMap<InventoryItem, InventoryItemSummaryDto>()
            .ForMember(dest => dest.LocationName, opt => opt.MapFrom(src => src.Location != null ? src.Location.Name : null))
            .ForMember(dest => dest.StockStatus, opt => opt.MapFrom(src => GetStockStatus(src)));

        // Location mappings
        CreateMap<CreateLocationDto, Location>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.ChildLocations, opt => opt.Ignore());

        CreateMap<UpdateLocationDto, Location>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

        CreateMap<Location, LocationDto>()
            .ForMember(dest => dest.ParentLocation, opt => opt.MapFrom(src => src.ParentLocation))
            .ForMember(dest => dest.ChildLocationsCount, opt => opt.MapFrom(src => src.ChildLocations.Count))
            .ForMember(dest => dest.CapacityUtilization, opt => opt.Ignore()) // Set in service layer
            .ForMember(dest => dest.TotalItems, opt => opt.Ignore()) // Set in service layer
            .ForMember(dest => dest.TotalValue, opt => opt.Ignore()); // Set in service layer

        CreateMap<Location, LocationSummaryDto>();

        CreateMap<Location, LocationHierarchyDto>()
            .ForMember(dest => dest.Level, opt => opt.Ignore()) // Set in service layer
            .ForMember(dest => dest.HierarchyPath, opt => opt.Ignore()); // Set in service layer

        // Supplier mappings
        CreateMap<CreateSupplierDto, Supplier>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.InventoryItems, opt => opt.Ignore());

        CreateMap<UpdateSupplierDto, Supplier>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

        CreateMap<Supplier, SupplierDto>()
            .ForMember(dest => dest.TotalItems, opt => opt.Ignore()) // Set in service layer
            .ForMember(dest => dest.TotalValue, opt => opt.Ignore()) // Set in service layer
            .ForMember(dest => dest.LastOrderDate, opt => opt.Ignore()); // Set in service layer

        CreateMap<Supplier, SupplierSummaryDto>();

        CreateMap<Supplier, SupplierPerformanceDto>()
            .ForMember(dest => dest.SupplierName, opt => opt.MapFrom(src => src.CompanyName))
            .ForMember(dest => dest.TotalOrders, opt => opt.Ignore()) // Set in service layer
            .ForMember(dest => dest.AverageLeadTime, opt => opt.Ignore()) // Set in service layer
            .ForMember(dest => dest.OnTimeDeliveryRate, opt => opt.Ignore()) // Set in service layer
            .ForMember(dest => dest.TotalOrderValue, opt => opt.Ignore()); // Set in service layer
    }

    /// <summary>
    /// Get stock status for an inventory item
    /// </summary>
    private static string GetStockStatus(InventoryItem item)
    {
        if (item.CurrentStock == 0) return "Out of Stock";
        if (item.CurrentStock <= item.MinimumStock) return "Low Stock";
        if (item.CurrentStock >= item.MaximumStock) return "Overstocked";
        return "In Stock";
    }
}
