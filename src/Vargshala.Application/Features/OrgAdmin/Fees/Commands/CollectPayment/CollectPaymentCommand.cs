using MediatR;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.Fees;

namespace Vargshala.Application.Features.OrgAdmin.Fees.Commands.CollectPayment;

public record CollectPaymentCommand(CollectPaymentRequest Request) : IRequest<ApiResponse<PaymentDto>>;
