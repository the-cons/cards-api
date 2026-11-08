using System.ComponentModel.DataAnnotations;

namespace Cards.Api.Models;

public record CreateCardRequest(
    [Required, StringLength(100, MinimumLength = 1)] string Name,
    [Required, StringLength(50)] string Type,
    [Required, Range(0, 5000)] int? Attack);