using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vargshala.Application.Abstractions.CurrentUser;
using Vargshala.Application.Abstractions.Storage;
using Vargshala.Application.Features.Messages.Commands.CreateConversation;
using Vargshala.Application.Features.Messages.Commands.MarkConversationAsRead;
using Vargshala.Application.Features.Messages.Commands.SendMessage;
using Vargshala.Application.Features.Messages.Commands.ToggleReaction;
using Vargshala.Application.Features.Messages.Queries.GetConversations;
using Vargshala.Application.Features.Messages.Queries.GetMessages;
using Vargshala.Contracts.Common;
using Vargshala.Contracts.Messages;

namespace Vargshala.API.Controllers;

[ApiController]
[Route("api/v1/messages")]
[Authorize]
public class MessagesController : ControllerBase
{
    private readonly IMediator _mediator;

    public MessagesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("conversations")]
    public async Task<IActionResult> GetConversations([FromQuery] GetConversationsPagedRequest request)
    {
        var result = await _mediator.Send(new GetConversationsQuery(request));
        if (!result.Success)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    [HttpPost("conversations")]
    public async Task<IActionResult> CreateConversation([FromBody] CreateConversationRequest request)
    {
        var result = await _mediator.Send(new CreateConversationCommand(request));
        if (!result.Success)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    [HttpGet("conversations/{conversationId:guid}/messages")]
    public async Task<IActionResult> GetMessages([FromRoute] Guid conversationId, [FromQuery] PagedRequest request)
    {
        var result = await _mediator.Send(new GetMessagesQuery(conversationId, request));
        if (!result.Success)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    [HttpPost("send")]
    public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest request)
    {
        var result = await _mediator.Send(new SendMessageCommand(request));
        if (!result.Success)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    [HttpPost("conversations/{conversationId:guid}/read/{latestMessageId:guid}")]
    public async Task<IActionResult> MarkAsRead([FromRoute] Guid conversationId, [FromRoute] Guid latestMessageId)
    {
        var result = await _mediator.Send(new MarkConversationAsReadCommand(conversationId, latestMessageId));
        if (!result.Success)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    [HttpPost("conversations/{conversationId:guid}/admins/{userId:guid}")]
    public async Task<IActionResult> PromoteAdmin([FromRoute] Guid conversationId, [FromRoute] Guid userId)
    {
        var result = await _mediator.Send(new Vargshala.Application.Features.Messages.Commands.PromoteAdmin.PromoteAdminCommand(conversationId, userId));
        if (!result.Success)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    [HttpDelete("conversations/{conversationId:guid}/participants/{userId:guid}")]
    public async Task<IActionResult> RemoveParticipant([FromRoute] Guid conversationId, [FromRoute] Guid userId)
    {
        var result = await _mediator.Send(new Vargshala.Application.Features.Messages.Commands.RemoveParticipant.RemoveParticipantCommand(conversationId, userId));
        if (!result.Success)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    [HttpPost("conversations/{conversationId:guid}/participants/{userId:guid}")]
    public async Task<IActionResult> AddParticipant([FromRoute] Guid conversationId, [FromRoute] Guid userId)
    {
        var result = await _mediator.Send(new Vargshala.Application.Features.Messages.Commands.AddParticipant.AddParticipantCommand(conversationId, userId));
        if (!result.Success)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    [HttpPut("conversations/{conversationId:guid}/photo")]
    public async Task<IActionResult> ChangeGroupPhoto([FromRoute] Guid conversationId, [FromBody] ChangeGroupPhotoRequest request)
    {
        var result = await _mediator.Send(new Vargshala.Application.Features.Messages.Commands.ChangeGroupPhoto.ChangeGroupPhotoCommand(conversationId, request.GroupPhotoUrl));
        if (!result.Success)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    [HttpGet("recipients/eligible")]
    public async Task<IActionResult> GetEligibleRecipients([FromQuery] GetEligibleRecipientsRequest request)
    {
        var result = await _mediator.Send(new Vargshala.Application.Features.Messages.Queries.GetEligibleRecipients.GetEligibleRecipientsQuery(request));
        if (!result.Success)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    [HttpPost("{messageId:guid}/reactions")]
    public async Task<IActionResult> ToggleReaction([FromRoute] Guid messageId, [FromBody] ToggleMessageReactionRequest request)
    {
        var result = await _mediator.Send(new ToggleMessageReactionCommand(messageId, request.Emoji));
        if (!result.Success)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    [HttpPost("conversations/{conversationId:guid}/attachments")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(15 * 1024 * 1024)]
    [RequestFormLimits(MultipartBodyLengthLimit = 15 * 1024 * 1024)]
    public async Task<IActionResult> UploadAttachment(
        [FromRoute] Guid conversationId,
        IFormFile? file,
        [FromServices] IStorageService storageService,
        [FromServices] ICurrentUser currentUser)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(ApiResponse<MessageAttachmentUploadResponse>.FailureResponse("No file was uploaded."));
        }

        if (currentUser.OrganizationId is null || currentUser.UserId == Guid.Empty)
        {
            return Unauthorized(ApiResponse<MessageAttachmentUploadResponse>.FailureResponse("User is not authenticated."));
        }

        var ext = Path.GetExtension(file.FileName)?.ToLowerInvariant();
        var allowedExts = new[] { ".pdf", ".png", ".jpg", ".jpeg", ".webp", ".docx", ".doc" };
        if (string.IsNullOrEmpty(ext) || !allowedExts.Contains(ext))
        {
            return BadRequest(ApiResponse<MessageAttachmentUploadResponse>.FailureResponse(
                $"File type '{ext}' is not supported. Allowed: PDF, PNG, JPG, JPEG, WEBP, DOCX, DOC"));
        }

        if (file.Length > 15 * 1024 * 1024)
        {
            return BadRequest(ApiResponse<MessageAttachmentUploadResponse>.FailureResponse("File size exceeds 15 MB limit."));
        }

        // Subfolder routing matching Supabase bucket structure: DOCX, Image, PDF
        var subFolder = ext switch
        {
            ".pdf" => "PDF",
            ".png" or ".jpg" or ".jpeg" or ".webp" => "Image",
            ".docx" or ".doc" => "DOCX",
            _ => "DOCX"
        };

        var orgId = currentUser.OrganizationId.Value;
        var folderPath = subFolder;

        using var stream = file.OpenReadStream();
        var result = await storageService.UploadFileAsync(
            stream, 
            file.FileName, 
            file.ContentType, 
            folderPath, 
            HttpContext.RequestAborted);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpDelete("{messageId:guid}")]
    public async Task<IActionResult> DeleteMessage([FromRoute] Guid messageId)
    {
        var result = await _mediator.Send(new Vargshala.Application.Features.Messages.Commands.DeleteMessage.DeleteMessageCommand(messageId));
        if (!result.Success)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    [HttpPost("{messageId:guid}/restore")]
    public async Task<IActionResult> RestoreMessage([FromRoute] Guid messageId)
    {
        var result = await _mediator.Send(new Vargshala.Application.Features.Messages.Commands.RestoreMessage.RestoreMessageCommand(messageId));
        if (!result.Success)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }
}


