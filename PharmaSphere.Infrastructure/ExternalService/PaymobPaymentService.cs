using PharmaSphere.Application.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace PharmaSphere.Infrastructure.ExternalService
{
    public class PaymobPaymentService : IPaymentService
    {
        private readonly HttpClient _httpClient;

        public PaymobPaymentService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> CreatePaymentSessionAsync(int orderId, decimal amount, string clientEmail)
        {
            await Task.Delay(100);
            return $"https://accept.paymob.com/api/acceptance/iframes/sandbox_id?payment_token=MOCK_TOKEN_{orderId}";
        }

        public async Task<bool> ProcessPaymentWebhookAsync(string payload, string signature)
        {
            await Task.Delay(50);
            return true;
        }
    }
}
