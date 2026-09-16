using Microsoft.AspNetCore.SignalR;
using PharmaSphere.Application.DTOs;
using PharmaSphere.Application.Hubs;
using PharmaSphere.Application.Interfaces.Repositories;
using PharmaSphere.Application.Interfaces.Services;
using PharmaSphere.Domain.Entities;
using PharmaSphere.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PharmaSphere.Infrastructure.Services
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHubContext<OrderHub> _orderHub;
        private readonly IHubContext<StockHub> _stockHub;

        public OrderService(
            IUnitOfWork unitOfWork,
            IHubContext<OrderHub> orderHub,
            IHubContext<StockHub> stockHub)
        {
            _unitOfWork = unitOfWork;
            _orderHub = orderHub;
            _stockHub = stockHub;
        }

        public async Task<int> CreateOnlineOrderAsync(CreateOrderDto dto, string clientId)
        {
            var order = new Order
            {
                OrderType = OrderType.Online_Delivery,
                OrderStatus = OrderStatus.Pending,
                PaymentStatus = dto.PaymentMethod == PaymentMethod.Cash ? PaymentStatus.Unpaid : PaymentStatus.Paid,
                PaymentMethod = dto.PaymentMethod,
                ShippingAddress = dto.ShippingAddress,
                DestinationLatitude = dto.DestinationLatitude,
                DestinationLongitude = dto.DestinationLongitude,
                ClientId = clientId,
                DeliveryFee = 20.00m,
                OrderDate = DateTime.UtcNow
            };

            decimal itemsTotal = 0;

            foreach (var item in dto.Items)
            {
                var medicine = await _unitOfWork.Medicines.GetByIdAsync(item.MedicineId);
                if (medicine == null)
                {
                    throw new InvalidOperationException("الدواء غير موجود.");
                }

                // جلب التشغيلات الصالحة المتاحة للمنتج وتخصيصها بأسلوب FEFO
                var batches = await _unitOfWork.MedicineBatches.FindAsync(b =>
                    b.MedicineId == item.MedicineId &&
                    b.RemainingUnits > 0 &&
                    b.ExpiryDate > DateTime.UtcNow);

                var activeBatches = batches.OrderBy(b => b.ExpiryDate).ToList();
                int totalAvailableUnits = activeBatches.Sum(b => b.RemainingUnits);

                if (totalAvailableUnits < item.Quantity)
                {
                    throw new InvalidOperationException($"الدواء {medicine.Name} غير متوفر بالكمية المطلوبة.");
                }

                int unitsToDeduct = item.Quantity;

                foreach (var batch in activeBatches)
                {
                    if (unitsToDeduct <= 0) break;

                    int deductFromBatch = Math.Min(batch.RemainingUnits, unitsToDeduct);
                    batch.RemainingUnits -= deductFromBatch;
                    unitsToDeduct -= deductFromBatch;

                    _unitOfWork.MedicineBatches.Update(batch);

                    // إضافة عنصر الطلب وربطه بالـ Batch وسعر التكلفة
                    var orderItem = new OrderItem
                    {
                        MedicineId = medicine.Id,
                        MedicineBatchId = batch.Id,
                        Quantity = deductFromBatch,
                        UnitCostPrice = batch.PurchasePrice
                    };

                    itemsTotal += orderItem.TotalPrice;
                    order.OrderItems.Add(orderItem);
                }

                // بث التحديث اللحظي لسطح مكتب الـ POS والمتجر الإلكتروني
                int remainingTotalStock = activeBatches.Sum(b => b.RemainingUnits);
                await _stockHub.Clients.All.SendAsync("UpdateMedicineStock", medicine.Id, remainingTotalStock);
            }

            order.TotalAmount = itemsTotal + order.DeliveryFee;

            if (dto.PrescriptionId.HasValue)
            {
                var prescription = await _unitOfWork.Prescriptions.GetByIdAsync(dto.PrescriptionId.Value);
                if (prescription != null)
                {
                    prescription.Status = PrescriptionStatus.Processed;
                    order.Prescription = prescription;
                }
            }

            await _unitOfWork.Orders.AddAsync(order);
            await _unitOfWork.CompleteAsync();

            return order.Id;
        }

        public async Task<OrderDetailsDto?> GetOrderByIdAsync(int orderId)
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(orderId);
            if (order == null) return null;

            return new OrderDetailsDto
            {
                Id = order.Id,
                OrderNumber = order.OrderNumber,
                OrderType = order.OrderType,
                OrderStatus = order.OrderStatus,
                PaymentStatus = order.PaymentStatus,
                PaymentMethod = order.PaymentMethod,
                TotalAmount = order.TotalAmount,
                DeliveryFee = order.DeliveryFee,
                OrderDate = order.OrderDate,
                ShippingAddress = order.ShippingAddress
            };
        }

        public async Task UpdateOrderStatusAsync(int orderId, OrderStatus newStatus)
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(orderId);
            if (order != null)
            {
                order.OrderStatus = newStatus;
                _unitOfWork.Orders.Update(order);
                await _unitOfWork.CompleteAsync();

                if (!string.IsNullOrEmpty(order.ClientId))
                {
                    await _orderHub.Clients.User(order.ClientId).SendAsync("OrderStatusChanged", order.Id, newStatus.ToString());
                }
            }
        }

        public async Task AssignDeliveryBoyAsync(int orderId, string deliveryBoyId)
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(orderId);
            if (order != null)
            {
                order.DeliveryBoyId = deliveryBoyId;
                order.OrderStatus = OrderStatus.OutForDelivery;
                _unitOfWork.Orders.Update(order);
                await _unitOfWork.CompleteAsync();

                await _orderHub.Clients.User(deliveryBoyId).SendAsync("ReceiveNewDeliveryOrder", order.Id);
            }
        }
    }
}