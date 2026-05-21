namespace AiChatMemory.Api.Exceptions;

public sealed class AnthropicException(int statusCode, string responseText)
    : Exception($"Anthropic API request failed with status code: {statusCode}. Response: {responseText}")
{

}