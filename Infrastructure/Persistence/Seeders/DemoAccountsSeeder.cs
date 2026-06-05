using Application.Common.Security;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Seeders;

public static class DemoAccountsSeeder
{
    private const string DemoPassword = "Testing123*";
    private const string DocumentTypeCode = "CC";
    private const string DefaultCountryName = "Colombia";
    private const string DefaultVehicleBrandName = "Renault";
    private const string DefaultVehicleModelName = "Logan";
    private const string DefaultVehicleTypeName = "Sedan";
    private const string DefaultMechanicSpecialtyName = "General Mechanics";

    private static readonly DemoAccount[] DemoAccounts =
    [
        new(
            FirstName: "Tomas",
            LastName: "Medina",
            DocumentNumber: "100000001",
            Email: "tmedina@gmail.com",
            PhoneNumber: "3000000001",
            RoleName: "Receptionist"),
        new(
            FirstName: "Nicolas",
            LastName: "Zabala",
            DocumentNumber: "100000002",
            Email: "nzabala1@gmail.com",
            PhoneNumber: "3000000002",
            RoleName: "Mechanic"),
        new(
            FirstName: "Rolo",
            LastName: "Escobar",
            DocumentNumber: "100000003",
            Email: "rolito1@gmail.com",
            PhoneNumber: "3000000003",
            RoleName: "Client")
    ];

    public static async Task SeedAsync(
        AppDbContext context,
        IPasswordHasher passwordHasher,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(passwordHasher);

        var documentType = await context.DocumentTypes
            .FirstOrDefaultAsync(x => x.Code == DocumentTypeCode, cancellationToken);
        if (documentType is null)
        {
            return;
        }

        var roles = await context.Roles.ToListAsync(cancellationToken);
        var country = await context.Countries
            .FirstOrDefaultAsync(x => x.Name == DefaultCountryName, cancellationToken);

        foreach (var account in DemoAccounts)
        {
            var role = roles.FirstOrDefault(x =>
                x.RoleName.Equals(account.RoleName, StringComparison.OrdinalIgnoreCase));
            if (role is null)
            {
                continue;
            }

            var person = await EnsurePersonAsync(
                context,
                account,
                documentType.DocumentTypeId,
                cancellationToken);

            await EnsureEmailAsync(context, person, account.Email, cancellationToken);

            if (country is not null)
            {
                await EnsurePhoneAsync(
                    context,
                    person,
                    account.PhoneNumber,
                    country.CountryId,
                    cancellationToken);
            }

            await EnsureUserAsync(context, person, passwordHasher, cancellationToken);
            await EnsureExpectedRoleAsync(context, person, role, cancellationToken);

            if (account.RoleName.Equals("Mechanic", StringComparison.OrdinalIgnoreCase))
            {
                await EnsureMechanicSpecialtyAsync(context, person, cancellationToken);
            }

            if (account.RoleName.Equals("Client", StringComparison.OrdinalIgnoreCase))
            {
                await EnsureClientVehicleAsync(context, person, cancellationToken);
            }

            await context.SaveChangesAsync(cancellationToken);
        }
    }

    private static async Task<Person> EnsurePersonAsync(
        AppDbContext context,
        DemoAccount account,
        int documentTypeId,
        CancellationToken cancellationToken)
    {
        var emailParts = SplitEmail(account.Email);

        var person = await context.PersonEmails
            .Where(x => x.EmailUser == emailParts.User && x.EmailDomain.Domain == emailParts.Domain)
            .Select(x => x.Person)
            .FirstOrDefaultAsync(cancellationToken);

        if (person is null)
        {
            person = await context.Persons
                .FirstOrDefaultAsync(x => x.DocumentNumber == account.DocumentNumber, cancellationToken);
        }

        if (person is null)
        {
            person = new Person
            {
                DocumentTypeId = documentTypeId,
                DocumentNumber = account.DocumentNumber,
                FirstName = account.FirstName,
                LastName = account.LastName,
                BirthDate = null,
                GenderId = null,
                AddressId = null
            };

            await context.Persons.AddAsync(person, cancellationToken);
        }
        else
        {
            person.DocumentTypeId = documentTypeId;
            person.DocumentNumber = account.DocumentNumber;
            person.FirstName = account.FirstName;
            person.LastName = account.LastName;
        }

        return person;
    }

    private static async Task EnsureEmailAsync(
        AppDbContext context,
        Person person,
        string email,
        CancellationToken cancellationToken)
    {
        var emailParts = SplitEmail(email);
        var emailDomain = await context.EmailDomains
            .FirstOrDefaultAsync(x => x.Domain == emailParts.Domain, cancellationToken);

        if (emailDomain is null)
        {
            emailDomain = new EmailDomain
            {
                Domain = emailParts.Domain
            };

            await context.EmailDomains.AddAsync(emailDomain, cancellationToken);
        }

        var emailExists = await context.PersonEmails.AnyAsync(
            x => x.PersonId == person.PersonId &&
                 x.EmailUser == emailParts.User &&
                 x.EmailDomain.Domain == emailParts.Domain,
            cancellationToken);

        if (emailExists)
        {
            return;
        }

        var personEmail = new PersonEmail
        {
            Person = person,
            EmailUser = emailParts.User,
            IsPrimary = true
        };

        if (emailDomain.EmailDomainId > 0)
        {
            personEmail.EmailDomainId = emailDomain.EmailDomainId;
        }
        else
        {
            personEmail.EmailDomain = emailDomain;
        }

        await context.PersonEmails.AddAsync(personEmail, cancellationToken);
    }

    private static async Task EnsurePhoneAsync(
        AppDbContext context,
        Person person,
        string phoneNumber,
        int countryId,
        CancellationToken cancellationToken)
    {
        var phoneExists = await context.PersonPhones.AnyAsync(
            x => x.PersonId == person.PersonId &&
                 x.CountryId == countryId &&
                 x.PhoneNumber == phoneNumber,
            cancellationToken);

        if (phoneExists)
        {
            return;
        }

        await context.PersonPhones.AddAsync(
            new PersonPhone
            {
                Person = person,
                CountryId = countryId,
                PhoneNumber = phoneNumber,
                IsPrimary = true
            },
            cancellationToken);
    }

    private static async Task EnsureUserAsync(
        AppDbContext context,
        Person person,
        IPasswordHasher passwordHasher,
        CancellationToken cancellationToken)
    {
        var user = await context.Users
            .FirstOrDefaultAsync(x => x.PersonId == person.PersonId, cancellationToken);

        if (user is null)
        {
            await context.Users.AddAsync(
                new User
                {
                    Person = person,
                    PasswordHash = passwordHasher.Hash(DemoPassword),
                    RefreshToken = null,
                    RefreshTokenExpiration = null,
                    IsActive = true
                },
                cancellationToken);

            return;
        }

        user.IsActive = true;
        user.RefreshToken = null;
        user.RefreshTokenExpiration = null;

        if (!passwordHasher.Verify(DemoPassword, user.PasswordHash))
        {
            user.PasswordHash = passwordHasher.Hash(DemoPassword);
        }
    }

    private static async Task EnsureExpectedRoleAsync(
        AppDbContext context,
        Person person,
        Role expectedRole,
        CancellationToken cancellationToken)
    {
        var personRoles = await context.PersonRoles
            .Where(x => x.PersonId == person.PersonId)
            .ToListAsync(cancellationToken);

        foreach (var personRole in personRoles)
        {
            personRole.IsActive = personRole.RoleId == expectedRole.RoleId;
        }

        if (personRoles.All(x => x.RoleId != expectedRole.RoleId))
        {
            await context.PersonRoles.AddAsync(
                new PersonRole
                {
                    Person = person,
                    RoleId = expectedRole.RoleId,
                    IsActive = true
                },
                cancellationToken);
        }
    }

    private static async Task EnsureMechanicSpecialtyAsync(
        AppDbContext context,
        Person person,
        CancellationToken cancellationToken)
    {
        var specialty = await context.MechanicSpecialties
            .FirstOrDefaultAsync(
                x => x.Name == DefaultMechanicSpecialtyName ||
                     x.Name == "Preventive Maintenance" ||
                     x.Name == "Brakes",
                cancellationToken);

        if (specialty is null)
        {
            return;
        }

        var assignmentExists = await context.MechanicSpecialtyAssignments.AnyAsync(
            x => x.PersonId == person.PersonId && x.SpecialtyId == specialty.SpecialtyId,
            cancellationToken);

        if (assignmentExists)
        {
            return;
        }

        await context.MechanicSpecialtyAssignments.AddAsync(
            new MechanicSpecialtyAssignment
            {
                Person = person,
                SpecialtyId = specialty.SpecialtyId
            },
            cancellationToken);
    }

    private static async Task EnsureClientVehicleAsync(
        AppDbContext context,
        Person person,
        CancellationToken cancellationToken)
    {
        var vehicleType = await context.VehicleTypes
            .OrderByDescending(x => x.Name == DefaultVehicleTypeName)
            .FirstOrDefaultAsync(cancellationToken);
        var vehicleModel = await context.VehicleModels
            .Include(x => x.Brand)
            .OrderByDescending(x =>
                x.ModelName == DefaultVehicleModelName &&
                x.Brand.BrandName == DefaultVehicleBrandName)
            .FirstOrDefaultAsync(cancellationToken);

        if (vehicleType is null || vehicleModel is null)
        {
            return;
        }

        var vehicle = await context.Vehicles
            .FirstOrDefaultAsync(
                x => x.Plate == "ROL123" || x.VIN == "9BWZZZ377VT004251",
                cancellationToken);

        if (vehicle is null)
        {
            vehicle = new Vehicle
            {
                ModelId = vehicleModel.ModelId,
                VehicleTypeId = vehicleType.VehicleTypeId,
                Plate = "ROL123",
                VIN = "9BWZZZ377VT004251",
                Year = 2020,
                Color = "Gray",
                Mileage = 45000,
                IsActive = true
            };

            await context.Vehicles.AddAsync(vehicle, cancellationToken);
        }
        else
        {
            vehicle.IsActive = true;
        }

        var roloOwnsVehicle = await context.VehicleOwnerHistories.AnyAsync(
            x => x.PersonId == person.PersonId &&
                 x.VehicleId == vehicle.VehicleId &&
                 x.EndDate == null,
            cancellationToken);

        if (roloOwnsVehicle)
        {
            return;
        }

        var hasAnotherActiveOwner = await context.VehicleOwnerHistories.AnyAsync(
            x => x.VehicleId == vehicle.VehicleId &&
                 x.PersonId != person.PersonId &&
                 x.EndDate == null,
            cancellationToken);

        if (hasAnotherActiveOwner)
        {
            return;
        }

        await context.VehicleOwnerHistories.AddAsync(
            new VehicleOwnerHistory
            {
                Vehicle = vehicle,
                Person = person,
                StartDate = DateTime.UtcNow.Date,
                EndDate = null
            },
            cancellationToken);
    }

    private static (string User, string Domain) SplitEmail(string email)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        var atIndex = normalizedEmail.IndexOf('@');

        return (normalizedEmail[..atIndex], normalizedEmail[(atIndex + 1)..]);
    }

    private sealed record DemoAccount(
        string FirstName,
        string LastName,
        string DocumentNumber,
        string Email,
        string PhoneNumber,
        string RoleName);
}
