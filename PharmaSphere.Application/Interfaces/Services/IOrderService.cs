using PharmaSphere.Application.DTOs;
using PharmaSphere.Domain.Enums;
using System.Threading.Tasks;

namespace PharmaSphere.Application.Interfaces.Services
{
    public interface IOrderService
    {
        Task<int> CreateOnlineOrderAsync(CreateOrderDto dto, string clientId);
        Task<OrderDetailsDto?> GetOrderByIdAsync(int orderId);
        Task UpdateOrderStatusAsync(int orderId, OrderStatus newStatus);
        Task AssignDeliveryBoyAsync(int orderId, string deliveryBoyId);
    }
}