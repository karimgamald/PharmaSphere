using PharmaSphere.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace PharmaSphere.Application.DTOs
{
    public class PosSaleRequestDto
    {
        public List<PosItemDto> Items { get; set; } = new List<PosItemDto>();
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;
    }

    public class PosItemDto
    {
        public int MedicineId { get; set; }
        public int Quantity { get; set; }
    }
}
