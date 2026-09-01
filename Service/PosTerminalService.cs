using PosApi.DTOs.Pos;
using PosApi.Exceptions;
using PosApi.Repository;
using PosApi.Service.Interfaces;

namespace PosApi.Service;

public class PosTerminalService : IPosTerminalService
{
    private readonly IUnitOfWork _unitOfWork;

    public PosTerminalService(
        IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<PosTerminalItemDto>>
        GetItemsAsync(
            string branchCode,
            string? warehouseCode,
            int? categoryId,
            string? keyword,
            bool onlyAvailable,
            CancellationToken cancellationToken = default)
    {
        var normalizedBranchCode = branchCode.Trim();

        var branch = await _unitOfWork.Branches.GetByIdAsync(
            normalizedBranchCode,
            cancellationToken);

        if (branch is null)
        {
            throw new NotFoundException(
                "Branch",
                normalizedBranchCode);
        }

        if (!string.IsNullOrWhiteSpace(warehouseCode))
        {
            var normalizedWarehouseCode =
                warehouseCode.Trim();

            var warehouse =
                await _unitOfWork.Warehouses.GetByIdAsync(
                    normalizedWarehouseCode,
                    cancellationToken);

            if (warehouse is null)
            {
                throw new NotFoundException(
                    "Warehouse",
                    normalizedWarehouseCode);
            }

            if (!string.Equals(
                    warehouse.BranchCode,
                    normalizedBranchCode,
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new BadRequestException(
                    $"Warehouse '{normalizedWarehouseCode}' " +
                    $"does not belong to branch " +
                    $"'{normalizedBranchCode}'.");
            }
        }

        var stockItems =
            await _unitOfWork.StockInventories
                .GetPosItemsAsync(
                    normalizedBranchCode,
                    warehouseCode,
                    categoryId,
                    keyword,
                    onlyAvailable,
                    cancellationToken);

        var result = stockItems
            .GroupBy(stock => stock.ItemCode)
            .Select(group =>
            {
                var firstStock = group.First();
                var product = firstStock.Product!;

                return new PosTerminalItemDto
                {
                    StockId = group.Count() == 1
                        ? firstStock.StockId
                        : 0,

                    ItemCode = product.ItemCode,
                    ItemName = product.ItemName,
                    Description = product.Description,
                    Barcode = product.Barcode,

                    CategoryId = product.CategoryId,
                    CategoryName =
                        product.Category?.CategoryName,

                    BrandId = product.BrandId,
                    BrandName =
                        product.Brand?.BrandName,

                    UnitOfMeasure =
                        product.UnitOfMeasure,

                    ItemGroup = product.ItemGroup,

                    Price =
                        product.SellingPrice ?? 0,

                    AvailableQty =
                        group.Sum(stock =>
                            stock.CurrentQty),

                    ReorderLevel =
                        product.ReorderLevel,

                    TaxCode = product.TaxCode,

                    TaxPercentage =
                        product.Tax?.IsActive == true
                            ? product.Tax.Percentage
                            : 0,

                    BranchCode =
                        normalizedBranchCode,

                    WarehouseCode =
                        string.IsNullOrWhiteSpace(
                            warehouseCode)
                            ? "ALL"
                            : warehouseCode.Trim()
                };
            })
            .OrderBy(item => item.ItemName)
            .ToList();

        return result;
    }
}