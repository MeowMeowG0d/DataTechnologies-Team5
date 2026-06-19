namespace DataTechnologies.Services;

public interface IFieldCipher
{
    string Encrypt(string plaintext);
    string Decrypt(string ciphertext);
    string BlindIndex(string value);
}
