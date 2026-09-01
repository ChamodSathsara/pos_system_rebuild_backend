using AutoMapper;
using PosApi.DTOs.Grn;
using PosApi.Exceptions;
using PosApi.Repository;
using PosApi.Service.Interfaces;

namespace PosApi.Service;

public class GrnItemService : IGrnItemService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GrnItemService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<GrnItemDto>> GetByGrnIdAsync(int grnId, CancellationToken cancellationToken = default)
    {
        var items = await _unitOfWork.GrnItems.GetByGrnIdAsync(grnId, cancellationToken);
        return _mapper.Map<IReadOnlyList<GrnItemDto>>(items);
    }

    public async Task<GrnItemDto> GetByIdAsync(int grnItemId, CancellationToken cancellationToken = default)
    {
        var item = await _unitOfWork.GrnItems.GetByIdWithDetailsAsync(grnItemId, cancellationToken)
            ?? throw new NotFoundException("GrnItem", grnItemId);

        return _mapper.Map<GrnItemDto>(item);
    }
}
