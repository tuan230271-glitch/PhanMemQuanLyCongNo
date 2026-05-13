using PhanMemQuanLyCongNo.Domain.Entities;
using PhanMemQuanLyCongNo.Domain.Enums;

namespace PhanMemQuanLyCongNo.Domain.Interfaces;

public interface ICongNoRepository
{
    IReadOnlyCollection<CongNo> GetByTenant(Guid tenantId, string? status = null);
    CongNo? GetById(Guid tenantId, Guid id);
    CongNo Add(CongNo congNo);
    void Update(CongNo congNo);
}
