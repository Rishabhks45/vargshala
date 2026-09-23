using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Vargshala.Application.Abstractions.Pdf;
using Vargshala.Contracts.Subscriptions;

namespace Vargshala.Infrastructure.Services.Receipts;

public class QuestPdfSubscriptionReceiptGenerator : ISubscriptionReceiptPdfGenerator
{
    private static readonly Color PrimaryTeal = Color.FromHex("#004D40");
    private static readonly Color AccentTeal = Color.FromHex("#009488");
    private static readonly Color Slate900 = Color.FromHex("#0F172A");
    private static readonly Color Slate800 = Color.FromHex("#1E293B");
    private static readonly Color Slate700 = Color.FromHex("#334155");
    private static readonly Color Slate600 = Color.FromHex("#475569");
    private static readonly Color Slate500 = Color.FromHex("#64748B");
    private static readonly Color Slate400 = Color.FromHex("#94A3B8");
    private static readonly Color Slate200 = Color.FromHex("#E2E8F0");
    private static readonly Color Slate100 = Color.FromHex("#F1F5F9");
    private static readonly Color Slate50 = Color.FromHex("#F8FAFC");
    private static readonly Color TableHeaderBg = Color.FromHex("#F0F7F6");
    private static readonly Color TableTotalBg = Color.FromHex("#E0EFEF");
    private static readonly Color PaidBadgeBg = Color.FromHex("#ECFDF5");
    private static readonly Color PaidBadgeBorder = Color.FromHex("#86EFAC");
    private static readonly Color PaidBadgeText = Color.FromHex("#15803D");

    public byte[] GenerateSubscriptionReceiptPdf(SubscriptionPaymentReceiptDto receipt)
    {
        // Ensure QuestPDF community license is active
        QuestPDF.Settings.License = LicenseType.Community;

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(28);
                page.DefaultTextStyle(x => x.FontFamily(Fonts.Arial).FontSize(9).FontColor(Slate800));

                page.Content().Element(c => ComposeReceipt(c, receipt));

                page.Footer().Row(row =>
                {
                    row.RelativeItem().Text(text =>
                    {
                        text.Span("Vargshala SaaS • Multi-tenant Educational Institute Platform • ").FontSize(8).FontColor(Slate400);
                        text.Span("https://vargshala.com").FontSize(8).FontColor(AccentTeal);
                    });
                    row.RelativeItem().AlignRight().Text(text =>
                    {
                        text.Span("Generated on: ").FontSize(8).FontColor(Slate400);
                        text.Span(DateTime.UtcNow.ToString("dd MMM yyyy, hh:mm tt 'UTC'")).FontSize(8).FontColor(Slate500);
                    });
                });
            });
        }).GeneratePdf();
    }

    private void ComposeReceipt(IContainer container, SubscriptionPaymentReceiptDto receipt)
    {
        container.Border(2)
            .BorderColor(PrimaryTeal)
            .CornerRadius(8)
            .Padding(20)
            .Column(col =>
            {
                // 1. Header Bar
                ComposeHeader(col, receipt);

                col.Item().PaddingVertical(10).LineHorizontal(1).LineColor(Slate200);

                // 2. Metadata Bar
                ComposeMetadataBar(col, receipt);

                col.Item().PaddingVertical(10).LineHorizontal(1).LineColor(Slate200);

                // 3. Subscriber Profile Card
                ComposeSubscriberCard(col, receipt);

                col.Item().PaddingVertical(10);

                // 4. Particulars Breakdown Table
                ComposeParticularsTable(col, receipt);

                // 5. Amount in Words Box
                ComposeAmountInWords(col, receipt);

                // 6. Remarks / Notes if present
                if (!string.IsNullOrWhiteSpace(receipt.Remarks))
                {
                    col.Item().PaddingTop(8).Background(Color.FromHex("#FFFBEB"))
                        .Border(1).BorderColor(Color.FromHex("#FDE68A"))
                        .CornerRadius(6).Padding(6).Row(r =>
                        {
                            r.AutoItem().Text("Note: ").FontSize(8).Bold().FontColor(Color.FromHex("#92400E"));
                            r.RelativeItem().Text(receipt.Remarks).FontSize(8).Italic().FontColor(Color.FromHex("#78350F"));
                        });
                }

                // 7. Verification & Terms
                ComposeFooterAndTerms(col);
            });
    }

    private void ComposeHeader(ColumnDescriptor col, SubscriptionPaymentReceiptDto receipt)
    {
        col.Item().Row(row =>
        {
            // Left: Vargshala Branding
            row.RelativeItem().Column(innerCol =>
            {
                innerCol.Item().Row(logoRow =>
                {
                    logoRow.AutoItem().Background(PrimaryTeal).Padding(4).CornerRadius(6)
                        .Text("VS").Bold().FontSize(14).FontColor(Colors.White);
                    logoRow.RelativeItem().PaddingLeft(8).Column(c =>
                    {
                        c.Item().Text("VARGSHALA").FontSize(16).Black().FontColor(PrimaryTeal).LetterSpacing(0.05f);
                        c.Item().Text("Cloud Coaching & Institute Management SaaS").FontSize(8).FontColor(Slate500);
                    });
                });
            });

            // Right: Tax Invoice / Official Receipt Tag
            row.RelativeItem().AlignRight().Column(innerCol =>
            {
                innerCol.Item().Text("OFFICIAL TAX INVOICE").FontSize(12).Black().FontColor(PrimaryTeal);
                innerCol.Item().Text("SUBSCRIPTION PAYMENT RECEIPT").FontSize(8.5f).Bold().FontColor(AccentTeal);
                innerCol.Item().PaddingTop(3).Container()
                    .Background(PaidBadgeBg)
                    .Border(1).BorderColor(PaidBadgeBorder)
                    .CornerRadius(4)
                    .PaddingHorizontal(8).PaddingVertical(2)
                    .Text("STATUS: PAID").FontSize(8).Black().FontColor(PaidBadgeText);
            });
        });
    }

    private void ComposeMetadataBar(ColumnDescriptor col, SubscriptionPaymentReceiptDto receipt)
    {
        col.Item().Row(row =>
        {
            row.RelativeItem().Column(c =>
            {
                c.Item().Text("RECEIPT / INVOICE #").FontSize(7.5f).Bold().FontColor(Slate400);
                c.Item().Text(receipt.ReceiptNumber).FontSize(10).Black().FontColor(PrimaryTeal);
            });

            row.RelativeItem().Column(c =>
            {
                c.Item().Text("PAYMENT DATE").FontSize(7.5f).Bold().FontColor(Slate400);
                c.Item().Text(receipt.PaymentDate.ToString("dd MMMM yyyy, hh:mm tt")).FontSize(9).FontColor(Slate800);
            });

            row.RelativeItem().Column(c =>
            {
                c.Item().Text("PAYMENT METHOD").FontSize(7.5f).Bold().FontColor(Slate400);
                c.Item().Text(receipt.PaymentMethod).FontSize(9).Bold().FontColor(Slate800);
            });

            row.RelativeItem().Column(c =>
            {
                c.Item().Text("GATEWAY REFERENCE").FontSize(7.5f).Bold().FontColor(Slate400);
                c.Item().Text(!string.IsNullOrWhiteSpace(receipt.TransactionReference) ? receipt.TransactionReference : "N/A")
                    .FontSize(8.5f).FontFamily(Fonts.Courier).FontColor(Slate700);
            });
        });
    }

    private void ComposeSubscriberCard(ColumnDescriptor col, SubscriptionPaymentReceiptDto receipt)
    {
        col.Item().Background(Slate50)
            .Border(1).BorderColor(Slate200)
            .CornerRadius(6)
            .Padding(10)
            .Row(row =>
            {
                row.RelativeItem().Column(c =>
                {
                    c.Item().Text("BILL TO (SUBSCRIBING INSTITUTION):").FontSize(8).Bold().FontColor(PrimaryTeal);
                    c.Item().PaddingTop(2).Text(receipt.OrganizationName).FontSize(11).Bold().FontColor(Slate900);
                    if (!string.IsNullOrWhiteSpace(receipt.OrganizationEmail))
                    {
                        c.Item().Text($"Email: {receipt.OrganizationEmail}").FontSize(8.5f).FontColor(Slate600);
                    }
                    if (!string.IsNullOrWhiteSpace(receipt.OrganizationPhone))
                    {
                        c.Item().Text($"Phone: {receipt.OrganizationPhone}").FontSize(8.5f).FontColor(Slate600);
                    }
                    if (!string.IsNullOrWhiteSpace(receipt.OrganizationAddress))
                    {
                        c.Item().Text($"Address: {receipt.OrganizationAddress}").FontSize(8.5f).FontColor(Slate600);
                    }
                });

                row.RelativeItem().Column(c =>
                {
                    c.Item().Text("SUBSCRIPTION PERIOD & QUOTA:").FontSize(8).Bold().FontColor(PrimaryTeal);
                    c.Item().PaddingTop(2).Text($"Plan Tier: {receipt.PlanName} ({receipt.BillingCycle})").FontSize(9.5f).Bold().FontColor(Slate800);
                    c.Item().Text($"Student Capacity: {receipt.StudentQuota}").FontSize(8.5f).FontColor(Slate600);
                    c.Item().Text($"Faculty & Staff Quota: {receipt.TeacherQuota}").FontSize(8.5f).FontColor(Slate600);
                    c.Item().Text($"Campus Limit: {receipt.BranchQuota}").FontSize(8.5f).FontColor(Slate600);
                    if (receipt.SubscriptionStartDate.HasValue && receipt.SubscriptionEndDate.HasValue)
                    {
                        c.Item().Text($"Validity: {receipt.SubscriptionStartDate.Value:dd MMM yyyy} to {receipt.SubscriptionEndDate.Value:dd MMM yyyy}").FontSize(8.5f).Bold().FontColor(AccentTeal);
                    }
                });
            });
    }

    private void ComposeParticularsTable(ColumnDescriptor col, SubscriptionPaymentReceiptDto receipt)
    {
        col.Item().Table(table =>
        {
            table.ColumnsDefinition(cols =>
            {
                cols.ConstantColumn(30);
                cols.RelativeColumn(5);
                cols.RelativeColumn(2);
                cols.RelativeColumn(2);
            });

            // Table Header
            table.Header(header =>
            {
                header.Cell().Background(TableHeaderBg).Padding(6).Text("#").FontSize(8).Bold().FontColor(PrimaryTeal);
                header.Cell().Background(TableHeaderBg).Padding(6).Text("Service / Plan Particulars").FontSize(8).Bold().FontColor(PrimaryTeal);
                header.Cell().Background(TableHeaderBg).Padding(6).Text("Billing Cycle").FontSize(8).Bold().FontColor(PrimaryTeal);
                header.Cell().Background(TableHeaderBg).Padding(6).AlignRight().Text("Amount (INR)").FontSize(8).Bold().FontColor(PrimaryTeal);
            });

            // Row 1: Plan Base Price
            table.Cell().BorderBottom(1).BorderColor(Slate100).Padding(6).Text("1").FontSize(8.5f);
            table.Cell().BorderBottom(1).BorderColor(Slate100).Padding(6).Column(c =>
            {
                c.Item().Text($"Vargshala SaaS Platform - {receipt.PlanName} Plan").FontSize(9).Bold().FontColor(Slate900);
                c.Item().Text($"Includes up to {receipt.StudentQuota}, {receipt.TeacherQuota}, {receipt.BranchQuota}, automated fee collections, attendance & messaging.").FontSize(7.5f).FontColor(Slate500);
            });
            table.Cell().BorderBottom(1).BorderColor(Slate100).Padding(6).Text(receipt.BillingCycle).FontSize(8.5f);
            table.Cell().BorderBottom(1).BorderColor(Slate100).Padding(6).AlignRight().Text($"₹{receipt.OriginalAmount:N2}").FontSize(8.5f);

            // Row 2: Discount if applicable
            if (receipt.DiscountAmount > 0)
            {
                table.Cell().BorderBottom(1).BorderColor(Slate100).Padding(6).Text("2").FontSize(8.5f);
                table.Cell().BorderBottom(1).BorderColor(Slate100).Padding(6).Text($"Promotional Coupon Discount {(!string.IsNullOrWhiteSpace(receipt.CouponCode) ? $"({receipt.CouponCode})" : "")}").FontSize(8.5f).FontColor(Color.FromHex("#15803D"));
                table.Cell().BorderBottom(1).BorderColor(Slate100).Padding(6).Text("Applied").FontSize(8.5f).FontColor(Color.FromHex("#15803D"));
                table.Cell().BorderBottom(1).BorderColor(Slate100).Padding(6).AlignRight().Text($"- ₹{receipt.DiscountAmount:N2}").FontSize(8.5f).Bold().FontColor(Color.FromHex("#15803D"));
            }

            // Total Row
            table.Cell().ColumnSpan(3).Background(TableTotalBg).Padding(6).AlignRight().Text("NET TOTAL PAID:").FontSize(9.5f).Black().FontColor(PrimaryTeal);
            table.Cell().Background(TableTotalBg).Padding(6).AlignRight().Text($"₹{receipt.Amount:N2}").FontSize(11).Black().FontColor(PrimaryTeal);
        });
    }

    private void ComposeAmountInWords(ColumnDescriptor col, SubscriptionPaymentReceiptDto receipt)
    {
        var amountWords = ConvertNumberToWords((long)Math.Round(receipt.Amount));

        col.Item().PaddingTop(6).Background(Slate50)
            .Border(1).BorderColor(Slate200)
            .CornerRadius(4)
            .Padding(6)
            .Row(row =>
            {
                row.AutoItem().Text("Amount in words: ").FontSize(8).Bold().FontColor(PrimaryTeal);
                row.RelativeItem().Text($"Rupees {amountWords} Only").FontSize(8).Bold().Italic().FontColor(Slate800);
            });
    }

    private void ComposeFooterAndTerms(ColumnDescriptor col)
    {
        col.Item().PaddingTop(12).Row(row =>
        {
            // Left: Terms
            row.RelativeItem().Column(c =>
            {
                c.Item().Text("TERMS & CONDITIONS:").FontSize(7.5f).Bold().FontColor(Slate600);
                c.Item().Text("1. This is an electronically generated receipt for software subscription services.").FontSize(7).FontColor(Slate400);
                c.Item().Text("2. Subscriptions are billed in advance according to the selected billing cycle.").FontSize(7).FontColor(Slate400);
                c.Item().Text("3. For tax invoice questions or billing assistance, contact billing@vargshala.com.").FontSize(7).FontColor(Slate400);
            });

            // Right: Verification Seal
            row.RelativeItem().AlignRight().Column(c =>
            {
                c.Item().Text("DIGITALLY SIGNED & VERIFIED").FontSize(7.5f).Bold().FontColor(AccentTeal);
                c.Item().Text("Authorized by Vargshala Automated Billing System").FontSize(7).FontColor(Slate400);
                c.Item().Text("No physical signature required.").FontSize(6.5f).Italic().FontColor(Slate400);
            });
        });
    }

    private string ConvertNumberToWords(long number)
    {
        if (number == 0) return "Zero";
        if (number < 0) return "Minus " + ConvertNumberToWords(Math.Abs(number));

        string words = "";

        if ((number / 10000000) > 0)
        {
            words += ConvertNumberToWords(number / 10000000) + " Crore ";
            number %= 10000000;
        }

        if ((number / 100000) > 0)
        {
            words += ConvertNumberToWords(number / 100000) + " Lakh ";
            number %= 100000;
        }

        if ((number / 1000) > 0)
        {
            words += ConvertNumberToWords(number / 1000) + " Thousand ";
            number %= 1000;
        }

        if ((number / 100) > 0)
        {
            words += ConvertNumberToWords(number / 100) + " Hundred ";
            number %= 100;
        }

        if (number > 0)
        {
            var unitsMap = new[] { "Zero", "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine", "Ten", "Eleven", "Twelve", "Thirteen", "Fourteen", "Fifteen", "Sixteen", "Seventeen", "Eighteen", "Nineteen" };
            var tensMap = new[] { "Zero", "Ten", "Twenty", "Thirty", "Forty", "Fifty", "Sixty", "Seventy", "Eighty", "Ninety" };

            if (number < 20)
                words += unitsMap[number];
            else
            {
                words += tensMap[number / 10];
                if ((number % 10) > 0)
                    words += " " + unitsMap[number % 10];
            }
        }

        return words.Trim();
    }
}
