namespace ReconciliationDashboard.Api.Models;

public class CaseNote
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Tags { get; set; } = string.Empty;
    // Cosine-similarity RAG: store the embedding as JSON so no vector DB is needed
    // for this scale. Swap for pgvector / Azure AI Search in production.
    public string? EmbeddingJson { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
