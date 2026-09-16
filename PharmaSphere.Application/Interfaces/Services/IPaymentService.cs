using System.Threading.Tasks;

namespace PharmaSphere.Application.Interfaces.Services
{
    public interface IPaymentService
    {
        Task<string> CreatePaymentSessionAsync(int orderId, decimal amount, string clientEmail);
        Task<bool> ProcessPaymentWebhookAsync(string payload, string signature);
    }
}