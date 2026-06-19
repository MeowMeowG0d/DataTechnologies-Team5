using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Xunit;
using DataTechnologies.Models;

namespace AddressTests
{
    public class AddressModelTests
    {
        private static Addresses CreateValidAddress() => new()
        {
            UserID   = 1,
            Street   = "Main Street",
            Number   = "42",
            Postcode = "1234AB",
            city     = "Amsterdam",
            Country  = "Netherlands",
            Type     = "Home"
        };

        private static IList<ValidationResult> Validate(Addresses address)
        {
            var results = new List<ValidationResult>();
            Validator.TryValidateObject(address, new ValidationContext(address), results, validateAllProperties: true);
            return results;
        }

        [Fact]
        public void Create_SetsAllProvidedFields()
        {
            var address = CreateValidAddress();

            Assert.Equal(1, address.UserID);
            Assert.Equal("Main Street", address.Street);
            Assert.Equal("42", address.Number);
            Assert.Equal("1234AB", address.Postcode);
            Assert.Equal("Amsterdam", address.city);
            Assert.Equal("Netherlands", address.Country);
            Assert.Equal("Home", address.Type);
        }

        [Fact]
        public void ValidAddress_PassesValidation()
        {
            Assert.Empty(Validate(CreateValidAddress()));
        }

        [Fact]
        public void MissingPostcode_FailsValidation()
        {
            var address = CreateValidAddress();
            address.Postcode = "";

            Assert.Contains(Validate(address), e => e.MemberNames.Contains(nameof(Addresses.Postcode)));
        }

        [Theory]
        [InlineData("Street")]
        [InlineData("Number")]
        [InlineData("Postcode")]
        [InlineData("city")]
        [InlineData("Country")]
        public void MissingRequiredField_FailsValidation(string field)
        {
            var address = CreateValidAddress();
            switch (field)
            {
                case "Street":   address.Street   = ""; break;
                case "Number":   address.Number   = ""; break;
                case "Postcode": address.Postcode = ""; break;
                case "city":     address.city     = ""; break;
                case "Country":  address.Country  = ""; break;
            }

            Assert.Contains(Validate(address), e => e.MemberNames.Contains(field));
        }

        [Fact]
        public void MissingType_IsStillValid()
        {
            var address = CreateValidAddress();
            address.Type = "";

            Assert.Empty(Validate(address));
        }
    }
}
