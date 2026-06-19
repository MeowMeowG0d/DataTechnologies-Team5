using System;
using System.Security.Cryptography;
using Xunit;
using DataTechnologies.Services;

namespace UserTests
{
    public class CryptoServiceTests
    {
        private static CryptoService CreateCipher() =>
            new(RandomNumberGenerator.GetBytes(32), RandomNumberGenerator.GetBytes(32));

        [Fact]
        public void Encrypt_ThenDecrypt_ReturnsOriginal()
        {
            var cipher = CreateCipher();
            const string plaintext = "jan.jansen@voorbeeld.nl";

            string decrypted = cipher.Decrypt(cipher.Encrypt(plaintext));

            Assert.Equal(plaintext, decrypted);
        }

        [Fact]
        public void Encrypt_OutputIsNotReadablePlaintext()
        {
            var cipher = CreateCipher();
            const string plaintext = "Gevoelige Naam";

            string encrypted = cipher.Encrypt(plaintext);

            Assert.DoesNotContain(plaintext, encrypted);
            Assert.StartsWith("v1:", encrypted);
        }

        [Fact]
        public void Encrypt_SameValueTwice_ProducesDifferentCiphertext()
        {
            var cipher = CreateCipher();
            const string plaintext = "herhaalde waarde";

            string first = cipher.Encrypt(plaintext);
            string second = cipher.Encrypt(plaintext);

            Assert.NotEqual(first, second);
            Assert.Equal(plaintext, cipher.Decrypt(first));
            Assert.Equal(plaintext, cipher.Decrypt(second));
        }

        [Fact]
        public void Decrypt_TamperedCiphertext_Throws()
        {
            var cipher = CreateCipher();
            char[] chars = cipher.Encrypt("integriteit").ToCharArray();
            int last = chars.Length - 1;
            if (chars[last] == 'A')
                chars[last] = 'B';
            else
                chars[last] = 'A';

            Assert.ThrowsAny<CryptographicException>(() => cipher.Decrypt(new string(chars)));
        }

        [Fact]
        public void Decrypt_ValueWithoutScheme_ThrowsFormatException()
        {
            var cipher = CreateCipher();
            Assert.Throws<FormatException>(() => cipher.Decrypt("gewoon platte tekst"));
        }

        [Fact]
        public void Decrypt_WithDifferentKey_Throws()
        {
            string encrypted = CreateCipher().Encrypt("geheim");
            Assert.ThrowsAny<CryptographicException>(() => CreateCipher().Decrypt(encrypted));
        }

        [Fact]
        public void BlindIndex_IsDeterministic_AndCaseInsensitive()
        {
            var cipher = CreateCipher();

            string a = cipher.BlindIndex("User@Example.com");
            string b = cipher.BlindIndex("  user@example.COM  ");

            Assert.Equal(a, b);
        }

        [Fact]
        public void BlindIndex_DifferentValues_ProduceDifferentIndexes()
        {
            var cipher = CreateCipher();
            Assert.NotEqual(cipher.BlindIndex("a@example.com"), cipher.BlindIndex("b@example.com"));
        }

        [Fact]
        public void Constructor_WrongKeySize_Throws()
        {
            Assert.Throws<ArgumentException>(() => new CryptoService(new byte[16], new byte[32]));
        }
    }
}
