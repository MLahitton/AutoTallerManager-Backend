using Application.Common.Results;

namespace Application.Features.Addresses.Errors;

public static class AddressErrors
{
    public static readonly Error NotFound = new("Addresses.NotFound", "Address was not found.");
    public static readonly Error NeighborhoodNotFound = new("Addresses.NeighborhoodNotFound", "Neighborhood was not found.");
    public static readonly Error NeighborhoodIdInvalid = new("Addresses.NeighborhoodIdInvalid", "NeighborhoodId must be greater than 0.");
    public static readonly Error StreetTypeNotFound = new("Addresses.StreetTypeNotFound", "Street type was not found.");
    public static readonly Error StreetTypeIdInvalid = new("Addresses.StreetTypeIdInvalid", "StreetTypeId must be greater than 0.");
    public static readonly Error MainNumberTooLong = new("Addresses.MainNumberTooLong", "Main number cannot exceed 10 characters.");
    public static readonly Error SecondaryNumberTooLong = new("Addresses.SecondaryNumberTooLong", "Secondary number cannot exceed 10 characters.");
    public static readonly Error TertiaryNumberTooLong = new("Addresses.TertiaryNumberTooLong", "Tertiary number cannot exceed 10 characters.");
    public static readonly Error ComplementTooLong = new("Addresses.ComplementTooLong", "Complement cannot exceed 150 characters.");
    public static readonly Error InUse = new("Addresses.InUse", "Address is assigned to one or more persons.");
}
