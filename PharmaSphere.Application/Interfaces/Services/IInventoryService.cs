using System.Threading.Tasks;

namespace PharmaSphere.Application.Interfaces.Services
{
    public interface IInventoryService
    {
        Task<bool> DeductStockUsingFEFOAsync(int medicineId, int requestedQuantity);
        Task RestoreStockAsync(int medicineBatchId, int quantityToRestore);
        Task<int> GetAvailableStockAsync(int medicineId);
    }
}