using System;
using System.Collections.Generic;
using System.Text;

namespace PharmaSphere.Domain.Enums
{
    public enum OrderType
    {
        POS_InStore = 1,     // مبيعات مباشرة داخل الصيدلية
        Online_Delivery = 2  // طلبات أونلاين وتوصيل للمنزل
    }
}
