using PharmaSphere.Application.Interfaces.Repositories;
using PharmaSphere.Application.Interfaces.Services;
using PharmaSphere.Domain.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PharmaSphere.Infrastructure.Services
{
    public class SmartDispensingService : ISmartDispensingService
    {
        private readonly IUnitOfWork _unitOfWork;

        public SmartDispensingService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<Medicine>> GetAlternativesAsync(int medicineId)
        {
            var target = await _unitOfWork.Medicines.GetByIdAsync(medicineId);
            if (target == null)
            {
                return Enumerable.Empty<Medicine>();
            }

            string scientificName = target.ScientificName?.Trim().ToLower();
            string activeIngredient = target.ActiveIngredient?.Trim().ToLower();

            var alternatives = await _unitOfWork.Medicines.FindAsync(m =>
                m.Id != medicineId &&
                ((scientificName != null && m.ScientificName != null && m.ScientificName.Trim().ToLower() == scientificName) ||
                 (activeIngredient != null && m.ActiveIngredient != null && m.ActiveIngredient.Trim().ToLower() == activeIngredient))
            );

            return alternatives;
        }

        public async Task<IEnumerable<DrugInteraction>> CheckInteractionsAsync(List<int> medicineIds)
        {
            if (medicineIds == null || medicineIds.Count < 2)
            {
                return Enumerable.Empty<DrugInteraction>();
            }

            var interactions = await _unitOfWork.DrugInteractions.FindAsync(di =>
                medicineIds.Contains(di.MedicineId) && medicineIds.Contains(di.InteractingMedicineId)
            );

            return interactions;
        }
    }
}