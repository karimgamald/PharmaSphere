using System;
using System.Collections.Generic;
using System.Text;

namespace PharmaSphere.Domain.Enums
{
    public enum StockTransactionType
    {
        Purchase = 1,
        Sale = 2,
        Return = 3,
        Expired = 4,
        Damaged = 5,
        Adjustment = 6
    }
}
