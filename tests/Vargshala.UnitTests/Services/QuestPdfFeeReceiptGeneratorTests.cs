using FluentAssertions;
using Vargshala.Contracts.Fees;
using Vargshala.Infrastructure.Services.Receipts;
using Xunit;

namespace Vargshala.UnitTests.Services;

public class QuestPdfFeeReceiptGeneratorTests
{
    [Fact]
    public void GenerateReceiptPdf_WithDefaultLogoSvg_ShouldGenerateValidPdf()
    {
        // Arrange
        var generator = new QuestPdfFeeReceiptGenerator();
        var model = new FeeReceiptPdfModel
        {
            ReceiptNumber = "REC-202609-0022",
            PaymentDate = new DateTime(2026, 9, 14, 11, 4, 0),
            PaymentMethod = "CASH",
            TransactionReference = "CASH / COUNTER",
            StudentName = "Rishumi Sharma",
            StudentCode = "2600002",
            StudentRollNumber = "2600002",
            ClassName = "Coaching Program",
            AcademicSession = "2026 – 2027",
            BranchName = "Town Hall Campus",
            InstituteName = "Apex Coaching Academy",
            InstituteTagline = "Branch Academic & Coaching Center",
            RegistrationNumber = "APEX-2026/ACAD",
            Helpline = "+91 98765 43210",
            TotalAmount = 1000.00m,
            AmountInWords = "One Thousand Rupees Only",
            Items = new List<FeeReceiptItemDto>
            {
                new()
                {
                    Description = "Installment 1 Fee",
                    DueDate = "21 Sep 2026",
                    Amount = 1000.00m
                }
            }
        };

        // Act
        var pdfBytes = generator.GenerateReceiptPdf(model);

        // Assert
        pdfBytes.Should().NotBeNull();
        pdfBytes.Length.Should().BeGreaterThan(1000);

        // Verify PDF Header magic bytes (%PDF)
        var header = System.Text.Encoding.ASCII.GetString(pdfBytes, 0, 4);
        header.Should().Be("%PDF");

        // Save to scratch for visual inspection
        var scratchPath = @"d:\Rishabhks45\vargshala\scratch";
        if (Directory.Exists(scratchPath))
        {
            File.WriteAllBytes(Path.Combine(scratchPath, "test_receipt_apex.pdf"), pdfBytes);
        }
    }

    [Fact]
    public void GenerateReceiptPdf_WithVargshalaLogoSvg_ShouldGenerateValidPdf()
    {
        // Arrange
        var generator = new QuestPdfFeeReceiptGenerator();
        var logoSvgPath = @"d:\Rishabhks45\vargshala\src\Vargshala.Web\wwwroot\images\Logo.svg";
        var logoSvg = File.ReadAllText(logoSvgPath);

        var model = new FeeReceiptPdfModel
        {
            ReceiptNumber = "REC-202609-0022",
            PaymentDate = new DateTime(2026, 9, 14, 11, 4, 0),
            PaymentMethod = "CASH",
            TransactionReference = "CASH / COUNTER",
            StudentName = "Rishumi Sharma",
            StudentCode = "2600002",
            StudentRollNumber = "2600002",
            ClassName = "Coaching Program",
            AcademicSession = "2026 – 2027",
            BranchName = "Town Hall Campus",
            InstituteName = "VARGSHALA INSTITUTE",
            InstituteTagline = "Branch Academic & Coaching Center",
            RegistrationNumber = "VARG-2026/ACAD",
            Helpline = "+91 98765 43210",
            LogoSvg = logoSvg,
            TotalAmount = 1000.00m,
            AmountInWords = "One Thousand Rupees Only",
            Items = new List<FeeReceiptItemDto>
            {
                new()
                {
                    Description = "Installment 1 Fee",
                    DueDate = "21 Sep 2026",
                    Amount = 1000.00m
                }
            }
        };

        // Act
        var pdfBytes = generator.GenerateReceiptPdf(model);

        // Assert
        pdfBytes.Should().NotBeNull();
        pdfBytes.Length.Should().BeGreaterThan(1000);

        var scratchPath = @"d:\Rishabhks45\vargshala\scratch";
        if (Directory.Exists(scratchPath))
        {
            File.WriteAllBytes(Path.Combine(scratchPath, "test_receipt_logo_svg.pdf"), pdfBytes);
        }
    }

    [Fact]
    public void GenerateReceiptPdf_WhenModelHasLogoBytesFromUploadedOrgLogo_ShouldStillRenderDefaultLogoSvg()
    {
        // Arrange: Model has raster LogoBytes (like the plant-books image in the database)
        var generator = new QuestPdfFeeReceiptGenerator();
        var model = new FeeReceiptPdfModel
        {
            ReceiptNumber = "REC-202609-0022",
            PaymentDate = new DateTime(2026, 9, 14, 11, 4, 0),
            PaymentMethod = "CASH",
            TransactionReference = "CASH / COUNTER",
            StudentName = "Rishumi Sharma",
            StudentCode = "2600002",
            StudentRollNumber = "2600002",
            ClassName = "Coaching Program",
            AcademicSession = "2026 – 2027",
            BranchName = "Town Hall Campus",
            InstituteName = "Apex Coaching Academy",
            InstituteTagline = "Branch Academic & Coaching Center",
            RegistrationNumber = "APEX-2026/ACAD",
            Helpline = "+91 98765 43210",
            LogoBytes = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }, // Dummy PNG bytes
            TotalAmount = 1000.00m,
            AmountInWords = "One Thousand Rupees Only",
            Items = new List<FeeReceiptItemDto>
            {
                new()
                {
                    Description = "Installment 1 Fee",
                    DueDate = "21 Sep 2026",
                    Amount = 1000.00m
                }
            }
        };

        // Act
        var pdfBytes = generator.GenerateReceiptPdf(model);

        // Assert: Must generate valid PDF with Logo.svg emblem, ignoring raster bytes
        pdfBytes.Should().NotBeNull();
        pdfBytes.Length.Should().BeGreaterThan(1000);

        var header = System.Text.Encoding.ASCII.GetString(pdfBytes, 0, 4);
        header.Should().Be("%PDF");
    }
}
