using PharmaSphere.Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PharmaSphere.Application.Interfaces.Services
{
    public interface IPosService
    {
        Task<IReadOnlyList<PosMedicineSearchDto>> SearchMedicinesForPosAsync(string query);
        Task<int> ProcessPosSaleAsync(PosSaleRequestDto saleRequest, string pharmacistId);
    }
}