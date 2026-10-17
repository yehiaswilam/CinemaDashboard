using System.ComponentModel.DataAnnotations;
using CinemaDashboard.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CinemaDashboard.ViewModels;

public class MovieFormVM
{
    public int Id { get; set; }

    [Required, MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(2000)]
    [Display(Name = "Description")]
    public string Description { get; set; } = string.Empty;

    [Range(0, 100000, ErrorMessage = "Price must be between 0 and 100,000.")]
    public decimal Price { get; set; }

    public MovieStatus Status { get; set; } = MovieStatus.Upcoming;

    [Required]
    [Display(Name = "Show Date & Time")]
    public DateTime DateTime { get; set; } = DateTime.Today.AddDays(1).AddHours(20);

    [Range(1, int.MaxValue, ErrorMessage = "Please select a category.")]
    [Display(Name = "Category")]
    public int CategoryId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Please select a cinema.")]
    [Display(Name = "Cinema")]
    public int CinemaId { get; set; }

    [Display(Name = "Actors")]
    public List<int> ActorIds { get; set; } = [];

    // ---------- Uploads ----------
    [Display(Name = "Main Image")]
    public IFormFile? MainImgFile { get; set; }

    [Display(Name = "Sub Images")]
    public List<IFormFile> SubImageFiles { get; set; } = [];

    // Sub images the user ticked for removal (Edit only)
    public List<int> RemoveSubImageIds { get; set; } = [];

    // ---------- Display-only data (not posted back) ----------
    [ValidateNever]
    public string? ExistingMainImg { get; set; }

    [ValidateNever]
    public List<MovieImage> ExistingSubImages { get; set; } = [];

    [ValidateNever]
    public IEnumerable<SelectListItem> Categories { get; set; } = [];

    [ValidateNever]
    public IEnumerable<SelectListItem> Cinemas { get; set; } = [];

    [ValidateNever]
    public List<ActorOption> Actors { get; set; } = [];
}

public record ActorOption(int Id, string Name, string? Img);
