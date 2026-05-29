using Application.Common.Results;

namespace Application.Features.Auth.Errors;

public static class AuthErrors
{
    public static readonly Error EmailRequired = new("Auth.EmailRequired", "Email is required.");
    public static readonly Error EmailInvalid = new("Auth.EmailInvalid", "Email format is invalid.");
    public static readonly Error EmailAlreadyExists = new("Auth.EmailAlreadyExists", "Email already exists.");
    public static readonly Error PasswordRequired = new("Auth.PasswordRequired", "Password is required.");
    public static readonly Error PasswordTooShort = new("Auth.PasswordTooShort", "Password must be at least 8 characters long.");
    public static readonly Error PasswordTooLong = new("Auth.PasswordTooLong", "Password cannot exceed 100 characters.");
    public static readonly Error InvalidCredentials = new("Auth.InvalidCredentials", "Invalid credentials.");
    public static readonly Error UserInactiveInvalid = new("Auth.UserInactiveInvalid", "User is inactive.");
    public static readonly Error RefreshTokenRequired = new("Auth.RefreshTokenRequired", "Refresh token is required.");
    public static readonly Error RefreshTokenInvalid = new("Auth.RefreshTokenInvalid", "Refresh token is invalid.");
    public static readonly Error RefreshTokenExpired = new("Auth.RefreshTokenExpired", "Refresh token has expired.");
    public static readonly Error UserEmailNotFound = new("Auth.UserEmailNotFound", "User email could not be resolved.");
    public static readonly Error DocumentTypeIdInvalid = new("Auth.DocumentTypeIdInvalid", "DocumentTypeId must be greater than 0.");
    public static readonly Error DocumentTypeNotFound = new("Auth.DocumentTypeNotFound", "Document type was not found.");
    public static readonly Error DocumentNumberRequired = new("Auth.DocumentNumberRequired", "Document number is required.");
    public static readonly Error DocumentNumberTooLong = new("Auth.DocumentNumberTooLong", "Document number cannot exceed 30 characters.");
    public static readonly Error DocumentNumberAlreadyExists = new("Auth.DocumentNumberAlreadyExists", "Document number already exists.");
    public static readonly Error FirstNameRequired = new("Auth.FirstNameRequired", "First name is required.");
    public static readonly Error FirstNameTooLong = new("Auth.FirstNameTooLong", "First name cannot exceed 50 characters.");
    public static readonly Error MiddleNameTooLong = new("Auth.MiddleNameTooLong", "Middle name cannot exceed 50 characters.");
    public static readonly Error LastNameRequired = new("Auth.LastNameRequired", "Last name is required.");
    public static readonly Error LastNameTooLong = new("Auth.LastNameTooLong", "Last name cannot exceed 50 characters.");
    public static readonly Error SecondLastNameTooLong = new("Auth.SecondLastNameTooLong", "Second last name cannot exceed 50 characters.");
    public static readonly Error BirthDateInvalid = new("Auth.BirthDateInvalid", "Birth date cannot be in the future.");
    public static readonly Error GenderIdInvalid = new("Auth.GenderIdInvalid", "GenderId must be greater than 0 when provided.");
    public static readonly Error GenderNotFound = new("Auth.GenderNotFound", "Gender was not found.");
    public static readonly Error AddressIdInvalid = new("Auth.AddressIdInvalid", "AddressId must be greater than 0 when provided.");
    public static readonly Error AddressNotFound = new("Auth.AddressNotFound", "Address was not found.");
    public static readonly Error ClientRoleNotFound = new("Auth.ClientRoleNotFound", "Client role was not found.");
    public static readonly Error PhoneCountryIdRequired = new("Auth.PhoneCountryIdRequired", "PhoneCountryId is required when phone number is provided.");
    public static readonly Error PhoneCountryNotFound = new("Auth.PhoneCountryNotFound", "Phone country was not found.");
    public static readonly Error PhoneNumberInvalid = new("Auth.PhoneNumberInvalid", "Phone number format is invalid.");
    public static readonly Error PhoneNumberTooLong = new("Auth.PhoneNumberTooLong", "Phone number cannot exceed 20 characters.");
    public static readonly Error PhoneNumberAlreadyExists = new("Auth.PhoneNumberAlreadyExists", "Phone number already exists.");
}
