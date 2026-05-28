using Application.Common.Results;

namespace Application.Features.Persons.Errors;

public static class PersonErrors
{
    public static readonly Error NotFound = new("Persons.NotFound", "Person was not found.");
    public static readonly Error DocumentTypeIdInvalid = new("Persons.DocumentTypeIdInvalid", "DocumentTypeId must be greater than 0.");
    public static readonly Error DocumentTypeNotFound = new("Persons.DocumentTypeNotFound", "Document type was not found.");
    public static readonly Error DocumentNumberRequired = new("Persons.DocumentNumberRequired", "Document number is required.");
    public static readonly Error DocumentNumberTooLong = new("Persons.DocumentNumberTooLong", "Document number cannot exceed 30 characters.");
    public static readonly Error DocumentNumberAlreadyExists = new("Persons.DocumentNumberAlreadyExists", "Document number already exists.");
    public static readonly Error FirstNameRequired = new("Persons.FirstNameRequired", "First name is required.");
    public static readonly Error FirstNameTooLong = new("Persons.FirstNameTooLong", "First name cannot exceed 50 characters.");
    public static readonly Error MiddleNameTooLong = new("Persons.MiddleNameTooLong", "Middle name cannot exceed 50 characters.");
    public static readonly Error LastNameRequired = new("Persons.LastNameRequired", "Last name is required.");
    public static readonly Error LastNameTooLong = new("Persons.LastNameTooLong", "Last name cannot exceed 50 characters.");
    public static readonly Error SecondLastNameTooLong = new("Persons.SecondLastNameTooLong", "Second last name cannot exceed 50 characters.");
    public static readonly Error BirthDateInvalid = new("Persons.BirthDateInvalid", "Birth date cannot be in the future.");
    public static readonly Error GenderIdInvalid = new("Persons.GenderIdInvalid", "GenderId must be greater than 0.");
    public static readonly Error GenderNotFound = new("Persons.GenderNotFound", "Gender was not found.");
    public static readonly Error AddressIdInvalid = new("Persons.AddressIdInvalid", "AddressId must be greater than 0.");
    public static readonly Error AddressNotFound = new("Persons.AddressNotFound", "Address was not found.");
    public static readonly Error InUse = new("Persons.InUse", "Person is assigned to one or more records.");
}
