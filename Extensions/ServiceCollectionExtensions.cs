using System.Text;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PosApi.Configuration;
using PosApi.Data;
using PosApi.Data.Repositories;
using PosApi.Repository;
using PosApi.Security;
using PosApi.Service;
using PosApi.Service.Interfaces;
using Microsoft.OpenApi;

namespace PosApi.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString, sql =>
            {
                sql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                sql.EnableRetryOnFailure(maxRetryCount: 3);
            }));

        return services;
    }

    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<ICompanyRepository, CompanyRepository>();
        services.AddScoped<IBranchRepository, BranchRepository>();
        services.AddScoped<IWarehouseRepository, WarehouseRepository>();
        services.AddScoped<IUserRoleRepository, UserRoleRepository>();
        services.AddScoped<IPermissionRepository, PermissionRepository>();
        services.AddScoped<IUserRolePermissionRepository, UserRolePermissionRepository>();

        services.AddScoped<IVendorRepository, VendorRepository>();
        services.AddScoped<IVendorLedgerRepository, VendorLedgerRepository>();

        services.AddScoped<IStockInventoryRepository, StockInventoryRepository>();
        services.AddScoped<IStockBatchRepository, StockBatchRepository>();
        services.AddScoped<IStockMovementRepository, StockMovementRepository>();

        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IBrandRepository, BrandRepository>();
        services.AddScoped<ITaxMasterRepository, TaxMasterRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();

        services.AddScoped<IPurchaseOrderRepository, PurchaseOrderRepository>();
        services.AddScoped<IPurchaseOrderItemRepository, PurchaseOrderItemRepository>();
        services.AddScoped<IPurchaseOrderHistoryRepository, PurchaseOrderHistoryRepository>();
        services.AddScoped<IPurchaseOrderHistoryChangeRepository, PurchaseOrderHistoryChangeRepository>();

        services.AddScoped<IGrnMasterRepository, GrnMasterRepository>();
        services.AddScoped<IGrnItemRepository, GrnItemRepository>();
        services.AddScoped<IGrnReturnRepository, GrnReturnRepository>();
        services.AddScoped<IGrnReturnItemRepository, GrnReturnItemRepository>();

        services.AddScoped<IDiscountRepository, DiscountRepository>();

        services.AddScoped<ISaleRepository, SaleRepository>();
        services.AddScoped<ISaleItemRepository, SaleItemRepository>();
        services.AddScoped<ISaleReturnRepository, SaleReturnRepository>();
        services.AddScoped<ISaleReturnItemRepository, SaleReturnItemRepository>();

        services.AddScoped<IPaymentRepository, PaymentRepository>();

        services.AddScoped<IExpenseCategoryRepository, ExpenseCategoryRepository>();
        services.AddScoped<IExpenseRepository, ExpenseRepository>();

        services.AddScoped<IDamageItemRepository, DamageItemRepository>();
        services.AddScoped<IItemLogRepository, ItemLogRepository>();

        services.AddScoped<ICashierShiftRepository, CashierShiftRepository>();
        services.AddScoped<ICashierShiftHistoryRepository, CashierShiftHistoryRepository>();

        services.AddScoped<ISalesReportRepository, SalesReportRepository>();

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }

    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ICustomerService, CustomerService>();

        services.AddScoped<ICompanyService, CompanyService>();
        services.AddScoped<IBranchService, BranchService>();
        services.AddScoped<IWarehouseService, WarehouseService>();

        services.AddScoped<IUserRoleService, UserRoleService>();
        services.AddScoped<IPermissionService, PermissionService>();
        services.AddScoped<IUserRolePermissionService, UserRolePermissionService>();
        services.AddScoped<ISystemUserService, SystemUserService>();

        services.AddScoped<IVendorService, VendorService>();
        services.AddScoped<IVendorLedgerService, VendorLedgerService>();

        services.AddScoped<IStockInventoryService, StockInventoryService>();
        services.AddScoped<IStockBatchService, StockBatchService>();
        services.AddScoped<IStockMovementService, StockMovementService>();
        services.AddScoped<IOpeningStockService, OpeningStockService>();

        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IBrandService, BrandService>();
        services.AddScoped<ITaxMasterService, TaxMasterService>();
        services.AddScoped<IProductService, ProductService>();

        services.AddScoped<IPurchaseOrderService, PurchaseOrderService>();
        services.AddScoped<IPurchaseOrderItemService, PurchaseOrderItemService>();
        services.AddScoped<IPurchaseOrderHistoryService, PurchaseOrderHistoryService>();
        services.AddScoped<IPurchaseOrderHistoryChangeService, PurchaseOrderHistoryChangeService>();

        services.AddScoped<IGrnMasterService, GrnMasterService>();
        services.AddScoped<IGrnItemService, GrnItemService>();
        services.AddScoped<IGrnReturnService, GrnReturnService>();
        services.AddScoped<IGrnReturnItemService, GrnReturnItemService>();

        services.AddScoped<IDiscountService, DiscountService>();

        services.AddScoped<ISaleService, SaleService>();
        services.AddScoped<ISaleItemService, SaleItemService>();
        services.AddScoped<ISaleReturnService, SaleReturnService>();
        services.AddScoped<ISaleReturnItemService, SaleReturnItemService>();

        services.AddScoped<IPaymentService, PaymentService>();

        services.AddScoped<IExpenseCategoryService, ExpenseCategoryService>();
        services.AddScoped<IExpenseService, ExpenseService>();

        services.AddScoped<IDamageItemService, DamageItemService>();
        services.AddScoped<IItemLogService, ItemLogService>();

        services.AddScoped<ICashierShiftService, CashierShiftService>();

        services.AddScoped<ISalesReportService, SalesReportService>();

        services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();
        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();

        services.AddScoped<IPosTerminalService,PosTerminalService>();

        services.AddAutoMapper(typeof(Mappings.MappingProfile).Assembly);

        return services;
    }

    public static IServiceCollection AddValidatorsAndFluentValidation(this IServiceCollection services)
    {
        services.AddFluentValidationAutoValidation();
        services.AddValidatorsFromAssembly(typeof(Program).Assembly);

        return services;
    }

    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtSection = configuration.GetSection(JwtSettings.SectionName);
        services.Configure<JwtSettings>(jwtSection);

        var jwtSettings = jwtSection.Get<JwtSettings>()
            ?? throw new InvalidOperationException("JWT settings section is missing from configuration.");

        if (string.IsNullOrWhiteSpace(jwtSettings.Secret))
        {
            throw new InvalidOperationException("Jwt:Secret must be configured (use user-secrets or environment variables in production).");
        }

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = true;
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidateAudience = true,
                ValidAudience = jwtSettings.Audience,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret)),
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(1)
            };

            options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
                {
                    if (context.Exception is SecurityTokenExpiredException)
                    {
                        context.Response.Headers.Append("Token-Expired", "true");
                    }
                    return Task.CompletedTask;
                }
            };
        });

        services.AddAuthorization();

        return services;
    }

    public static IServiceCollection AddSwaggerWithJwt(
        this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();

        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "POS System API",
                Version = "v1",
                Description = "POS System Backend API"
            });
        });

        return services;
    }
}