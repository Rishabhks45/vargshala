using Vargshala.Contracts.Fees;

namespace Vargshala.Application.Abstractions.Pdf;

public interface IFeeReceiptPdfGenerator
{
    byte[] GenerateReceiptPdf(FeeReceiptPdfModel model);
}
