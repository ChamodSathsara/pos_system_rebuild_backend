using AutoMapper;
using PosApi.DTOs.Grn;
using PosApi.Exceptions;
using PosApi.Repository;
using PosApi.Service.Interfaces;

namespace PosApi.Service;

public class GrnReturnItemService : IGrnReturnItemService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GrnReturnItemService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<GrnReturnItemDto>> GetByGrnReturnIdAsync(int grnReturnId, CancellationToken cancellationToken = default)
    {
        var items = await _unitOfWork.GrnReturnItems.GetByGrnReturnIdAsync(grnReturnId, cancellationToken);
        return _mapper.Map<IReadOnlyList<GrnReturnItemDto>>(items);
    }

    public async Task<GrnReturnItemDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var item = await _unitOfWork.GrnReturnItems.GetByIdWithDetailsAsync(id, cancellationToken)
            ?? throw new NotFoundException("GrnReturnItem", id);

        return _mapper.Map<GrnReturnItemDto>(item);
    }
}
