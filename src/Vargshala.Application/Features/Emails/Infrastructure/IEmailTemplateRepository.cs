using Vargshala.Contracts.Common;
using Vargshala.Domain.Entities;

namespace Vargshala.Application.Features.Emails.Infrastructure;

#region Interface
public interface IEmailTemplateRepository
{
    Task<EmailTemplate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<EmailTemplate?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default);
    Task<EmailTemplate?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<bool> ExistsByCodeAsync(string code, Guid? excludeId = null, CancellationToken cancellationToken = default);
    Task<(List<EmailTemplate> Items, int TotalRecords)> GetTemplatesPagedAsync(
        PagedRequest request,
        EmailTemplateCategory? category = null,
        bool? isActive = null,
        CancellationToken cancellationToken = default);
    Task AddAsync(EmailTemplate template, CancellationToken cancellationToken = default);
    void Update(EmailTemplate template);
    void Delete(EmailTemplate template);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
#endregion
