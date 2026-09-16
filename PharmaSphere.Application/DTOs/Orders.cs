using PharmaSphere.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace PharmaSphere.Application.DTOs
{
    public class CreateOrderDto
    {
        public List<OrderItemDto> Items { get; set; } = new List<OrderItemDto>();
        public PaymentMethod PaymentMethod { get; set; }
        public string ShippingAddress { get; set; } = string.Empty;
        public double? DestinationLatitude { get; set; }
        public double? DestinationLongitude { get; set; }
        public int? PrescriptionId { get; set; }
    }

    public class OrderItemDto
    {
        public int MedicineId { get; set; }
        public int Quantity { get; set; }
    }
    public class OrderDetailsDto
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public OrderType OrderType { get; set; }
        public OrderStatus OrderStatus { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal DeliveryFee { get; set; }
        public DateTime OrderDate { get; set; }
        public string? ShippingAddress { get; set; }
        public string? ClientName { get; set; }
        public string? DeliveryBoyName { get; set; }
        public List<OrderItemDetailsDto> Items { get; set; } = new List<OrderItemDetailsDto>();
    }

    public class OrderItemDetailsDto
    {
        public string MedicineName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
