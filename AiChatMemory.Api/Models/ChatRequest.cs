using System.ComponentModel.DataAnnotations;

namespace AiChatMemory.Api.Models;

public sealed class ChatRequest
{
    [Required]
    [MinLength(1)]
    public string ConversationId { get; set; } = string.Empty;

    [Required]
    [MinLength(2)]
    public string Message { get; set; } = string.Empty;
}