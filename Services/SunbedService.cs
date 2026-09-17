using AquaPass.Models;
using AquaPass.ModelsDto;
using Microsoft.EntityFrameworkCore;

namespace AquaPass.Services
{
    public class SunbedService
    {
        private readonly AppDbContext _context;
        private readonly SunbedHoldService _holdService;

        public SunbedService(AppDbContext context, SunbedHoldService holdService)
        {
            _context = context;
            _holdService = holdService;
        }

        #region Read Operations

        public async Task<List<SunbedResponseDto>> GetAllAsync()
        {
            return await _context.Sunbeds
                .AsNoTracking()
                .OrderBy(s => s.Row)
                .ThenBy(s => s.Number)
                .Select(s => new SunbedResponseDto
                {
                    Id = s.Id,
                    Number = s.Number,
                    Row = s.Row,
                    ZoneId = s.ZoneId,
                    Description = s.Description,
                    IsAvailable = true
                })
                .ToListAsync();
        }

        public async Task<SunbedResponseDto> GetByIdAsync(Guid id)
        {
            var dto = await _context.Sunbeds
                .AsNoTracking()
                .Where(s => s.Id == id)
                .Select(s => new SunbedResponseDto
                {
                    Id = s.Id,
                    Number = s.Number,
                    Row = s.Row,
                    ZoneId = s.ZoneId,
                    Description = s.Description,
                    IsAvailable = true
                })
                .FirstOrDefaultAsync();

            if (dto == null) throw new KeyNotFoundException($"Sunbed with ID {id} not found.");

            return dto;
        }

        public async Task<List<SunbedResponseDto>> GetByRowAsync(string row)
        {
            return await _context.Sunbeds
                .AsNoTracking()
                .Where(s => s.Row == row)
                .OrderBy(s => s.Number)
                .Select(s => new SunbedResponseDto
                {
                    Id = s.Id,
                    Number = s.Number,
                    Row = s.Row,
                    ZoneId = s.ZoneId,
                    Description = s.Description,
                    IsAvailable = true
                })
                .ToListAsync();
        }

        public async Task<List<SunbedResponseDto>> GetAvailableSeubedsAsync(DateTime visitDate, string? holdToken = null)
        {
            var utcDate = DateTime.SpecifyKind(visitDate.Date, DateTimeKind.Utc);
            var nextDayUtc = utcDate.AddDays(1);

            // 1. Зайняті квитками в базі PostgreSQL
            var bookedSunbedIds = await _context.Tickets
                .Where(t => t.Order.VisitDate >= utcDate
                         && t.Order.VisitDate < nextDayUtc
                         && t.Order.Status != "Cancelled"
                         && t.SunbedId != null)
                .Select(t => t.SunbedId!.Value)
                .ToListAsync();

            var unavailableSet = bookedSunbedIds.ToHashSet();

            // 2. Тимчасово заблоковані в Redis іншими користувачами
            var heldIds = await _holdService.GetHeldSunbedIdsAsync(visitDate, holdToken);
            foreach (var id in heldIds)
            {
                unavailableSet.Add(id);
            }

            // 3. Формуємо відповідь
            return await _context.Sunbeds
                .AsNoTracking()
                .OrderBy(s => s.Row)
                .ThenBy(s => s.Number)
                .Select(s => new SunbedResponseDto
                {
                    Id = s.Id,
                    Number = s.Number,
                    Row = s.Row,
                    ZoneId = s.ZoneId,
                    Description = s.Description,
                    IsAvailable = !unavailableSet.Contains(s.Id)
                })
                .ToListAsync();
        }

        public async Task<List<SunbedResponseDto>> GetByRowAndNumberAsync(string row, int number)
        {
            return await _context.Sunbeds
                .AsNoTracking()
                .Where(s => s.Row == row && s.Number == number)
                .Select(s => new SunbedResponseDto
                {
                    Id = s.Id,
                    Number = s.Number,
                    Row = s.Row,
                    ZoneId = s.ZoneId,
                    Description = s.Description,
                    IsAvailable = true
                })
                .ToListAsync();
        }

        #endregion

        #region Create Operations

        public async Task<SunbedResponseDto> CreateAsync(SunbedCreateDto sunbedDto)
        {
            if (sunbedDto.ZoneId == Guid.Empty)
            {
                sunbedDto.ZoneId = Guid.Parse("22222222-2222-2222-2222-222222222222");
            }

            var sunbed = new Sunbed
            {
                Id = Guid.NewGuid(),
                Number = sunbedDto.Number,
                Row = sunbedDto.Row,
                ZoneId = sunbedDto.ZoneId,
                Description = string.Empty
            };

            await _context.Sunbeds.AddAsync(sunbed);
            await _context.SaveChangesAsync();

            return new SunbedResponseDto
            {
                Id = sunbed.Id,
                Number = sunbed.Number,
                Row = sunbed.Row,
                ZoneId = sunbed.ZoneId,
                Description = sunbed.Description,
                IsAvailable = true
            };
        }

        public async Task CreateRangeAsync(string row, int count)
        {
            var defaultZoneId = Guid.Parse("22222222-2222-2222-2222-222222222222");
            var sunbeds = new List<Sunbed>();

            for (int i = 1; i <= count; i++)
            {
                sunbeds.Add(new Sunbed
                {
                    Id = Guid.NewGuid(),
                    Row = row,
                    Number = i,
                    ZoneId = defaultZoneId,
                    Description = string.Empty
                });
            }

            await _context.Sunbeds.AddRangeAsync(sunbeds);
            await _context.SaveChangesAsync();
        }

        #endregion

        #region Update Operations

        public async Task UpdateAsync(Guid id, SunbedUpdateDto dto)
        {
            var existing = await _context.Sunbeds.FindAsync(id);

            if (existing == null) throw new KeyNotFoundException($"Sunbed with ID {id} not found.");

            existing.Number = dto.Number;
            existing.Row = dto.Row;

            await _context.SaveChangesAsync();
        }

        #endregion

        #region Delete Operations

        public async Task DeleteAsync(Guid id)
        {
            var sunbed = await _context.Sunbeds.FindAsync(id);

            if (sunbed == null) throw new KeyNotFoundException($"Sunbed with ID {id} not found.");

            _context.Sunbeds.Remove(sunbed);
            await _context.SaveChangesAsync();
        }

        #endregion
    }
}