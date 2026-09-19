using MediatR;
using Vargshala.Application.Abstractions.CurrentUser;
using Vargshala.Application.Abstractions.Pdf;
using Vargshala.Application.Features.Organizations.Infrastructure;
using Vargshala.Application.Features.OrgAdmin.Fees.Infrastructure;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.Fees;

namespace Vargshala.Application.Features.OrgAdmin.Fees.Queries.GetFeeReceiptPdf;

public record GetStudentFeeReceiptPdfQuery(Guid StudentFeeId) : IRequest<ApiResponse<FeeReceiptPdfResult>>;

public record GetPaymentReceiptPdfQuery(Guid PaymentId) : IRequest<ApiResponse<FeeReceiptPdfResult>>;

public class GetStudentFeeReceiptPdfQueryHandler : IRequestHandler<GetStudentFeeReceiptPdfQuery, ApiResponse<FeeReceiptPdfResult>>
{
    private readonly IFeeRepository _feeRepository;
    private readonly IOrganizationRepository _organizationRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IFeeReceiptPdfGenerator _pdfGenerator;

    public GetStudentFeeReceiptPdfQueryHandler(
        IFeeRepository feeRepository,
        IOrganizationRepository organizationRepository,
        ICurrentUser currentUser,
        IFeeReceiptPdfGenerator pdfGenerator)
    {
        _feeRepository = feeRepository;
        _organizationRepository = organizationRepository;
        _currentUser = currentUser;
        _pdfGenerator = pdfGenerator;
    }

    public async Task<ApiResponse<FeeReceiptPdfResult>> Handle(GetStudentFeeReceiptPdfQuery query, CancellationToken cancellationToken)
    {
        var orgId = _currentUser.OrganizationId;
        if (!orgId.HasValue || orgId.Value == Guid.Empty)
        {
            return ApiResponse<FeeReceiptPdfResult>.FailureResponse("No active organization context found.");
        }

        var fee = await _feeRepository.GetStudentFeeDetailByIdAsync(query.StudentFeeId, orgId.Value, cancellationToken);
        if (fee == null)
        {
            return ApiResponse<FeeReceiptPdfResult>.FailureResponse("Student fee record not found.");
        }

        var org = await _organizationRepository.GetByIdAsync(orgId.Value, cancellationToken);
        var payments = await _feeRepository.GetPaymentsByStudentFeeIdAsync(query.StudentFeeId, orgId.Value, cancellationToken);
        var latestPayment = payments.OrderByDescending(p => p.PaymentDate).FirstOrDefault();

        var student = fee.Student;
        var studentName = student?.User != null ? $"{student.User.FirstName} {student.User.LastName}".Trim() : "Student";
        var branch = fee.FeeStructure?.Branch;
        var branchName = branch?.Name ?? "Main Campus";
        var className = fee.FeeStructure?.Class?.Name ?? "General Academic Course";

        var instituteName = !string.IsNullOrWhiteSpace(org?.Name)
            ? org.Name
            : (!string.IsNullOrWhiteSpace(branch?.Name) ? branch.Name : "Coaching Institute");

        var instituteTagline = !string.IsNullOrWhiteSpace(branch?.Name) && branch.Name != "Main Campus"
            ? $"{branch.Name} Academic & Coaching Center"
            : "Academic & Coaching Excellence Center";

        var helpline = !string.IsNullOrWhiteSpace(branch?.Mobile)
            ? branch.Mobile
            : (!string.IsNullOrWhiteSpace(org?.Mobile) ? org.Mobile : (!string.IsNullOrWhiteSpace(org?.Email) ? org.Email : "+91 98765 43210"));

        var regNo = !string.IsNullOrWhiteSpace(org?.Code) ? org.Code : "REG-ACAD";
        var session = !string.IsNullOrWhiteSpace(org?.AcademicSession) ? org.AcademicSession : "2026 – 2027";

        var addressParts = new[] { branch?.Address ?? org?.Address, branch?.City ?? org?.City, branch?.State ?? org?.State, branch?.Pincode ?? org?.Pincode }
            .Where(p => !string.IsNullOrWhiteSpace(p));
        var address = string.Join(", ", addressParts);

        var logoUrl = branch?.LogoUrl ?? org?.LogoUrl;
        var (logoBytes, logoSvg) = ReceiptLogoHelper.TryResolveLogo(logoUrl);

        var receiptNumber = latestPayment?.ReceiptNumber 
            ?? $"REC-{(string.IsNullOrWhiteSpace(student?.RollNumber) ? "FEE" : student.RollNumber)}-{fee.Id.ToString()[..6].ToUpper()}";

        var totalAmount = latestPayment?.Amount ?? (fee.PaidAmount > 0 ? fee.PaidAmount : fee.FinalAmount);

        var model = new FeeReceiptPdfModel
        {
            ReceiptNumber = receiptNumber,
            PaymentDate = latestPayment?.PaymentDate ?? fee.AssignedAt,
            PaymentMethod = latestPayment?.PaymentMethod ?? "COUNTER / DIRECT",
            TransactionReference = latestPayment?.TransactionReference,
            StudentName = studentName,
            StudentRollNumber = student?.RollNumber,
            StudentCode = student?.StudentCode,
            ClassName = className,
            BranchName = branchName,
            AcademicSession = session,
            InstituteName = instituteName,
            InstituteTagline = instituteTagline,
            InstituteAddress = address,
            Helpline = helpline,
            RegistrationNumber = regNo,
            LogoUrl = logoUrl,
            LogoBytes = logoBytes,
            LogoSvg = logoSvg,
            TotalAmount = totalAmount,
            AmountInWords = NumberToWordsHelper.ToIndianCurrencyWords(totalAmount),
            Remarks = latestPayment?.Remarks ?? $"Fee settlement for {fee.FeeStructure?.Name}"
        };

        if (latestPayment != null && latestPayment.Allocations.Any())
        {
            model.Items = latestPayment.Allocations.Select(a => new FeeReceiptItemDto
            {
                ItemIndex = a.FeeInstallment?.InstallmentNumber ?? 1,
                Description = $"Installment {a.FeeInstallment?.InstallmentNumber} Fee ({fee.FeeStructure?.Name})",
                DueDate = a.FeeInstallment?.DueDate.ToString("dd MMM yyyy"),
                Amount = a.AllocatedAmount
            }).ToList();
        }
        else if (fee.Installments.Any())
        {
            model.Items = fee.Installments.OrderBy(i => i.InstallmentNumber).Select(i => new FeeReceiptItemDto
            {
                ItemIndex = i.InstallmentNumber,
                Description = $"Installment {i.InstallmentNumber} Fee ({fee.FeeStructure?.Name})",
                DueDate = i.DueDate.ToString("dd MMM yyyy"),
                Amount = i.Amount
            }).ToList();
        }
        else
        {
            model.Items.Add(new FeeReceiptItemDto
            {
                ItemIndex = 1,
                Description = $"Academic Coaching & Course Fee ({fee.FeeStructure?.Name})",
                DueDate = fee.AssignedAt.ToString("dd MMM yyyy"),
                Amount = totalAmount
            });
        }

        var pdfBytes = _pdfGenerator.GenerateReceiptPdf(model);
        var result = new FeeReceiptPdfResult
        {
            FileBytes = pdfBytes,
            FileName = $"Receipt_{receiptNumber}.pdf",
            ContentType = "application/pdf"
        };

        return ApiResponse<FeeReceiptPdfResult>.SuccessResponse(result);
    }
}

public class GetPaymentReceiptPdfQueryHandler : IRequestHandler<GetPaymentReceiptPdfQuery, ApiResponse<FeeReceiptPdfResult>>
{
    private readonly IFeeRepository _feeRepository;
    private readonly IOrganizationRepository _organizationRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IFeeReceiptPdfGenerator _pdfGenerator;

    public GetPaymentReceiptPdfQueryHandler(
        IFeeRepository feeRepository,
        IOrganizationRepository organizationRepository,
        ICurrentUser currentUser,
        IFeeReceiptPdfGenerator pdfGenerator)
    {
        _feeRepository = feeRepository;
        _organizationRepository = organizationRepository;
        _currentUser = currentUser;
        _pdfGenerator = pdfGenerator;
    }

    public async Task<ApiResponse<FeeReceiptPdfResult>> Handle(GetPaymentReceiptPdfQuery query, CancellationToken cancellationToken)
    {
        var orgId = _currentUser.OrganizationId;
        if (!orgId.HasValue || orgId.Value == Guid.Empty)
        {
            return ApiResponse<FeeReceiptPdfResult>.FailureResponse("No active organization context found.");
        }

        var payment = await _feeRepository.GetPaymentReceiptByIdAsync(query.PaymentId, orgId.Value, cancellationToken);
        if (payment == null)
        {
            return ApiResponse<FeeReceiptPdfResult>.FailureResponse("Payment record not found.");
        }

        var org = await _organizationRepository.GetByIdAsync(orgId.Value, cancellationToken);
        var student = payment.Student;
        var studentName = student?.User != null ? $"{student.User.FirstName} {student.User.LastName}".Trim() : "Student";
        var branch = payment.Branch;
        var branchName = branch?.Name ?? "Main Campus";
        var receiptNumber = payment.ReceiptNumber ?? $"REC-{payment.Id.ToString()[..8].ToUpper()}";

        var instituteName = !string.IsNullOrWhiteSpace(org?.Name)
            ? org.Name
            : (!string.IsNullOrWhiteSpace(branch?.Name) ? branch.Name : "Coaching Institute");

        var instituteTagline = !string.IsNullOrWhiteSpace(branch?.Name) && branch.Name != "Main Campus"
            ? $"{branch.Name} Academic & Coaching Center"
            : "Academic & Coaching Excellence Center";

        var helpline = !string.IsNullOrWhiteSpace(branch?.Mobile)
            ? branch.Mobile
            : (!string.IsNullOrWhiteSpace(org?.Mobile) ? org.Mobile : (!string.IsNullOrWhiteSpace(org?.Email) ? org.Email : "+91 98765 43210"));

        var regNo = !string.IsNullOrWhiteSpace(org?.Code) ? org.Code : "REG-ACAD";
        var session = !string.IsNullOrWhiteSpace(org?.AcademicSession) ? org.AcademicSession : "2026 – 2027";

        var addressParts = new[] { branch?.Address ?? org?.Address, branch?.City ?? org?.City, branch?.State ?? org?.State, branch?.Pincode ?? org?.Pincode }
            .Where(p => !string.IsNullOrWhiteSpace(p));
        var address = string.Join(", ", addressParts);

        var logoUrl = branch?.LogoUrl ?? org?.LogoUrl;
        var (logoBytes, logoSvg) = ReceiptLogoHelper.TryResolveLogo(logoUrl);

        var model = new FeeReceiptPdfModel
        {
            ReceiptNumber = receiptNumber,
            PaymentDate = payment.PaymentDate,
            PaymentMethod = payment.PaymentMethod,
            TransactionReference = payment.TransactionReference,
            StudentName = studentName,
            StudentRollNumber = student?.RollNumber,
            StudentCode = student?.StudentCode,
            ClassName = "Academic Course",
            BranchName = branchName,
            AcademicSession = session,
            InstituteName = instituteName,
            InstituteTagline = instituteTagline,
            InstituteAddress = address,
            Helpline = helpline,
            RegistrationNumber = regNo,
            LogoUrl = logoUrl,
            LogoBytes = logoBytes,
            LogoSvg = logoSvg,
            TotalAmount = payment.Amount,
            AmountInWords = NumberToWordsHelper.ToIndianCurrencyWords(payment.Amount),
            Remarks = payment.Remarks
        };

        if (payment.Allocations.Any())
        {
            model.Items = payment.Allocations.Select((a, idx) => new FeeReceiptItemDto
            {
                ItemIndex = idx + 1,
                Description = $"Installment {a.FeeInstallment?.InstallmentNumber ?? idx + 1} Fee",
                DueDate = a.FeeInstallment?.DueDate.ToString("dd MMM yyyy"),
                Amount = a.AllocatedAmount
            }).ToList();
        }
        else
        {
            model.Items.Add(new FeeReceiptItemDto
            {
                ItemIndex = 1,
                Description = "Academic Coaching & Tuition Fee Payment",
                DueDate = payment.PaymentDate.ToString("dd MMM yyyy"),
                Amount = payment.Amount
            });
        }

        var pdfBytes = _pdfGenerator.GenerateReceiptPdf(model);
        var result = new FeeReceiptPdfResult
        {
            FileBytes = pdfBytes,
            FileName = $"Receipt_{receiptNumber}.pdf",
            ContentType = "application/pdf"
        };

        return ApiResponse<FeeReceiptPdfResult>.SuccessResponse(result);
    }
}

internal static class ReceiptLogoHelper
{
    public static (byte[]? Bytes, string? Svg) TryResolveLogo(string? logoUrl)
    {
        if (string.IsNullOrWhiteSpace(logoUrl))
            return (null, null);

        try
        {
            // 1. Data URI
            if (logoUrl.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
            {
                var comma = logoUrl.IndexOf(',');
                if (comma > 0)
                {
                    var base64 = logoUrl[(comma + 1)..];
                    return (Convert.FromBase64String(base64), null);
                }
            }

            // 2. Inline SVG
            if (logoUrl.TrimStart().StartsWith("<svg", StringComparison.OrdinalIgnoreCase))
            {
                return (null, logoUrl);
            }

            // 3. Local disk lookup across potential locations
            var cleaned = logoUrl.TrimStart('/', '\\').Replace('/', Path.DirectorySeparatorChar);
            var searchPaths = new List<string>
            {
                Path.Combine(AppContext.BaseDirectory, "wwwroot", cleaned),
                Path.Combine(AppContext.BaseDirectory, cleaned),
                Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", cleaned),
                Path.Combine(Directory.GetCurrentDirectory(), cleaned),
                Path.Combine(Directory.GetCurrentDirectory(), "src", "Vargshala.API", "wwwroot", cleaned)
            };

            foreach (var path in searchPaths)
            {
                if (File.Exists(path))
                {
                    if (path.EndsWith(".svg", StringComparison.OrdinalIgnoreCase))
                    {
                        return (null, File.ReadAllText(path));
                    }
                    return (File.ReadAllBytes(path), null);
                }
            }
        }
        catch
        {
            // Fallback gracefully to monogram
        }

        return (null, null);
    }
}

