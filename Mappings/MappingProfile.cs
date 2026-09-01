using AutoMapper;
using PosApi.DTOs.Cash;
using PosApi.DTOs.Customer;
using PosApi.DTOs.Discount;
using PosApi.DTOs.Expense;
using PosApi.DTOs.Grn;
using PosApi.DTOs.Organization;
using PosApi.DTOs.Product;
using PosApi.DTOs.Purchase;
using PosApi.DTOs.Security;
using PosApi.DTOs.Sale;
using PosApi.DTOs.Payment;
using PosApi.DTOs.Stock;
using PosApi.DTOs.Vendor;
using PosApi.Models.Entities;

namespace PosApi.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Customer, CustomerDto>();

        // Organization
        CreateMap<Company, CompanyDto>();
        CreateMap<Branch, BranchDto>();
        CreateMap<Warehouse, WarehouseDto>();

        // Security
        CreateMap<UserRole, UserRoleDto>();
        CreateMap<UserRole, UserRoleWithPermissionsDto>()
            .ForMember(dest => dest.Permissions,
                opt => opt.MapFrom(src => src.UserRolePermissions.Select(rp => rp.Permission)));
        CreateMap<Permission, PermissionDto>();
        CreateMap<UserRolePermission, UserRolePermissionDto>()
            .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.Role != null ? src.Role.RoleName : null))
            .ForMember(dest => dest.PermissionName, opt => opt.MapFrom(src => src.Permission != null ? src.Permission.PermissionName : null));
        CreateMap<SystemUser, SystemUserDto>()
            .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.Role != null ? src.Role.RoleName : null));

        // Vendor
        CreateMap<Vendor, VendorDto>()
            .ForMember(dest => dest.OutstandingBalance, opt => opt.MapFrom(src => src.VendorLedger != null ? src.VendorLedger.OutstandingBalance : (decimal?)null));
        CreateMap<VendorLedger, VendorLedgerDto>()
            .ForMember(dest => dest.VendorCode, opt => opt.MapFrom(src => src.Vendor != null ? src.Vendor.VendorCode : null))
            .ForMember(dest => dest.VendorName, opt => opt.MapFrom(src => src.Vendor != null ? src.Vendor.VendorName : null));

        // Stock
        CreateMap<StockInventory, StockInventoryDto>()
            .ForMember(dest => dest.ItemName, opt => opt.MapFrom(src => src.Product != null ? src.Product.ItemName : null));
        CreateMap<StockBatch, StockBatchDto>();
        CreateMap<StockMovement, StockMovementDto>();

        // Product
        CreateMap<Category, CategoryDto>();
        CreateMap<Brand, BrandDto>();
        CreateMap<TaxMaster, TaxMasterDto>();
        CreateMap<ProductMaster, ProductDto>()
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.CategoryName : null))
            .ForMember(dest => dest.BrandName, opt => opt.MapFrom(src => src.Brand != null ? src.Brand.BrandName : null))
            .ForMember(dest => dest.TaxPercentage, opt => opt.MapFrom(src => src.Tax != null ? src.Tax.Percentage : (decimal?)null));

        // Purchase order
        CreateMap<PurchaseOrder, PurchaseOrderDto>()
            .ForMember(dest => dest.VendorCode, opt => opt.MapFrom(src => src.Vendor != null ? src.Vendor.VendorCode : null))
            .ForMember(dest => dest.VendorName, opt => opt.MapFrom(src => src.Vendor != null ? src.Vendor.VendorName : null))
            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items));
        CreateMap<PurchaseOrderItem, PurchaseOrderItemDto>()
            .ForMember(dest => dest.ItemName, opt => opt.MapFrom(src => src.Product != null ? src.Product.ItemName : null));
        CreateMap<PurchaseOrderHistory, PurchaseOrderHistoryDto>()
            .ForMember(dest => dest.Changes, opt => opt.MapFrom(src => src.Changes));
        CreateMap<PurchaseOrderHistoryChange, PurchaseOrderHistoryChangeDto>();

        // GRN
        CreateMap<GrnMaster, GrnDto>()
            .ForMember(dest => dest.VendorCode, opt => opt.MapFrom(src => src.Vendor != null ? src.Vendor.VendorCode : null))
            .ForMember(dest => dest.VendorName, opt => opt.MapFrom(src => src.Vendor != null ? src.Vendor.VendorName : null))
            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items));
        CreateMap<GrnItem, GrnItemDto>()
            .ForMember(dest => dest.ItemName, opt => opt.MapFrom(src => src.Product != null ? src.Product.ItemName : null));
        CreateMap<GrnReturn, GrnReturnDto>()
            .ForMember(dest => dest.GrnNo, opt => opt.MapFrom(src => src.GrnMaster != null ? src.GrnMaster.GrnNo : null))
            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items));
        CreateMap<GrnReturnItem, GrnReturnItemDto>()
            .ForMember(dest => dest.ItemName, opt => opt.MapFrom(src => src.Product != null ? src.Product.ItemName : null));

        // Discount
        CreateMap<Discount, DiscountDto>()
            .ForMember(dest => dest.ItemName, opt => opt.MapFrom(src => src.Product != null ? src.Product.ItemName : null));

        // Sales
        CreateMap<Sale, SaleDto>()
            .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Customer != null ? src.Customer.CustomerName : null))
            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items));
        CreateMap<SaleItem, SaleItemDto>()
            .ForMember(dest => dest.ItemName, opt => opt.MapFrom(src => src.Product != null ? src.Product.ItemName : null));
        CreateMap<SaleReturn, SaleReturnDto>()
            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items));
        CreateMap<SaleReturnItem, SaleReturnItemDto>()
            .ForMember(dest => dest.ItemName, opt => opt.MapFrom(src => src.Product != null ? src.Product.ItemName : null));

        // Payment
        CreateMap<Payment, PaymentDto>();

        // Expense
        CreateMap<ExpenseCategory, ExpenseCategoryDto>();
        CreateMap<Expense, ExpenseDto>()
            .ForMember(dest => dest.BranchName, opt => opt.MapFrom(src => src.Branch != null ? src.Branch.BranchName : null))
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.CategoryName : null))
            .ForMember(dest => dest.PaidByName, opt => opt.MapFrom(src => src.PaidByUser != null ? src.PaidByUser.FullName : null));

        // Damage item
        CreateMap<DamageItem, DamageItemDto>()
            .ForMember(dest => dest.ItemName, opt => opt.MapFrom(src => src.Product != null ? src.Product.ItemName : null))
            .ForMember(dest => dest.BranchName, opt => opt.MapFrom(src => src.Branch != null ? src.Branch.BranchName : null))
            .ForMember(dest => dest.WarehouseName, opt => opt.MapFrom(src => src.Warehouse != null ? src.Warehouse.WarehouseName : null))
            .ForMember(dest => dest.ReportedByName, opt => opt.MapFrom(src => src.ReportedByUser != null ? src.ReportedByUser.FullName : null));

        // Item log
        CreateMap<ItemLog, ItemLogDto>()
            .ForMember(dest => dest.ItemName, opt => opt.MapFrom(src => src.Product != null ? src.Product.ItemName : null))
            .ForMember(dest => dest.ChangedByName, opt => opt.MapFrom(src => src.ChangedByUser != null ? src.ChangedByUser.FullName : null));

        // Cashier shift
        CreateMap<CashierShift, CashierShiftDto>()
            .ForMember(dest => dest.BranchName, opt => opt.MapFrom(src => src.Branch != null ? src.Branch.BranchName : null))
            .ForMember(dest => dest.CashierName, opt => opt.MapFrom(src => src.Cashier != null ? src.Cashier.FullName : null))
            .ForMember(dest => dest.ClosedByName, opt => opt.MapFrom(src => src.ClosedByUser != null ? src.ClosedByUser.FullName : null));
        CreateMap<CashierShiftHistory, CashierShiftHistoryDto>()
            .ForMember(dest => dest.ChangedByName, opt => opt.MapFrom(src => src.ChangedByUser != null ? src.ChangedByUser.FullName : null));
    }
}