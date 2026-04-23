using System.Net;
using System.Net.Mail;
using System.Text;

public class EmailService
{
    private readonly string _smtpHost;
    private readonly int _smtpPort;
    private readonly string _fromEmail;
    private readonly string _fromPassword;

    public EmailService(string smtpHost, int smtpPort, string fromEmail, string fromPassword)
    {
        _smtpHost = smtpHost;
        _smtpPort = smtpPort;
        _fromEmail = fromEmail;
        _fromPassword = fromPassword;
    }

    public void SendOrderConfirmation(
        string toEmail,
        string customerName,
        string orderNumber,
        string addressLine,
        List<(string ProductName, int Quantity, decimal Price)> products,
        decimal subtotaal,
        decimal btw,
        decimal verzendkosten,
        decimal totaal)
    {
        var body = new StringBuilder();
        body.AppendLine($"Beste {customerName},");
        body.AppendLine();
        body.AppendLine("Bedankt voor je bestelling! Hieronder vind je een overzicht.");
        body.AppendLine();
        body.AppendLine($"Ordernummer: {orderNumber}");
        body.AppendLine();
        body.AppendLine("───────────────────────────────────");
        body.AppendLine("Bezorgadres:");
        body.AppendLine(addressLine);
        body.AppendLine();
        body.AppendLine("───────────────────────────────────");
        body.AppendLine("Bestelde producten:");
        body.AppendLine();
        body.AppendLine($"{"Product",-30} {"Aantal",6} {"Prijs",12} {"Subtotaal",12}");
        body.AppendLine(new string('-', 62));

        foreach (var (productName, quantity, price) in products)
        {
            decimal rowTotal = quantity * price;
            body.AppendLine($"{productName,-30} {quantity,6} {$"EUR {price:0.00}",12} {$"EUR {rowTotal:0.00}",12}");
        }

        body.AppendLine();
        body.AppendLine($"Subtotaal (excl. BTW): EUR {subtotaal:0.00}");
        body.AppendLine($"BTW (21%):             EUR {btw:0.00}");
        body.AppendLine($"Verzendkosten:         EUR {verzendkosten:0.00}");
        body.AppendLine($"Totaalprijs:           EUR {totaal:0.00}");
        body.AppendLine();
        body.AppendLine("───────────────────────────────────");
        body.AppendLine($"Verwachte levertijd: {DateTime.Now.AddDays(3):dddd d MMMM yyyy}");
        body.AppendLine();
        body.AppendLine("Bekijk je bestelstatus:");
        body.AppendLine($"https://unlucky.com");
        body.AppendLine();
        body.AppendLine("Met vriendelijke groet,");
        body.AppendLine("DataTechnologies Team 5");

        var message = new MailMessage
        {
            From = new MailAddress(_fromEmail, "DataTechnologies"),
            Subject = $"Bevestiging bestelling {orderNumber}",
            Body = body.ToString(),
            IsBodyHtml = false
        };
        message.To.Add(new MailAddress(toEmail));

        using var client = new SmtpClient(_smtpHost, _smtpPort)
        {
            Credentials = new NetworkCredential(_fromEmail, _fromPassword),
            EnableSsl = true
        };

        client.Send(message);
    }
}
