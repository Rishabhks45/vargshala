using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vargshala.Application.Features.Messages.Commands.CreateConversation;
using Vargshala.Application.Features.Messages.Commands.MarkConversationAsRead;
using Vargshala.Application.Features.Messages.Commands.SendMessage;
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
}

