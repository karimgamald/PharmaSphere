using PharmaSphere.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PharmaSphere.Application.Interfaces.Services
{
    public interface ISmartDispensingService
    {
        Task<IEnumerable<Medicine>> GetAlternativesAsync(int medicineId);
        Task<IEnumerable<DrugInteraction>> CheckInteractionsAsync(List<int> medicineIds);
    }
}