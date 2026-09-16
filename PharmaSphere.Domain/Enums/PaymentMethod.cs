using System;
using System.Collections.Generic;
using System.Text;

namespace PharmaSphere.Domain.Enums
{
    public enum PaymentMethod
    {
        Cash = 1,
        CreditCard_POS = 2,   // دفع بكارت مباشر في الصيدلية
        Online_Gateway = 3,   // دفع عبر بوابة الدفع أونلاين (Paymob/Stripe)
        VodafoneCash = 4
    }
}
