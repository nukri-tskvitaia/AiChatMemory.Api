using AiChatMemory.Api.Models;
using AiChatMemory.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace AiChatMemory.Api.Controllers;

[ApiController]
[Route("api/chat")]
public sealed class ChatController(ClaudeChatService chatService) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(ChatResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ChatResponse>> Chat(
        [FromBody] ChatRequest request,
        CancellationToken cancellationToken)
    {
        var response = await chatService.ChatAsync(request, cancellationToken);

        return Ok(response);
    }

    [HttpDelete("{conversationId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public IActionResult ClearConversation(string conversationId) // in case later we use consistent memory
    {
        chatService.ClearConversation(conversationId);

        return NoContent();
    }
}