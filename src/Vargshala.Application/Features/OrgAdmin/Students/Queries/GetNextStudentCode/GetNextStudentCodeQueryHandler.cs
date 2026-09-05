using MediatR;
using Vargshala.Application.Features.OrgAdmin.Students.Helpers;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.Students;

namespace Vargshala.Application.Features.OrgAdmin.Students.Queries.GetNextStudentCode;

public class GetNextStudentCodeQueryHandler : IRequestHandler<GetNextStudentCodeQuery, ApiResponse<GeneratedStudentCodeDto>>
{
    private readonly IStudentCodeGenerator _studentCodeGenerator;

    public GetNextStudentCodeQueryHandler(IStudentCodeGenerator studentCodeGenerator)
    {
        _studentCodeGenerator = studentCodeGenerator;
    }

    public async Task<ApiResponse<GeneratedStudentCodeDto>> Handle(GetNextStudentCodeQuery request, CancellationToken cancellationToken)
    {
        var result = await _studentCodeGenerator.GenerateNextCodeAndRollAsync(cancellationToken);
        var dto = new GeneratedStudentCodeDto
        {
            StudentCode = result.StudentCode,
            RollNumber = result.RollNumber
        };
        return ApiResponse<GeneratedStudentCodeDto>.SuccessResponse(dto, "Next student code and roll number generated successfully.");
    }
}
