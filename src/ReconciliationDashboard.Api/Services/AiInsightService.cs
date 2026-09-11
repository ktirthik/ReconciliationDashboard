using System.Text.Json;
using Azure.AI.OpenAI;
using Microsoft.EntityFrameworkCore;
using OpenAI.Chat;
using OpenAI.Embeddings;
using ReconciliationDashboard.Api.Data;
using ReconciliationDashboard.Api.Models;
using ReconciliationDashboard.Api.Models.Dtos;

namespace ReconciliationDashboard.Api.Services;

public class AiInsightService(AppDbContext db, IConfiguration config, ILogger<AiInsightService> logger)
    : IAiInsightService
{
    public async Task<AiInsightDto> GetInsightAsync(Guid accountId, CancellationToken ct)
    {
        var account = await db.Accounts.FindAsync([accountId], ct)
            ?? throw new KeyNotFoundException($"Account {accountId} not found.");

        var summary = await GenerateSummaryAsync(account, ct);
        var similarCases = await FindSimilarCasesAsync(account, ct);

        return new AiInsightDto(summary, similarCases);
    }

    private async Task<string> GenerateSummaryAsync(Account account, CancellationToken ct)
    {
        var endpoint = config["AzureOpenAI:Endpoint"];
        var apiKey = config["AzureOpenAI:ApiKey"];
        var deployment = config["AzureOpenAI:ChatDeployment"] ?? "gpt-4o";

        if (string.IsNullOrWhiteSpace(endpoint) || string.IsNullOrWhiteSpace(apiKey))
        {
            logger.LogWarning("Azure OpenAI is not configured — returning placeholder summary.");
            return BuildPlaceholderSummary(account);
        }

        var client = new AzureOpenAIClient(new Uri(endpoint), new Azure.AzureKeyCredential(apiKey));
        var chatClient = client.GetChatClient(deployment);

        var prompt = BuildSummaryPrompt(account);
        var response = await chatClient.CompleteChatAsync(
            [new UserChatMessage(prompt)],
            cancellationToken: ct);

        return response.Value.Content[0].Text;
    }

    private async Task<IReadOnlyList<SimilarCaseDto>> FindSimilarCasesAsync(Account account, CancellationToken ct)
    {
        var caseNotes = await db.CaseNotes
            .Where(c => c.EmbeddingJson != null)
            .ToListAsync(ct);

        if (caseNotes.Count == 0) return [];

        var endpoint = config["AzureOpenAI:Endpoint"];
        var apiKey = config["AzureOpenAI:ApiKey"];
        var embeddingDeployment = config["AzureOpenAI:EmbeddingDeployment"] ?? "text-embedding-3-small";

        // If Azure OpenAI isn't configured, fall back to keyword matching
        if (string.IsNullOrWhiteSpace(endpoint) || string.IsNullOrWhiteSpace(apiKey))
            return KeywordFallback(account, caseNotes);

        try
        {
            var client = new AzureOpenAIClient(new Uri(endpoint), new Azure.AzureKeyCredential(apiKey));
            var embeddingClient = client.GetEmbeddingClient(embeddingDeployment);

            var query = $"{account.CustomerName} {account.Status} {account.FlagReason}";
            var embeddingResult = await embeddingClient.GenerateEmbeddingAsync(query, cancellationToken: ct);
            var queryVec = embeddingResult.Value.ToFloats().ToArray();

            return caseNotes
                .Select(c =>
                {
                    var stored = JsonSerializer.Deserialize<float[]>(c.EmbeddingJson!)!;
                    return new { Note = c, Score = CosineSimilarity(queryVec, stored) };
                })
                .OrderByDescending(x => x.Score)
                .Take(3)
                .Where(x => x.Score > 0.7)
                .Select(x => new SimilarCaseDto(x.Note.Id, x.Note.Title, x.Note.Content, x.Note.Tags, Math.Round(x.Score, 3)))
                .ToList();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Embedding call failed — falling back to keyword search.");
            return KeywordFallback(account, caseNotes);
        }
    }

    private static IReadOnlyList<SimilarCaseDto> KeywordFallback(Account account, List<CaseNote> notes)
    {
        var keywords = $"{account.Status} {account.FlagReason}"
            .ToLowerInvariant().Split(' ', StringSplitOptions.RemoveEmptyEntries);

        return notes
            .Select(n =>
            {
                var text = $"{n.Title} {n.Content} {n.Tags}".ToLowerInvariant();
                var score = keywords.Count(k => text.Contains(k)) / (double)keywords.Length;
                return new { Note = n, Score = score };
            })
            .Where(x => x.Score > 0)
            .OrderByDescending(x => x.Score)
            .Take(3)
            .Select(x => new SimilarCaseDto(x.Note.Id, x.Note.Title, x.Note.Content, x.Note.Tags, Math.Round(x.Score, 3)))
            .ToList();
    }

    private static string BuildSummaryPrompt(Account account) =>
        $"""
        You are a customer account analyst. Summarise the following flagged account in 2-3 plain-language sentences
        that a non-technical stakeholder can understand. Focus on what the underlying issue is and what
        action is likely needed.

        Account number: {account.AccountNumber}
        Customer: {account.CustomerName}
        Status: {account.Status}
        Flag reason: {account.FlagReason ?? "Not specified"}
        """;

    private static string BuildPlaceholderSummary(Account account) =>
        $"Account {account.AccountNumber} ({account.CustomerName}) is currently {account.Status}." +
        (account.FlagReason is not null
            ? $" The recorded flag reason is: {account.FlagReason}. Configure Azure OpenAI to generate a detailed AI summary."
            : " Configure Azure OpenAI credentials in appsettings to enable AI summaries.");

    private static double CosineSimilarity(float[] a, float[] b)
    {
        if (a.Length != b.Length) return 0;
        double dot = 0, magA = 0, magB = 0;
        for (int i = 0; i < a.Length; i++)
        {
            dot += a[i] * b[i];
            magA += a[i] * a[i];
            magB += b[i] * b[i];
        }
        return magA == 0 || magB == 0 ? 0 : dot / (Math.Sqrt(magA) * Math.Sqrt(magB));
    }
}
