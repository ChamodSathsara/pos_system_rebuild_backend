using AutoMapper;
using PosApi.DTOs.Sale;
using PosApi.Exceptions;
using PosApi.Repository;
using PosApi.Service.Interfaces;

namespace PosApi.Service;

public class SaleItemService : ISaleItemService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public SaleItemService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<SaleItemDto>> GetByInvoiceNoAsync(string invoiceNo, CancellationToken cancellationToken = default)
    {
        var items = await _unitOfWork.SaleItems.GetByInvoiceNoAsync(invoiceNo, cancellationToken);
        return _mapper.Map<IReadOnlyList<SaleItemDto>>(items);
    }

    public async Task<SaleItemDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var item = await _unitOfWork.SaleItems.GetByIdWithDetailsAsync(id, cancellationToken)
            ?? throw new NotFoundException("SaleItem", id);

        return _mapper.Map<SaleItemDto>(item);
    }
}
