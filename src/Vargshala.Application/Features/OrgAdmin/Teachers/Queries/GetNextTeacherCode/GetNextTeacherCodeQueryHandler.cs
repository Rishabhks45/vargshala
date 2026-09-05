using MediatR;
using Vargshala.Application.Features.OrgAdmin.Teachers.Helpers;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.Teachers;

namespace Vargshala.Application.Features.OrgAdmin.Teachers.Queries.GetNextTeacherCode;

public class GetNextTeacherCodeQueryHandler : IRequestHandler<GetNextTeacherCodeQuery, ApiResponse<GeneratedTeacherCodeDto>>
{
    private readonly IEmployeeCodeGenerator _employeeCodeGenerator;

    public GetNextTeacherCodeQueryHandler(IEmployeeCodeGenerator employeeCodeGenerator)
    {
        _employeeCodeGenerator = employeeCodeGenerator;
    }

    public async Task<ApiResponse<GeneratedTeacherCodeDto>> Handle(GetNextTeacherCodeQuery request, CancellationToken cancellationToken)
    {
        var code = await _employeeCodeGenerator.GenerateNextCodeAsync(cancellationToken);
        var dto = new GeneratedTeacherCodeDto
        {
            EmployeeCode = code
        };
        return ApiResponse<GeneratedTeacherCodeDto>.SuccessResponse(dto, "Next employee code generated successfully.");
    }
}
