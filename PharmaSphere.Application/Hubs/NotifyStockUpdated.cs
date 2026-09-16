using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Text;

namespace PharmaSphere.Application.Hubs
{
    public class InventoryHub : Hub
    {
        public async Task NotifyStockUpdated(int medicineId, int newStock)
        {
            await Clients.All.SendAsync("ReceiveStockUpdate", medicineId, newStock);
        }
    }
}
