namespace Vargshala.Contracts.Fees;

public class FeeReceiptPdfModel
{
    public string ReceiptNumber { get; set; } = string.Empty;
    public DateTime PaymentDate { get; set; }
    public string PaymentMethod { get; set; } = "CASH";
    public string? TransactionReference { get; set; }

    public string StudentName { get; set; } = string.Empty;
    public string? StudentRollNumber { get; set; }
    public string? StudentCode { get; set; }
    public string? ClassName { get; set; }
    public string? BranchName { get; set; }
    public string AcademicSession { get; set; } = "2026 – 2027";

    public string InstituteName { get; set; } = "Coaching Institute";
    public string? InstituteTagline { get; set; } = "Premier Coaching & Academic Excellence Center";
    public string? InstituteAddress { get; set; }
    public string? Helpline { get; set; }
    public string? RegistrationNumber { get; set; }
    public string? LogoUrl { get; set; }
    public byte[]? LogoBytes { get; set; }
    public string? LogoSvg { get; set; }

    public decimal TotalAmount { get; set; }
    public string AmountInWords { get; set; } = string.Empty;
    public string? Remarks { get; set; }

    public List<FeeReceiptItemDto> Items { get; set; } = new();
}

public class FeeReceiptItemDto
{
    public int ItemIndex { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? DueDate { get; set; }
    public decimal Amount { get; set; }
}

public class FeeReceiptPdfResult
{
    public byte[] FileBytes { get; set; } = Array.Empty<byte>();
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = "application/pdf";
}
