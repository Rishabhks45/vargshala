using Vargshala.Contracts.Attendances;
using Vargshala.Contracts.Common;

namespace Vargshala.Web.Services;

public interface IAttendanceService
{
    Task<ApiResponse<SessionAttendanceSheetDto>> GetSessionAttendanceSheetAsync(Guid sessionId);
    Task<ApiResponse<SessionAttendanceSheetDto>> SaveSessionAttendanceAsync(Guid sessionId, MarkSessionAttendanceRequest request);
    Task<ApiResponse<BatchAttendanceOverviewDto>> GetBatchOverviewAsync(Guid batchId, DateOnly? date = null);
    Task<ApiResponse<List<DateAttendanceReportDto>>> GetDateWiseReportAsync(Guid batchId, DateOnly? fromDate = null, DateOnly? toDate = null);
    Task<ApiResponse<List<StudentAttendanceReportDto>>> GetStudentWiseReportAsync(Guid batchId);
}
