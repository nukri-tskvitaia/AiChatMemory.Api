using AiChatMemory.Api.Options;
using AiChatMemory.Api.Services;
using AiChatMemory.Api.Storage;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services
    .AddOptions<ClaudeOptions>()
    .Bind(builder.Configuration.GetSection(ClaudeOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddSingleton<ConversationStore>();

builder.Services.AddHttpClient<ClaudeChatService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

await app.RunAsync();
