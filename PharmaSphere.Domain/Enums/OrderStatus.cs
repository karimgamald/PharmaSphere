using System;
using System.Collections.Generic;
using System.Text;

namespace PharmaSphere.Domain.Enums
{
    public enum OrderStatus
    {
        Pending = 1,        // قيد الانتظار (طلبات الأونلاين)
        Approved = 2,       // تم التأكيد والموافقة من الصيدلي
        Preparing = 3,      // جاري التجهيز
        OutForDelivery = 4, // خرج للتوصيل مع الطيار
        Completed = 5,      // مكتمل (تم التسليم أو البيع المباشر)
        Cancelled = 6       // ملغى
    }
}
