using System.ComponentModel.DataAnnotations;

namespace ReconciliationDashboard.Api.Models.Dtos;

public record CreateAccountRequest(
    [Required, StringLength(50)] string AccountNumber,
    [Required, StringLength(200)] string CustomerName
);
