using PosApi.Models.Entities;

namespace PosApi.Repository;

public interface IGrnMasterRepository : IGenericRepository<GrnMaster, int>
{
    Task<bool> GrnNoExistsAsync(string grnNo, CancellationToken cancellationToken = default);

    /// <summary>Returns the next sequential GRN number (e.g. "GRN000001", "GRN000002", ...) for use when the caller does not supply one explicitly.</summary>
    Task<string> GenerateNextGrnNoAsync(CancellationToken cancellationToken = default);

    Task<GrnMaster?> GetByIdWithDetailsAsync(int grnId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<GrnMaster>> SearchAsync(
        string? poNo,
        int? vendorId,
        string? branchCode,
        string? warehouseCode,
        DateTime? fromDate,
        DateTime? toDate,
        CancellationToken cancellationToken = default);

    Task<bool> HasReturnsAsync(int grnId, CancellationToken cancellationToken = default);
}
