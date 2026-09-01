using AutoMapper;
using PosApi.DTOs.Product;
using PosApi.Exceptions;
using PosApi.Models.Entities;
using PosApi.Repository;
using PosApi.Service.Interfaces;

namespace PosApi.Service;

public class BrandService : IBrandService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<BrandService> _logger;

    public BrandService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<BrandService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IReadOnlyList<BrandDto>> GetAllAsync(bool? isActive = null, CancellationToken cancellationToken = default)
    {
        var brands = await _unitOfWork.Brands.GetAllAsync(isActive, cancellationToken);
        return _mapper.Map<IReadOnlyList<BrandDto>>(brands);
    }

    public async Task<BrandDto> GetByIdAsync(int brandId, CancellationToken cancellationToken = default)
    {
        var brand = await _unitOfWork.Brands.GetByIdAsync(brandId, cancellationToken)
            ?? throw new NotFoundException("Brand", brandId);

        return _mapper.Map<BrandDto>(brand);
    }

    public async Task<BrandDto> CreateAsync(CreateBrandDto request, CancellationToken cancellationToken = default)
    {
        var brand = new Brand
        {
            BrandName = request.BrandName.Trim(),
            Description = request.Description,
            IsActive = request.IsActive
        };

        await _unitOfWork.Brands.AddAsync(brand, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Brand {BrandName} created with id {BrandId}", brand.BrandName, brand.BrandId);

        return _mapper.Map<BrandDto>(brand);
    }

    public async Task<BrandDto> UpdateAsync(int brandId, UpdateBrandDto request, CancellationToken cancellationToken = default)
    {
        var brand = await _unitOfWork.Brands.GetByIdAsync(brandId, cancellationToken)
            ?? throw new NotFoundException("Brand", brandId);

        brand.BrandName = request.BrandName.Trim();
        brand.Description = request.Description;
        brand.IsActive = request.IsActive;

        _unitOfWork.Brands.Update(brand);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Brand {BrandId} updated successfully", brandId);

        return _mapper.Map<BrandDto>(brand);
    }

    public async Task DeleteAsync(int brandId, CancellationToken cancellationToken = default)
    {
        var brand = await _unitOfWork.Brands.GetByIdAsync(brandId, cancellationToken)
            ?? throw new NotFoundException("Brand", brandId);

        if (await _unitOfWork.Brands.HasProductsAsync(brandId, cancellationToken))
        {
            throw new ConflictException($"Brand '{brand.BrandName}' has products assigned to it and cannot be deleted.");
        }

        _unitOfWork.Brands.Remove(brand);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Brand {BrandId} deleted successfully", brandId);
    }
}
