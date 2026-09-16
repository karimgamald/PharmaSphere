using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Text;

namespace PharmaSphere.Application.Hubs
{
    public class StockHub : Hub
    {
        // إرسال تحديث للمخزون فور حدوث عملية بيع في الصيدلية (POS) أو أونلاين
        public async Task NotifyStockUpdated(int medicineId, int newStockQuantity)
        {
            await Clients.All.SendAsync("UpdateMedicineStock", medicineId, newStockQuantity);
        }
    }
}
