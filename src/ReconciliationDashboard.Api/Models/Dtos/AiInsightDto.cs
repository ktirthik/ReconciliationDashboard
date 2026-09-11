namespace ReconciliationDashboard.Api.Models.Dtos;

public record AiInsightDto(
    string Summary,
    IReadOnlyList<SimilarCaseDto> SimilarCases
);

public record SimilarCaseDto(
    Guid Id,
    string Title,
    string Content,
    string Tags,
    double Similarity
);
