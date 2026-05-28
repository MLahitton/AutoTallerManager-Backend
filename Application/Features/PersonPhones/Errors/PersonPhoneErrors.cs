using Application.Common.Results;

namespace Application.Features.PersonPhones.Errors;

public static class PersonPhoneErrors
{
    public static readonly Error NotFound = new("PersonPhones.NotFound", "Person phone was not found.");
    public static readonly Error PersonIdInvalid = new("PersonPhones.PersonIdInvalid", "PersonId must be greater than 0.");
    public static readonly Error PersonNotFound = new("PersonPhones.PersonNotFound", "Person was not found.");
    public static readonly Error CountryIdInvalid = new("PersonPhones.CountryIdInvalid", "CountryId must be greater than 0.");
    public static readonly Error CountryNotFound = new("PersonPhones.CountryNotFound", "Country was not found.");
    public static readonly Error PhoneNumberRequired = new("PersonPhones.PhoneNumberRequired", "Phone number is required.");
    public static readonly Error PhoneNumberTooLong = new("PersonPhones.PhoneNumberTooLong", "Phone number cannot exceed 20 characters.");
    public static readonly Error PhoneNumberInvalid = new("PersonPhones.PhoneNumberInvalid", "Phone number format is invalid.");
    public static readonly Error PhoneNumberAlreadyExists = new("PersonPhones.PhoneNumberAlreadyExists", "Phone number already exists for the selected country.");
    public static readonly Error PrimaryAlreadyExists = new("PersonPhones.PrimaryAlreadyExists", "A primary phone already exists for this person.");
}
