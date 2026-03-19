using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace PharmacySystem.Suppliers;

// Supplier entity represents a medicine vendor/supplier
public class Supplier : FullAuditedAggregateRoot<Guid>
{
    // Supplier/company name
    public string Name { get; private set; } = null!;

    // Contact person name - optional
    public string? ContactPerson { get; private set; }

    // Supplier phone number - optional
    public string? Phone { get; private set; }

    // Supplier email - optional
    public string? Email { get; private set; }

    // Supplier address - optional
    public string? Address { get; private set; }

    // Indicates whether the supplier is active
    public bool IsActive { get; private set; }

    // Required by EF Core
    protected Supplier()
    {
    }

    // Main constructor for creating a supplier
    public Supplier(
        Guid id,
        string name,
        string? contactPerson = null,
        string? phone = null,
        string? email = null,
        string? address = null,
        bool isActive = true
    ) : base(id)
    {
        SetName(name);
        SetContactPerson(contactPerson);
        SetPhone(phone);
        SetEmail(email);
        SetAddress(address);
        SetIsActive(isActive);
    }

    // Validates and sets supplier name
    public void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Supplier name cannot be empty.", nameof(name));
        }

        Name = name.Trim();
    }

    // Sets optional contact person
    public void SetContactPerson(string? contactPerson)
    {
        ContactPerson = string.IsNullOrWhiteSpace(contactPerson)
            ? null
            : contactPerson.Trim();
    }

    // Sets optional phone
    public void SetPhone(string? phone)
    {
        Phone = string.IsNullOrWhiteSpace(phone)
            ? null
            : phone.Trim();
    }

    // Sets optional email
    public void SetEmail(string? email)
    {
        Email = string.IsNullOrWhiteSpace(email)
            ? null
            : email.Trim();
    }

    // Sets optional address
    public void SetAddress(string? address)
    {
        Address = string.IsNullOrWhiteSpace(address)
            ? null
            : address.Trim();
    }

    // Enables or disables the supplier
    public void SetIsActive(bool isActive)
    {
        IsActive = isActive;
    }
}