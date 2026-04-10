using System;
using DataTechnologies.Models;

namespace DataTechnologies.Services
{
    public class EmailService
    {
        public void SendOrderConfirmation(string email, Orders order, string address, string productOverview, float totalPrice)
        {
            Console.WriteLine($"--- E-mail naar: {email} ---");
            Console.WriteLine($"Uw bestelling is ontvangen!\nOrdernummer: {order.OrderNumber}\nProducten: {productOverview}\nBezorgadres: {address}\nTotaalprijs: € {totalPrice:0.00}");
        }
    }
}
