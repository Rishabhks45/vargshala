using Vargshala.Contracts.Common;
using Vargshala.Contracts.EmailTemplates;

namespace Vargshala.Web.Services;

public interface IEmailTemplateService
{
    Task<List<EmailTemplateDto>> GetAllTemplatesAsync(CancellationToken cancellationToken = default);
    Task<EmailTemplateDto?> GetTemplateByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<EmailTemplateDto>> CreateTemplateAsync(CreateEmailTemplateRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<EmailTemplateDto>> UpdateTemplateAsync(UpdateEmailTemplateRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> ToggleTemplateStatusAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> SendTestEmailAsync(SendTestEmailRequest request, CancellationToken cancellationToken = default);
}
