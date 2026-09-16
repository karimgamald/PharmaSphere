using Microsoft.Extensions.Caching.Memory;
using System;

namespace PharmaSphere.Infrastructure.Services
{
    public class InventoryLockService
    {
        private readonly IMemoryCache _cache;

        public InventoryLockService(IMemoryCache cache)
        {
            _cache = cache;
        }

        public bool ReserveStock(int medicineId, int quantity)
        {
            string key = $"lock_med_{medicineId}";
            int currentLocked = _cache.Get<int?>(key) ?? 0;

            _cache.Set(key, currentLocked + quantity, TimeSpan.FromMinutes(30));
            return true;
        }

        public void ReleaseStock(int medicineId, int quantity)
        {
            string key = $"lock_med_{medicineId}";
            int currentLocked = _cache.Get<int?>(key) ?? 0;
            int newLocked = Math.Max(0, currentLocked - quantity);

            _cache.Set(key, newLocked, TimeSpan.FromMinutes(30));
        }

        public int GetLockedStock(int medicineId)
        {
            return _cache.Get<int?>($"lock_med_{medicineId}") ?? 0;
        }
    }
}