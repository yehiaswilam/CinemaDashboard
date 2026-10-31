using CinemaDashboard.Models;

namespace CinemaDashboard.ViewModels;

public record MovieFilter(string? Search, int? CategoryId, int? CinemaId, MovieStatus? Status);
