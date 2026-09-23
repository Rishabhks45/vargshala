using Vargshala.Contracts.Subscriptions;

namespace Vargshala.Application.Abstractions.Pdf;

public interface ISubscriptionReceiptPdfGenerator
{
    byte[] GenerateSubscriptionReceiptPdf(SubscriptionPaymentReceiptDto receipt);
}
