using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace PharmacySystem.Customers;

// Customer entity represents a pharmacy customer/patient
public class Customer : FullAuditedAggregateRoot<Guid>
{
    // Customer full name
    public string Name { get; private set; } = null!;

    // Phone number - optional
    public string? Phone { get; private set; }

    // Address - optional
    public string? Address { get; private set; }

    // Gender - optional
    public string? Gender { get; private set; }

    // Date of birth - optional
    public DateTime? DateOfBirth { get; private set; }

    // Unique patient/customer code - optional
    public string? PatientCode { get; private set; }

    // Indicates whether the customer is active
    public bool IsActive { get; private set; }

    // Required by EF Core
    protected Customer()
    {
    }

    // Main constructor for creating a customer
    public Customer(
        Guid id,
        string name,
        string? phone = null,
        string? address = null,
        string? gender = null,
        DateTime? dateOfBirth = null,
        string? patientCode = null,
        bool isActive = true
    ) : base(id)
    {
        SetName(name);
        SetPhone(phone);
        SetAddress(address);
        SetGender(gender);
        SetDateOfBirth(dateOfBirth);
        SetPatientCode(patientCode);
        SetIsActive(isActive);
    }

    // Validates and sets customer name
    public void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Customer name cannot be empty.", nameof(name));
        }

        Name = name.Trim();
    }

    // Sets optional phone
    public void SetPhone(string? phone)
    {
        Phone = string.IsNullOrWhiteSpace(phone)
            ? null
            : phone.Trim();
    }

    // Sets optional address
    public void SetAddress(string? address)
    {
        Address = string.IsNullOrWhiteSpace(address)
            ? null
            : address.Trim();
    }

    // Sets optional gender
    public void SetGender(string? gender)
    {
        Gender = string.IsNullOrWhiteSpace(gender)
            ? null
            : gender.Trim();
    }

    // Sets optional date of birth
    public void SetDateOfBirth(DateTime? dateOfBirth)
    {
        DateOfBirth = dateOfBirth;
    }

    // Sets optional patient code
    public void SetPatientCode(string? patientCode)
    {
        PatientCode = string.IsNullOrWhiteSpace(patientCode)
            ? null
            : patientCode.Trim();
    }

    // Enables or disables the customer
    public void SetIsActive(bool isActive)
    {
        IsActive = isActive;
    }
}