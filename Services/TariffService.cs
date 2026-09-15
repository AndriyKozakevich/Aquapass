using AquaPass.Models;
using AquaPass.ModelsDto;
using AquaPass.Enums;
using Microsoft.EntityFrameworkCore;

namespace AquaPass.Services
{
    public class TariffService
    {
        private readonly AppDbContext _context;

        public TariffService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<TariffResponseDto>> GetAllAsync()
        {
            return await _context.Tariffs
                .AsNoTracking()
                .Select(t => new TariffResponseDto(
                    t.Id,
                    t.Name,
                    t.ServiceType,
                    t.ServiceType.ToString(),
                    t.Price,
                    t.ZoneId,
                    t.DayType.ToString()
                ))
                .ToListAsync();
        }

        public async Task<TariffResponseDto?> GetByIdAsync(Guid id)
        {
            return await _context.Tariffs
                .AsNoTracking()
                .Where(t => t.Id == id)
                .Select(t => new TariffResponseDto(
                    t.Id,
                    t.Name,
                    t.ServiceType,
                    t.ServiceType.ToString(),
                    t.Price,
                    t.ZoneId,
                    t.DayType.ToString()
                ))
                .FirstOrDefaultAsync();
        }

        public async Task<List<TariffResponseDto>> GetByZoneAsync(Guid zoneId)
        {
            return await _context.Tariffs
                .AsNoTracking()
                .Where(t => t.ZoneId == zoneId)
                .Select(t => new TariffResponseDto(
                    t.Id,
                    t.Name,
                    t.ServiceType,
                    t.ServiceType.ToString(),
                    t.Price,
                    t.ZoneId,
                    t.DayType.ToString()
                ))
                .ToListAsync();
        }

        public async Task<List<TariffResponseDto>> GetByServiceTypeAsync(ServiceType serviceType)
        {
            return await _context.Tariffs
                .AsNoTracking()
                .Where(t => t.ServiceType == serviceType)
                .Select(t => new TariffResponseDto(
                    t.Id,
                    t.Name,
                    t.ServiceType,
                    t.ServiceType.ToString(),
                    t.Price,
                    t.ZoneId,
                    t.DayType.ToString()
                ))
                .ToListAsync();
        }

        public async Task<List<TariffResponseDto>> GetByDateAsync(DateTime date)
        {
            DayType dayType = (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                ? DayType.Weekend
                : DayType.Weekday;

            return await _context.Tariffs
                .AsNoTracking()
                .Where(t => t.DayType == dayType)
                .Select(t => new TariffResponseDto(
                    t.Id,
                    t.Name,
                    t.ServiceType,
                    t.ServiceType.ToString(),
                    t.Price,
                    t.ZoneId,
                    t.DayType.ToString()
                ))
                .ToListAsync();
        }

        public async Task<Tariff> CreateAsync(Tariff tariff)
        {
            _context.Tariffs.Add(tariff);
            await _context.SaveChangesAsync();

            return tariff;
        }

        public async Task UpdateAsync(Guid id, TariffUpdateDto dto)
        {
            var existing = await _context.Tariffs.FindAsync(id);

            if (existing == null)
            {
                throw new KeyNotFoundException($"Tariff with ID {id} not found.");
            }

            existing.Name = dto.Name;
            existing.Price = dto.Price;
            existing.ServiceType = dto.ServiceType;
            existing.DayType = dto.DayType;
            existing.ZoneId = dto.ZoneId;

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var existing = await _context.Tariffs.FindAsync(id);

            if (existing == null)
            {
                throw new KeyNotFoundException($"Tariff with ID {id} not found.");
            }

            _context.Tariffs.Remove(existing);
            await _context.SaveChangesAsync();
        }
    }
}
