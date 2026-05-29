using Application.Common.Results;

namespace Application.Features.ClientVehicleFlows.Errors;

public static class ClientVehicleFlowErrors
{
    public static readonly Error PersonIdInvalid = new("ClientVehicleFlows.PersonIdInvalid", "PersonId must be greater than 0.");
    public static readonly Error PersonNotFound = new("ClientVehicleFlows.PersonNotFound", "Person was not found.");
    public static readonly Error PersonIsNotClientInvalid = new("ClientVehicleFlows.PersonIsNotClientInvalid", "Person does not have an active client role.");
    public static readonly Error DocumentTypeIdInvalid = new("ClientVehicleFlows.DocumentTypeIdInvalid", "DocumentTypeId must be greater than 0.");
    public static readonly Error DocumentTypeNotFound = new("ClientVehicleFlows.DocumentTypeNotFound", "Document type was not found.");
    public static readonly Error DocumentNumberRequired = new("ClientVehicleFlows.DocumentNumberRequired", "Document number is required.");
    public static readonly Error DocumentNumberTooLong = new("ClientVehicleFlows.DocumentNumberTooLong", "Document number cannot exceed 30 characters.");
    public static readonly Error DocumentNumberAlreadyExists = new("ClientVehicleFlows.DocumentNumberAlreadyExists", "Document number already exists.");
    public static readonly Error FirstNameRequired = new("ClientVehicleFlows.FirstNameRequired", "First name is required.");
    public static readonly Error FirstNameTooLong = new("ClientVehicleFlows.FirstNameTooLong", "First name cannot exceed 50 characters.");
    public static readonly Error MiddleNameTooLong = new("ClientVehicleFlows.MiddleNameTooLong", "Middle name cannot exceed 50 characters.");
    public static readonly Error LastNameRequired = new("ClientVehicleFlows.LastNameRequired", "Last name is required.");
    public static readonly Error LastNameTooLong = new("ClientVehicleFlows.LastNameTooLong", "Last name cannot exceed 50 characters.");
    public static readonly Error SecondLastNameTooLong = new("ClientVehicleFlows.SecondLastNameTooLong", "Second last name cannot exceed 50 characters.");
    public static readonly Error BirthDateInvalid = new("ClientVehicleFlows.BirthDateInvalid", "Birth date cannot be in the future.");
    public static readonly Error GenderIdInvalid = new("ClientVehicleFlows.GenderIdInvalid", "GenderId must be greater than 0 when provided.");
    public static readonly Error GenderNotFound = new("ClientVehicleFlows.GenderNotFound", "Gender was not found.");
    public static readonly Error AddressIdInvalid = new("ClientVehicleFlows.AddressIdInvalid", "AddressId must be greater than 0 when provided.");
    public static readonly Error AddressNotFound = new("ClientVehicleFlows.AddressNotFound", "Address was not found.");
    public static readonly Error EmailInvalid = new("ClientVehicleFlows.EmailInvalid", "Email format is invalid.");
    public static readonly Error EmailAlreadyExists = new("ClientVehicleFlows.EmailAlreadyExists", "Email already exists.");
    public static readonly Error PhoneCountryIdRequired = new("ClientVehicleFlows.PhoneCountryIdRequired", "PhoneCountryId is required when phone number is provided.");
    public static readonly Error PhoneCountryNotFound = new("ClientVehicleFlows.PhoneCountryNotFound", "Phone country was not found.");
    public static readonly Error PhoneNumberInvalid = new("ClientVehicleFlows.PhoneNumberInvalid", "Phone number format is invalid.");
    public static readonly Error PhoneNumberTooLong = new("ClientVehicleFlows.PhoneNumberTooLong", "Phone number cannot exceed 20 characters.");
    public static readonly Error PhoneNumberAlreadyExists = new("ClientVehicleFlows.PhoneNumberAlreadyExists", "Phone number already exists.");
    public static readonly Error ClientRoleNotFound = new("ClientVehicleFlows.ClientRoleNotFound", "Client role was not found.");
    public static readonly Error ModelIdInvalid = new("ClientVehicleFlows.ModelIdInvalid", "ModelId must be greater than 0.");
    public static readonly Error VehicleModelNotFound = new("ClientVehicleFlows.VehicleModelNotFound", "Vehicle model was not found.");
    public static readonly Error VehicleTypeIdInvalid = new("ClientVehicleFlows.VehicleTypeIdInvalid", "VehicleTypeId must be greater than 0.");
    public static readonly Error VehicleTypeNotFound = new("ClientVehicleFlows.VehicleTypeNotFound", "Vehicle type was not found.");
    public static readonly Error VinRequired = new("ClientVehicleFlows.VinRequired", "VIN is required.");
    public static readonly Error VinInvalid = new("ClientVehicleFlows.VinInvalid", "VIN format is invalid.");
    public static readonly Error VinAlreadyExists = new("ClientVehicleFlows.VinAlreadyExists", "VIN already exists.");
    public static readonly Error YearInvalid = new("ClientVehicleFlows.YearInvalid", "Year is invalid.");
    public static readonly Error ColorTooLong = new("ClientVehicleFlows.ColorTooLong", "Color cannot exceed 30 characters.");
    public static readonly Error MileageInvalid = new("ClientVehicleFlows.MileageInvalid", "Mileage must be greater than or equal to 0.");
    public static readonly Error VehicleIdInvalid = new("ClientVehicleFlows.VehicleIdInvalid", "VehicleId must be greater than 0.");
    public static readonly Error VehicleNotFound = new("ClientVehicleFlows.VehicleNotFound", "Vehicle was not found.");
    public static readonly Error VehicleInactive = new("ClientVehicleFlows.VehicleInactive", "Vehicle is inactive.");
    public static readonly Error CurrentOwnerNotFound = new("ClientVehicleFlows.CurrentOwnerNotFound", "Current owner record was not found.");
    public static readonly Error SameOwnerTransferConflict = new("ClientVehicleFlows.SameOwnerTransferConflict", "Vehicle is already assigned to this owner.");
    public static readonly Error TransferDateInvalid = new("ClientVehicleFlows.TransferDateInvalid", "TransferDate is invalid.");
}
