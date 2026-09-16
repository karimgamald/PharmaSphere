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
    public class PosService : IPosService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHubContext<StockHub> _stockHub;

        public PosService(IUnitOfWork unitOfWork, IHubContext<StockHub> stockHub)
        {
            _unitOfWork = unitOfWork;
            _stockHub = stockHub;
        }

        public async Task<IReadOnlyList<PosMedicineSearchDto>> SearchMedicinesForPosAsync(string query)
        {
            var medicines = await _unitOfWork.Medicines.FindAsync(m =>
                m.Name.Contains(query) ||
                (m.Barcode != null && m.Barcode == query) ||
                (m.ScientificName != null && m.ScientificName.Contains(query)));

            return medicines.Select(m => new PosMedicineSearchDto
            {
                Id = m.Id,
                Name = m.Name,
                ScientificName = m.ScientificName,
                Barcode = m.Barcode,
                StockQuantity = m.TotalStockUnits,
                RequiresPrescription = m.RequiresPrescription
            }).ToList();
        }

        public async Task<int> ProcessPosSaleAsync(PosSaleRequestDto saleRequest, string pharmacistId)
        {
            var order = new Order
            {
                OrderType = OrderType.POS_InStore,
                OrderStatus = OrderStatus.Completed,
                PaymentStatus = PaymentStatus.Paid,
                PaymentMethod = saleRequest.PaymentMethod,
                PharmacistId = pharmacistId,
                OrderDate = DateTime.UtcNow
            };

            decimal totalAmount = 0;

            foreach (var item in saleRequest.Items)
            {
                var medicine = await _unitOfWork.Medicines.GetByIdAsync(item.MedicineId);
                if (medicine == null)
                {
                    throw new InvalidOperationException("الدواء غير موجود بالمخزن.");
                }

                var batches = await _unitOfWork.MedicineBatches.FindAsync(b => b.MedicineId == item.MedicineId);

                var activeBatches = batches
                    .Where(b => b.RemainingUnits > 0 && b.ExpiryDate > DateTime.UtcNow)
                    .OrderBy(b => b.ExpiryDate)
                    .ToList();

                int totalAvailableUnits = activeBatches.Sum(b => b.RemainingUnits);

                if (totalAvailableUnits < item.Quantity)
                {
                    throw new InvalidOperationException($"المخزون غير كافٍ للدواء: {medicine.Name}");
                }

                int unitsToDeduct = item.Quantity;

                foreach (var batch in activeBatches)
                {
                    if (unitsToDeduct <= 0) break;

                    int deductFromBatch = Math.Min(batch.RemainingUnits, unitsToDeduct);
                    batch.RemainingUnits -= deductFromBatch;
                    unitsToDeduct -= deductFromBatch;

                    _unitOfWork.MedicineBatches.Update(batch);

                    var orderItem = new OrderItem
                    {
                        MedicineId = medicine.Id,
                        MedicineBatchId = batch.Id,
                        Quantity = deductFromBatch,
                        UnitCostPrice = batch.PurchasePrice
                    };

                    totalAmount += orderItem.TotalPrice;
                    order.OrderItems.Add(orderItem);
                }

                await _stockHub.Clients.All.SendAsync("UpdateMedicineStock", medicine.Id, activeBatches.Sum(b => b.RemainingUnits));
            }

            order.TotalAmount = totalAmount;
            await _unitOfWork.Orders.AddAsync(order);
            await _unitOfWork.CompleteAsync();

            return order.Id;
        }
    }
}