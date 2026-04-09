using System.Security.Authentication;

public class Addresses
{
    public int AddresID { get; set; }
    public int UserID { get; set; }
    public string Street { get; set; }
    public string Number { get; set; }

    public string Postcode { get; set; }
    public string city { get; set; }
    
    public string Country { get; set; }

    public string Type { get; set; }
}