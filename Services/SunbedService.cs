using AquaPass.Models;
using AquaPass.ModelsDto;
using Microsoft.EntityFrameworkCore;

namespace AquaPass.Services
{
    public class SunbedService
    {
        private readonly AppDbContext _context;

        public SunbedService(AppDbContext context)
        {
            _context = context;
        }

        #region Read Operations

        public async Task<List<SunbedResponseDto>> GetAllAsync()
        {
            return await _context.Sunbeds
                .AsNoTracking()
                .Select(s => new SunbedResponseDto
                {
                    Id = s.Id,
                    Number = s.Number,
                    Row = s.Row,
                    ZoneId = s.ZoneId,
                    Description = s.Description,
                    IsAvailable = s.IsAvailable
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
                    IsAvailable = s.IsAvailable
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
                .Select(s => new SunbedResponseDto
                {
                    Id = s.Id,
                    Number = s.Number,
                    Row = s.Row,
                    ZoneId = s.ZoneId,
                    Description = s.Description,
                    IsAvailable = s.IsAvailable
                })
                .ToListAsync();
        }

        public async Task<List<SunbedResponseDto>> GetAvailableSeubedsAsync(DateTime visitDate)
        {
            // 1. Примусово конвертуємо дату в UTC та беремо початок і кінець доби
            var utcDate = DateTime.SpecifyKind(visitDate.Date, DateTimeKind.Utc);
            var nextDayUtc = utcDate.AddDays(1);

            // 2. Фільтруємо за діапазоном дат через UTC
            var bookedSunbedIds = await _context.Tickets
                .Where(t => t.Order.VisitDate >= utcDate
                         && t.Order.VisitDate < nextDayUtc
                         && t.Order.Status != "Cancelled"
                         && t.SunbedId != null)
                .Select(t => t.SunbedId!.Value)
                .ToListAsync();

            var bookedSet = bookedSunbedIds.ToHashSet();

            var allSunbeds = await _context.Sunbeds
                .OrderBy(s => s.Number)
                .Select(s => new SunbedResponseDto
                {
                    Id = s.Id,
                    Number = s.Number,
                    ZoneId = s.ZoneId,
                    IsAvailable = !bookedSet.Contains(s.Id),
                    Row = s.Row
                })
                .ToListAsync();

            return allSunbeds;
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
                    IsAvailable = s.IsAvailable
                })
                .ToListAsync();
        }

        #endregion

        #region Create Operations

        public async Task<SunbedResponseDto> CreateAsync(SunbedCreateDto sunbedDto)
        {
            if(sunbedDto.ZoneId == Guid.Empty)
            {
                sunbedDto.ZoneId = Guid.Parse("22222222-2222-2222-2222-222222222222"); // Присвоюємо значення за замовчуванням
            }

            var sunbed = new Sunbed
            {
                Id = Guid.NewGuid(),
                Number = sunbedDto.Number,
                Row = sunbedDto.Row,
                ZoneId = sunbedDto.ZoneId, // Присвоюємо ZoneId
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
                IsAvailable = sunbed.IsAvailable
            };
        }

        // Зручний метод для швидкого створення ряду шезлонгів
        public async Task CreateRangeAsync(string row, int count)
        {
            var ZoneId = Guid.Parse("22222222-2222-2222-2222-222222222222");
            var sunbeds = new List<Sunbed>();

            for (int i = 1; i <= count; i++)
            {
                sunbeds.Add(new Sunbed
                {
                    Id = Guid.NewGuid(),
                    Row = row,
                    Number = i,
                    ZoneId = ZoneId,
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

        public async Task ToggleAvailabilityAsync(Guid id, bool isAvailable)
        {
            var sunbed = await _context.Sunbeds.FindAsync(id);

            if (sunbed == null) throw new KeyNotFoundException($"Sunbed with ID {id} not found.");

            sunbed.IsAvailable = isAvailable;

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