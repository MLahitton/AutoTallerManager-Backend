using Application.Common.Results;

namespace Application.Features.Staff.Errors;

public static class StaffErrors
{
    public static readonly Error UserIdInvalid = new("Staff.UserIdInvalid", "UserId must be greater than 0.");
    public static readonly Error UserNotFound = new("Staff.UserNotFound", "User was not found.");
    public static readonly Error CannotDeactivateCurrentUserConflict = new("Staff.CannotDeactivateCurrentUserConflict", "Current user cannot deactivate themselves.");
    public static readonly Error DocumentTypeIdInvalid = new("Staff.DocumentTypeIdInvalid", "DocumentTypeId must be greater than 0.");
    public static readonly Error DocumentTypeNotFound = new("Staff.DocumentTypeNotFound", "Document type was not found.");
    public static readonly Error DocumentNumberRequired = new("Staff.DocumentNumberRequired", "Document number is required.");
    public static readonly Error DocumentNumberTooLong = new("Staff.DocumentNumberTooLong", "Document number cannot exceed 30 characters.");
    public static readonly Error DocumentNumberAlreadyExists = new("Staff.DocumentNumberAlreadyExists", "Document number already exists.");
    public static readonly Error FirstNameRequired = new("Staff.FirstNameRequired", "First name is required.");
    public static readonly Error FirstNameTooLong = new("Staff.FirstNameTooLong", "First name cannot exceed 50 characters.");
    public static readonly Error MiddleNameTooLong = new("Staff.MiddleNameTooLong", "Middle name cannot exceed 50 characters.");
    public static readonly Error LastNameRequired = new("Staff.LastNameRequired", "Last name is required.");
    public static readonly Error LastNameTooLong = new("Staff.LastNameTooLong", "Last name cannot exceed 50 characters.");
    public static readonly Error SecondLastNameTooLong = new("Staff.SecondLastNameTooLong", "Second last name cannot exceed 50 characters.");
    public static readonly Error BirthDateInvalid = new("Staff.BirthDateInvalid", "Birth date cannot be in the future.");
    public static readonly Error GenderIdInvalid = new("Staff.GenderIdInvalid", "GenderId must be greater than 0 when provided.");
    public static readonly Error GenderNotFound = new("Staff.GenderNotFound", "Gender was not found.");
    public static readonly Error AddressIdInvalid = new("Staff.AddressIdInvalid", "AddressId must be greater than 0 when provided.");
    public static readonly Error AddressNotFound = new("Staff.AddressNotFound", "Address was not found.");
    public static readonly Error EmailRequired = new("Staff.EmailRequired", "Email is required.");
    public static readonly Error EmailInvalid = new("Staff.EmailInvalid", "Email format is invalid.");
    public static readonly Error EmailAlreadyExists = new("Staff.EmailAlreadyExists", "Email already exists.");
    public static readonly Error PasswordRequired = new("Staff.PasswordRequired", "Password is required.");
    public static readonly Error PasswordTooShort = new("Staff.PasswordTooShort", "Password must be at least 8 characters long.");
    public static readonly Error PasswordTooLong = new("Staff.PasswordTooLong", "Password cannot exceed 100 characters.");
    public static readonly Error PhoneCountryIdRequired = new("Staff.PhoneCountryIdRequired", "PhoneCountryId is required when phone number is provided.");
    public static readonly Error PhoneCountryNotFound = new("Staff.PhoneCountryNotFound", "Phone country was not found.");
    public static readonly Error PhoneNumberInvalid = new("Staff.PhoneNumberInvalid", "Phone number format is invalid.");
    public static readonly Error PhoneNumberTooLong = new("Staff.PhoneNumberTooLong", "Phone number cannot exceed 20 characters.");
    public static readonly Error PhoneNumberAlreadyExists = new("Staff.PhoneNumberAlreadyExists", "Phone number already exists.");
    public static readonly Error RoleNameRequired = new("Staff.RoleNameRequired", "Role name is required.");
    public static readonly Error StaffRoleInvalid = new("Staff.StaffRoleInvalid", "Role name must be Admin, Receptionist, or Mechanic.");
    public static readonly Error StaffRoleNotFound = new("Staff.StaffRoleNotFound", "Requested staff role was not found.");
    public static readonly Error SpecialtiesOnlyAllowedForMechanicInvalid = new("Staff.SpecialtiesOnlyAllowedForMechanicInvalid", "Specialties are only allowed for Mechanic role.");
    public static readonly Error PersonIdInvalid = new("Staff.PersonIdInvalid", "PersonId must be greater than 0.");
    public static readonly Error PersonNotFound = new("Staff.PersonNotFound", "Person was not found.");
    public static readonly Error PersonIsNotMechanicInvalid = new("Staff.PersonIsNotMechanicInvalid", "Person does not have an active mechanic role.");
    public static readonly Error SpecialtyIdInvalid = new("Staff.SpecialtyIdInvalid", "SpecialtyId must be greater than 0.");
    public static readonly Error SpecialtyNotFound = new("Staff.SpecialtyNotFound", "Mechanic specialty was not found.");
    public static readonly Error DuplicateSpecialtyConflict = new("Staff.DuplicateSpecialtyConflict", "Duplicate specialties are not allowed.");
    public static readonly Error MechanicSpecialtyInUseConflict = new("Staff.MechanicSpecialtyInUseConflict", "Cannot remove mechanic specialty because it is in use.");
}
