using DataTechnologies.Data;
using DataTechnologies.Repositories;
using DataTechnologies.Models;
using System.Globalization;
namespace DataTechnologies.Services;


public class AddressService
{
    private readonly AddressRepository _addressRepository;

    public AddressService(DBConnection dbConnection, IFieldCipher cipher)
    {
        _addressRepository = new AddressRepository(dbConnection, cipher);
    }

    public int SaveAddress(int userId, string street, string number, string postcode, string city, string country, string? type)
    {
        if (string.IsNullOrWhiteSpace(street))   throw new ArgumentException("Straat is verplicht.");
        if (string.IsNullOrWhiteSpace(number))   throw new ArgumentException("Huisnummer is verplicht.");
        if (string.IsNullOrWhiteSpace(postcode)) throw new ArgumentException("Postcode is verplicht.");
        if (string.IsNullOrWhiteSpace(city))     throw new ArgumentException("Stad is verplicht.");
        if (string.IsNullOrWhiteSpace(country))  throw new ArgumentException("Land is verplicht.");

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