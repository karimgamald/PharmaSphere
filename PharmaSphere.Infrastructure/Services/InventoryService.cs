using PharmaSphere.Application.Interfaces.Repositories;
using PharmaSphere.Application.Interfaces.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PharmaSphere.Infrastructure.Services
{
    public class InventoryService : IInventoryService
    {
        private readonly IUnitOfWork _unitOfWork;

        public InventoryService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<int> GetAvailableStockAsync(int medicineId)
        {
            var batches = await _unitOfWork.MedicineBatches.FindAsync(
                b => b.MedicineId == medicineId && b.ExpiryDate > DateTime.UtcNow && b.RemainingUnits > 0
            );

            return batches.Sum(b => b.RemainingUnits);
        }

        public async Task<bool> DeductStockUsingFEFOAsync(int medicineId, int requestedQuantity)
        {
            var batches = await _unitOfWork.MedicineBatches.FindAsync(
                b => b.MedicineId == medicineId && b.ExpiryDate > DateTime.UtcNow && b.RemainingUnits > 0
            );

            var sortedBatches = batches.OrderBy(b => b.ExpiryDate).ToList();

            int totalAvailable = sortedBatches.Sum(b => b.RemainingUnits);
            if (totalAvailable < requestedQuantity)
            {
                return false;
            }

            int remainingToDeduct = requestedQuantity;

            foreach (var batch in sortedBatches)
            {
                if (remainingToDeduct <= 0) break;

                if (batch.RemainingUnits >= remainingToDeduct)
                {
                    batch.RemainingUnits -= remainingToDeduct;
                    remainingToDeduct = 0;
                }
                else
                {
                    remainingToDeduct -= batch.RemainingUnits;
                    batch.RemainingUnits = 0;
                }

                _unitOfWork.MedicineBatches.Update(batch);
            }

            await _unitOfWork.CompleteAsync();
            return true;
        }

        public async Task RestoreStockAsync(int medicineBatchId, int quantityToRestore)
        {
            var batch = await _unitOfWork.MedicineBatches.GetByIdAsync(medicineBatchId);
            if (batch != null)
            {
                batch.RemainingUnits += quantityToRestore;
                _unitOfWork.MedicineBatches.Update(batch);
                await _unitOfWork.CompleteAsync();
            }
        }
    }
}