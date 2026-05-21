using System.Text;
using System.Text.Json;
using AiChatMemory.Api.Exceptions;
using AiChatMemory.Api.Models;
using AiChatMemory.Api.Options;
using AiChatMemory.Api.Storage;
using Microsoft.Extensions.Options;

namespace AiChatMemory.Api.Services;

public sealed class ClaudeChatService(
    HttpClient httpClient,
    IOptions<ClaudeOptions> options,
    ConversationStore conversationStore)
{
    private readonly ClaudeOptions _options = options.Value;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public async Task<ChatResponse> ChatAsync(
        ChatRequest request,
        CancellationToken cancellationToken = default)
    {
        conversationStore.AddMessage(
            request.ConversationId,
            "user",
            request.Message);

        var history = conversationStore.GetMessages(
            request.ConversationId,
            _options.MaxHistoryMessages);

        var answer = await SendToClaudeAsync(history, cancellationToken);

        conversationStore.AddMessage(
            request.ConversationId,
            "assistant",
            answer);

        return new ChatResponse
        {
            ConversationId = request.ConversationId,
            Answer = answer
        };
    }

    public void ClearConversation(string conversationId)
    {
        conversationStore.Clear(conversationId);
    }

    private async Task<string> SendToClaudeAsync(
        IReadOnlyList<ChatMessage> history,
        CancellationToken cancellationToken)
    {
        var requestBody = new
        {
            model = _options.Model,
            max_tokens = _options.MaxTokens,
            temperature = _options.Temperature,
            system = """
            You are a helpful assistant.

            Use the conversation history to understand follow-up questions.
            If the user says "that", "it", "this", or asks a follow-up, infer the reference from the previous messages.
            If the reference is unclear, ask a short clarifying question.
            """,
            messages = history.Select(message => new
            {
                role = message.Role,
                content = message.Content
            }).ToArray()
        };

        using var httpRequest = new HttpRequestMessage(
            HttpMethod.Post,
            _options.MessagesEndpointUrl);

        httpRequest.Headers.Add("x-api-key", _options.ApiKey);
        httpRequest.Headers.Add("anthropic-version", _options.Version);

        httpRequest.Content = new StringContent(
            JsonSerializer.Serialize(requestBody, JsonOptions),
            Encoding.UTF8,
            "application/json");

        using var response = await httpClient.SendAsync(httpRequest, cancellationToken);

        var responseText = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw new AnthropicException((int)response.StatusCode, responseText);

        return ExtractTextAnswer(responseText);
    }

    private static string ExtractTextAnswer(string responseText)
    {
        using var document = JsonDocument.Parse(responseText);

        var content = document.RootElement.GetProperty("content");

        foreach (var block in content.EnumerateArray())
        {
            if (!block.TryGetProperty("type", out var typeProperty))
                continue;

            if (!string.Equals(typeProperty.GetString(), "text", StringComparison.OrdinalIgnoreCase))
                continue;

            return block.GetProperty("text").GetString() ?? string.Empty;
        }

        return string.Empty;
    }
}