using Vargshala.Contracts.Common;
using Vargshala.Contracts.Messages;

namespace Vargshala.Application.Abstractions.Storage;

public interface IStorageService
{
    Task<ApiResponse<MessageAttachmentUploadResponse>> UploadFileAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        string folderPath,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<bool>> DeleteFileAsync(
        string storageKey,
        CancellationToken cancellationToken = default);
}
