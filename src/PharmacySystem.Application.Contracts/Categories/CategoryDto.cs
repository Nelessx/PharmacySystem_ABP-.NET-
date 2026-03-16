using System;
using Volo.Abp.Application.Dtos;

namespace PharmacySystem.Categories;

public class CategoryDto : FullAuditedEntityDto<Guid>
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
}