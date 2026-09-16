using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Text;

namespace PharmaSphere.Application.Hubs
{
    public class OrderHub : Hub
    {
        // إشعار الطيار بوجود طلب جديد موجه له
        public async Task NotifyDeliveryBoy(string deliveryBoyId, int orderId)
        {
            await Clients.User(deliveryBoyId).SendAsync("ReceiveNewDeliveryOrder", orderId);
        }

        // إشعار العميل بتحديث حالة الطلب
        public async Task NotifyClientOrderStatus(string clientId, int orderId, string status)
        {
            await Clients.User(clientId).SendAsync("OrderStatusChanged", orderId, status);
        }
    }
}
