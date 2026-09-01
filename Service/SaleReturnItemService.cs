using AutoMapper;
using PosApi.DTOs.Sale;
using PosApi.Exceptions;
using PosApi.Repository;
using PosApi.Service.Interfaces;

namespace PosApi.Service;

public class SaleReturnItemService : ISaleReturnItemService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public SaleReturnItemService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<SaleReturnItemDto>> GetByReturnNoAsync(string returnNo, CancellationToken cancellationToken = default)
    {
        var items = await _unitOfWork.SaleReturnItems.GetByReturnNoAsync(returnNo, cancellationToken);
        return _mapper.Map<IReadOnlyList<SaleReturnItemDto>>(items);
    }

    public async Task<SaleReturnItemDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var item = await _unitOfWork.SaleReturnItems.GetByIdWithDetailsAsync(id, cancellationToken)
            ?? throw new NotFoundException("SaleReturnItem", id);

        return _mapper.Map<SaleReturnItemDto>(item);
    }
}
