using System.ComponentModel.DataAnnotations;

namespace ReconciliationDashboard.Api.Models.Dtos;

public record UpdateAccountRequest(
    [Required, StringLength(200)] string CustomerName,
    [Required] string Status,
    [StringLength(500)] string? FlagReason
);
