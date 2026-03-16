using System.ComponentModel.DataAnnotations;

namespace PharmacySystem.Categories;

public class CreateUpdateCategoryDto
{
    [Required]
    [StringLength(128)]
    public string Name { get; set; } = string.Empty;

    [StringLength(512)]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;
}