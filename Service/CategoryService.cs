using AutoMapper;
using PosApi.DTOs.Product;
using PosApi.Exceptions;
using PosApi.Models.Entities;
using PosApi.Repository;
using PosApi.Service.Interfaces;

namespace PosApi.Service;

public class CategoryService : ICategoryService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<CategoryService> _logger;

    public CategoryService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<CategoryService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IReadOnlyList<CategoryDto>> GetAllAsync(bool? isActive = null, CancellationToken cancellationToken = default)
    {
        var categories = await _unitOfWork.Categories.GetAllAsync(isActive, cancellationToken);
        var dtos = _mapper.Map<List<CategoryDto>>(categories);

        var namesById = categories.ToDictionary(c => c.CategoryId, c => c.CategoryName);
        foreach (var dto in dtos)
        {
            if (dto.ParentCategoryId.HasValue && namesById.TryGetValue(dto.ParentCategoryId.Value, out var parentName))
            {
                dto.ParentCategoryName = parentName;
            }
        }

        return dtos;
    }

    public async Task<CategoryDto> GetByIdAsync(int categoryId, CancellationToken cancellationToken = default)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(categoryId, cancellationToken)
            ?? throw new NotFoundException("Category", categoryId);

        var dto = _mapper.Map<CategoryDto>(category);

        if (category.ParentCategoryId.HasValue)
        {
            var parent = await _unitOfWork.Categories.GetByIdAsync(category.ParentCategoryId.Value, cancellationToken);
            dto.ParentCategoryName = parent?.CategoryName;
        }

        return dto;
    }

    public async Task<CategoryDto> CreateAsync(CreateCategoryDto request, CancellationToken cancellationToken = default)
    {
        if (request.ParentCategoryId.HasValue &&
            await _unitOfWork.Categories.GetByIdAsync(request.ParentCategoryId.Value, cancellationToken) is null)
        {
            throw new BadRequestException($"Parent category {request.ParentCategoryId} does not exist.");
        }

        var category = new Category
        {
            CategoryName = request.CategoryName.Trim(),
            ParentCategoryId = request.ParentCategoryId,
            Description = request.Description,
            IsActive = request.IsActive
        };

        await _unitOfWork.Categories.AddAsync(category, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Category {CategoryName} created with id {CategoryId}", category.CategoryName, category.CategoryId);

        return await GetByIdAsync(category.CategoryId, cancellationToken);
    }

    public async Task<CategoryDto> UpdateAsync(int categoryId, UpdateCategoryDto request, CancellationToken cancellationToken = default)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(categoryId, cancellationToken)
            ?? throw new NotFoundException("Category", categoryId);

        if (request.ParentCategoryId == categoryId)
        {
            throw new BadRequestException("A category cannot be its own parent.");
        }

        if (request.ParentCategoryId.HasValue &&
            await _unitOfWork.Categories.GetByIdAsync(request.ParentCategoryId.Value, cancellationToken) is null)
        {
            throw new BadRequestException($"Parent category {request.ParentCategoryId} does not exist.");
        }

        category.CategoryName = request.CategoryName.Trim();
        category.ParentCategoryId = request.ParentCategoryId;
        category.Description = request.Description;
        category.IsActive = request.IsActive;

        _unitOfWork.Categories.Update(category);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Category {CategoryId} updated successfully", categoryId);

        return await GetByIdAsync(categoryId, cancellationToken);
    }

    public async Task DeleteAsync(int categoryId, CancellationToken cancellationToken = default)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(categoryId, cancellationToken)
            ?? throw new NotFoundException("Category", categoryId);

        if (await _unitOfWork.Categories.HasChildCategoriesAsync(categoryId, cancellationToken))
        {
            throw new ConflictException($"Category '{category.CategoryName}' has child categories and cannot be deleted.");
        }

        if (await _unitOfWork.Categories.HasProductsAsync(categoryId, cancellationToken))
        {
            throw new ConflictException($"Category '{category.CategoryName}' has products assigned to it and cannot be deleted.");
        }

        _unitOfWork.Categories.Remove(category);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Category {CategoryId} deleted successfully", categoryId);
    }
}
