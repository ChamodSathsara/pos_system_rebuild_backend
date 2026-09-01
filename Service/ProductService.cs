using AutoMapper;
using PosApi.Constants;
using PosApi.DTOs.Product;
using PosApi.Exceptions;
using PosApi.Helpers;
using PosApi.Models.Entities;
using PosApi.Repository;
using PosApi.Service.Interfaces;

namespace PosApi.Service;

public class ProductService : IProductService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<ProductService> _logger;

    public ProductService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<ProductService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IReadOnlyList<ProductDto>> SearchAsync(
        int? categoryId,
        int? brandId,
        bool? isActive,
        string? keyword,
        CancellationToken cancellationToken = default)
    {
        var products = await _unitOfWork.Products.SearchAsync(categoryId, brandId, isActive, keyword, cancellationToken);
        return _mapper.Map<IReadOnlyList<ProductDto>>(products);
    }

    public async Task<ProductDto> GetByIdAsync(string itemCode, CancellationToken cancellationToken = default)
    {
        var product = await _unitOfWork.Products.GetByIdWithDetailsAsync(itemCode, cancellationToken)
            ?? throw new NotFoundException("ProductMaster", itemCode);

        return _mapper.Map<ProductDto>(product);
    }

    public async Task<ProductDto> CreateAsync(CreateProductDto request, CancellationToken cancellationToken = default)
    {
        var itemCode = request.ItemCode?.Trim();

        if (string.IsNullOrWhiteSpace(itemCode))
        {
            itemCode = await _unitOfWork.Products.GenerateNextItemCodeAsync(cancellationToken);
        }
        else if (await _unitOfWork.Products.ItemCodeExistsAsync(itemCode, cancellationToken))
        {
            throw new ConflictException($"A product with item code '{itemCode}' already exists.");
        }

        await ValidateReferencesAsync(request.CategoryId, request.BrandId, request.TaxCode, cancellationToken);

        if (!string.IsNullOrWhiteSpace(request.Barcode) &&
            await _unitOfWork.Products.BarcodeExistsAsync(request.Barcode, cancellationToken: cancellationToken))
        {
            throw new ConflictException($"Barcode '{request.Barcode}' is already assigned to another product.");
        }

        var product = new ProductMaster
        {
            ItemCode = itemCode,
            ItemName = request.ItemName.Trim(),
            Description = request.Description,
            CategoryId = request.CategoryId,
            BrandId = request.BrandId,
            UnitOfMeasure = request.UnitOfMeasure,
            ItemGroup = request.ItemGroup,
            Barcode = request.Barcode,
            CostPrice = request.CostPrice,
            SellingPrice = request.SellingPrice,
            ReorderLevel = request.ReorderLevel,
            TaxCode = request.TaxCode,
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Products.AddAsync(product, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Product {ItemCode} created successfully", product.ItemCode);

        return await GetByIdAsync(product.ItemCode, cancellationToken);
    }

    public async Task<ProductDto> UpdateAsync(string itemCode, UpdateProductDto request, string changedBy, CancellationToken cancellationToken = default)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(itemCode, cancellationToken)
            ?? throw new NotFoundException("ProductMaster", itemCode);

        await ValidateReferencesAsync(request.CategoryId, request.BrandId, request.TaxCode, cancellationToken);

        if (!string.IsNullOrWhiteSpace(request.Barcode) &&
            await _unitOfWork.Products.BarcodeExistsAsync(request.Barcode, itemCode, cancellationToken))
        {
            throw new ConflictException($"Barcode '{request.Barcode}' is already assigned to another product.");
        }

        // ---- Snapshot the previous values so the automatic item_log entries below can record
        // a meaningful before/after, before anything is overwritten. ----
        var previousCostPrice = product.CostPrice;
        var previousSellingPrice = product.SellingPrice;
        var priceChanged = previousCostPrice != request.CostPrice || previousSellingPrice != request.SellingPrice;

        var fieldChanges = new List<string>();
        void TrackChange(string field, object? oldValue, object? newValue)
        {
            if (!Equals(oldValue, newValue))
            {
                fieldChanges.Add($"{field}: '{oldValue}' -> '{newValue}'");
            }
        }

        var newItemName = request.ItemName.Trim();
        TrackChange(nameof(product.ItemName), product.ItemName, newItemName);
        TrackChange(nameof(product.Description), product.Description, request.Description);
        TrackChange(nameof(product.CategoryId), product.CategoryId, request.CategoryId);
        TrackChange(nameof(product.BrandId), product.BrandId, request.BrandId);
        TrackChange(nameof(product.UnitOfMeasure), product.UnitOfMeasure, request.UnitOfMeasure);
        TrackChange(nameof(product.ItemGroup), product.ItemGroup, request.ItemGroup);
        TrackChange(nameof(product.Barcode), product.Barcode, request.Barcode);
        TrackChange(nameof(product.ReorderLevel), product.ReorderLevel, request.ReorderLevel);
        TrackChange(nameof(product.TaxCode), product.TaxCode, request.TaxCode);
        TrackChange(nameof(product.IsActive), product.IsActive, request.IsActive);

        product.ItemName = newItemName;
        product.Description = request.Description;
        product.CategoryId = request.CategoryId;
        product.BrandId = request.BrandId;
        product.UnitOfMeasure = request.UnitOfMeasure;
        product.ItemGroup = request.ItemGroup;
        product.Barcode = request.Barcode;
        product.CostPrice = request.CostPrice;
        product.SellingPrice = request.SellingPrice;
        product.ReorderLevel = request.ReorderLevel;
        product.TaxCode = request.TaxCode;
        product.IsActive = request.IsActive;
        product.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Products.Update(product);

        // ---- item_log: one entry for the general product update, and a separate one for the
        // price change, mirroring how they're treated as distinct triggers. ----
        if (fieldChanges.Count > 0)
        {
            await _unitOfWork.ItemLogs.AddAsync(
                ItemLogFactory.Create(itemCode, ItemLogActions.ProductUpdated, null, string.Join("; ", fieldChanges), changedBy),
                cancellationToken);
        }

        if (priceChanged)
        {
            await _unitOfWork.ItemLogs.AddAsync(
                ItemLogFactory.Create(
                    itemCode,
                    ItemLogActions.PriceChanged,
                    $"CostPrice: {previousCostPrice}, SellingPrice: {previousSellingPrice}",
                    $"CostPrice: {request.CostPrice}, SellingPrice: {request.SellingPrice}",
                    changedBy),
                cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Product {ItemCode} updated successfully", itemCode);

        return await GetByIdAsync(itemCode, cancellationToken);
    }

    public async Task DeleteAsync(string itemCode, CancellationToken cancellationToken = default)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(itemCode, cancellationToken)
            ?? throw new NotFoundException("ProductMaster", itemCode);

        if (await _unitOfWork.Products.HasStockAsync(itemCode, cancellationToken))
        {
            throw new ConflictException($"Product '{itemCode}' has stock records referencing it and cannot be deleted. Deactivate it instead.");
        }

        _unitOfWork.Products.Remove(product);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Product {ItemCode} deleted successfully", itemCode);
    }

    private async Task ValidateReferencesAsync(int? categoryId, int? brandId, string? taxCode, CancellationToken cancellationToken)
    {
        if (categoryId.HasValue && await _unitOfWork.Categories.GetByIdAsync(categoryId.Value, cancellationToken) is null)
        {
            throw new BadRequestException($"Category {categoryId} does not exist.");
        }

        if (brandId.HasValue && await _unitOfWork.Brands.GetByIdAsync(brandId.Value, cancellationToken) is null)
        {
            throw new BadRequestException($"Brand {brandId} does not exist.");
        }

        if (!string.IsNullOrWhiteSpace(taxCode) && await _unitOfWork.TaxMasters.GetByIdAsync(taxCode, cancellationToken) is null)
        {
            throw new BadRequestException($"Tax code '{taxCode}' does not exist.");
        }
    }
}
