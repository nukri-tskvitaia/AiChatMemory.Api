using System.Collections.Concurrent;
using AiChatMemory.Api.Models;

namespace AiChatMemory.Api.Storage;

public sealed class ConversationStore
{
    private sealed class Conversation
    {
        public readonly List<ChatMessage> Messages = [];
        public readonly object Lock = new();
    }

    private readonly ConcurrentDictionary<string, Conversation> _conversations = new();

    public IReadOnlyList<ChatMessage> GetMessages(string conversationId, int maxMessages)
    {
        conversationId = conversationId.Trim();

        if (maxMessages <= 0)
            return [];

        if (!_conversations.TryGetValue(conversationId, out var conversation))
            return [];

        lock (conversation.Lock)
        {
            var messages = conversation.Messages;
            var skip = Math.Max(0, messages.Count - maxMessages);

            return messages.GetRange(skip, messages.Count - skip);
        }
    }

    public void AddMessage(string conversationId, string role, string content)
    {
        var conversation = _conversations.GetOrAdd(conversationId, _ => new Conversation());

        lock (conversation.Lock)
        {
            conversation.Messages.Add(new ChatMessage
            {
                Role = role,
                Content = content
            });
        }
    }

    public void Clear(string conversationId)
    {
        _conversations.TryRemove(conversationId, out _);
    }
}