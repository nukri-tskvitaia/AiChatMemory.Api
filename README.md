# AI Chat Memory API

A lightweight ASP.NET Core 8 Web API demonstrating conversation memory management for LLM applications using Anthropic Claude.

The project shows how AI applications preserve conversation context between API calls by storing and resending chat history to the model.

---

# Features

- ASP.NET Core 8 Web API
- Anthropic Claude API integration
- Conversation memory management
- Follow-up question understanding
- In-memory conversation storage
- Conversation ID support
- Context window management
- Multi-turn AI conversations
- Strongly typed configuration using `IOptions`
- Request validation
- Swagger/OpenAPI support
- CancellationToken support
- Safe JSON parsing

---

# Architecture

```text
Client / Swagger
→ ChatController
→ ClaudeChatService
→ ConversationStore
   → Stores user + assistant messages
→ Claude API
   → Receives previous conversation history
→ Claude response
→ Store assistant response
→ API response
```

---

# Full Flow Explanation

## ① User sends first message

Example:

```json
{
  "conversationId": "demo-1",
  "message": "Explain Docker in simple words."
}
```

---

## ② API stores the user message

The backend stores:

```text
Role: user
Content: Explain Docker in simple words.
```

inside the conversation history.

---

## ③ API loads previous conversation history

The API retrieves conversation history using:

```text
conversationId
```

Example history:

```text
User: Explain Docker in simple words.
Assistant: Docker is a container system...
```

---

## ④ API sends history to Claude

Claude receives:

```json
[
  {
    "role": "user",
    "content": "Explain Docker in simple words."
  },
  {
    "role": "assistant",
    "content": "Docker is..."
  }
]
```

plus the newest user message.

This allows Claude to understand follow-up questions.

---

## ⑤ Claude generates response using memory

Example follow-up:

```json
{
  "conversationId": "demo-1",
  "message": "Make that even shorter."
}
```

Claude understands:

```text
"that" = previous Docker explanation
```

because the API resent the previous conversation history.

---

## ⑥ API stores Claude response

The assistant response is stored in memory:

```text
Role: assistant
Content: Docker is a container system...
```

---

## ⑦ Final response returned

Example:

```json
{
  "conversationId": "demo-1",
  "answer": "Docker is a container for your app..."
}
```

---

# AI Concepts Demonstrated

| Concept | Used |
|---|---|
| Conversation memory | Yes |
| Context window management | Yes |
| Multi-turn chat | Yes |
| Application-managed memory | Yes |
| Follow-up understanding | Yes |
| Conversation IDs | Yes |
| AI orchestration | Yes |

---

# Important Concept

LLM APIs typically do NOT automatically remember previous API calls.

Memory is usually managed by the application itself.

This project demonstrates the standard production pattern:

```text
Store conversation history
↓
Load previous messages
↓
Send history back to the model
↓
Generate contextual response
```

---

# Technologies Used

- ASP.NET Core 8 Web API
- C#
- Anthropic Claude API
- Swagger / OpenAPI
- System.Text.Json
- ConcurrentDictionary
- Options Pattern (`IOptions<T>`)

---

# Project Structure

```text
Controllers/
Models/
Options/
Services/
Storage/
Exceptions/
```

---

# Example Request

```json
{
  "conversationId": "demo-1",
  "message": "Explain Docker in simple words."
}
```

---

# Example Response

```json
{
  "conversationId": "demo-1",
  "answer": "Docker is a container for your app..."
}
```

---

# Example Follow-Up Request

```json
{
  "conversationId": "demo-1",
  "message": "Make that even shorter."
}
```

---

# Example Follow-Up Response

```json
{
  "conversationId": "demo-1",
  "answer": "Docker packages your app with everything it needs so it runs the same everywhere."
}
```

---

# Configuration

Store configuration securely using User Secrets or environment variables.

Example configuration:

```json
{
  "Claude": {
    "ApiKey": "your_claude_api_key",
    "Version": "2023-06-01",
    "Model": "claude-haiku-4-5-20251001",
    "MaxTokens": 1000,
    "Temperature": 0.3,
    "MessagesEndpointUrl": "https://api.anthropic.com/v1/messages",
    "MaxHistoryMessages": 10
  }
}
```

---

# Running the Project

## Clone repository

```bash
git clone <your_repo_url>
```

---

## Navigate to project

```bash
cd AiChatMemory.Api
```

---

## Restore packages

```bash
dotnet restore
```

---

## Run the API

```bash
dotnet run
```

Swagger UI will open automatically.

---

# API Endpoints

## Chat Endpoint

```http
POST /api/chat
```

---

## Clear Conversation

```http
DELETE /api/chat/{conversationId}
```

This removes stored conversation history.

---

# Conversation Storage
The project uses a lightweight in-memory store backed by a `ConcurrentDictionary<string, Conversation>`,
where each `Conversation` wraps a `List<ChatMessage>` with a dedicated lock object for thread safety.

This keeps the project intentionally simple and focused on AI memory concepts.

**Characteristics:**
- Thread-safe reads and writes per conversation
- Messages are stored in insertion order — no sorting on read
- All data is lost on restart (by design — no persistence layer)

---

# Context Window Management

The project includes simple context window limiting:

```json
"MaxHistoryMessages": 10
```

Only the most recent messages are resent to Claude.

This demonstrates an important production concept:

```text
Unlimited chat history is expensive and inefficient.
Applications usually limit context size.
```

---

# Notes

This project demonstrates how production AI chat systems typically manage conversational memory.

The architecture intentionally keeps memory management lightweight while showcasing:

- conversation persistence
- context reconstruction
- follow-up understanding
- multi-turn AI chat
- application-managed LLM memory
- context window limiting
- AI backend orchestration principles