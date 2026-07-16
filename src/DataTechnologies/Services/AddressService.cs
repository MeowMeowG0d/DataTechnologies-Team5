using DataTechnologies.Data;
using DataTechnologies.Repositories;
using DataTechnologies.Models;
using DataTechnologies.Utils;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
namespace DataTechnologies.Services;


public class AddressService
{
    private readonly AddressRepository _addressRepository;

    public AddressService(DBConnection dbConnection, IFieldCipher cipher)
    {
        _addressRepository = new AddressRepository(dbConnection, cipher);
    }

    private static void ValidateField([NotNull] string? value, Func<string?, bool> isValid, string emptyMessage, string invalidMessage)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException(emptyMessage);

        if (!isValid(value))
            throw new ArgumentException(invalidMessage);
    }

    public int SaveAddress(int userId, string street, string number, string postcode, string city, string country, string? type)
    {
        ValidateField(street, InputWhitelistValidator.IsValidAddressLine, "Straat is verplicht.", "Straat bevat ongeldige tekens.");
        ValidateField(number, InputWhitelistValidator.IsValidHouseNumber, "Huisnummer is verplicht.", "Huisnummer is ongeldig.");
        ValidateField(postcode, InputWhitelistValidator.IsValidPostcode, "Postcode is verplicht.", "Postcode is ongeldig.");
        ValidateField(city, InputWhitelistValidator.IsValidCity, "Stad is verplicht.", "Stad bevat ongeldige tekens.");
        ValidateField(country, InputWhitelistValidator.IsValidCountry, "Land is verplicht.", "Land bevat ongeldige tekens.");

        if (!string.IsNullOrWhiteSpace(type))
            ValidateField(type, InputWhitelistValidator.IsValidAddressLine, "Toevoeging is verplicht.", "Toevoeging bevat ongeldige tekens.");

        var address = new Addresses
        {
            UserID   = userId,
            Street   = street,
            Number   = number,
            Postcode = postcode.ToUpper(CultureInfo.InvariantCulture),
            city     = city,
            Country  = country,
            Type     = type ?? string.Empty
        };

        return _addressRepository.Add(address);
    }

    public Addresses? GetAddressByUserId(int userId)
    {
        return _addressRepository.GetByUserId(userId);
    }
}