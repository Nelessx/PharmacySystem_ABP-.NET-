using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace PharmacySystem.Categories;

public class Category : FullAuditedAggregateRoot<Guid>
{
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public bool IsActive { get; private set; }

    protected Category()
    {
    }

    public Category(Guid id, string name, string? description = null, bool isActive = true)
        : base(id)
    {
        SetName(name);
        SetDescription(description);
        IsActive = isActive;
    }

    public void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Category name cannot be empty.", nameof(name));
        }

        Name = name.Trim();
    }

    public void SetDescription(string? description)
    {
        Description = string.IsNullOrWhiteSpace(description)
            ? null
            : description.Trim();
    }

    public void SetIsActive(bool isActive)
    {
        IsActive = isActive;
    }
}