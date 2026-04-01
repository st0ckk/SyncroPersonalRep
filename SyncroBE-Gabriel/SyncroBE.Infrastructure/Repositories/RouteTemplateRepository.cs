using Microsoft.EntityFrameworkCore;
using SyncroBE.Application.DTOs.RouteTemplate;
using SyncroBE.Application.Interfaces;
using SyncroBE.Domain.Entities;
using SyncroBE.Infrastructure.Data;

namespace SyncroBE.Infrastructure.Repositories
{
    public class RouteTemplateRepository : IRouteTemplateRepository
    {
        private readonly SyncroDbContext _context;

        public RouteTemplateRepository(SyncroDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<RouteTemplateDto>> GetAsync(bool includeInactive)
        {
            var query = _context.RouteTemplates
                .AsNoTracking()
                .Include(t => t.DefaultDriverUser)
                .Include(t => t.Stops)
                .AsQueryable();

            if (!includeInactive)
                query = query.Where(t => t.IsActive);

            var templates = await query
                .OrderBy(t => t.TemplateName)
                .ToListAsync();

            return templates.Select(Map);
        }

        public async Task<RouteTemplateDto?> GetDtoByIdAsync(int id)
        {
            var entity = await _context.RouteTemplates
                .AsNoTracking()
                .Include(t => t.DefaultDriverUser)
                .Include(t => t.Stops)
                .FirstOrDefaultAsync(t => t.TemplateId == id);

            return entity == null ? null : Map(entity);
        }

        public async Task<RouteTemplate?> GetByIdAsync(int id)
        {
            return await _context.RouteTemplates
                .Include(t => t.Stops)
                .FirstOrDefaultAsync(t => t.TemplateId == id);
        }

        public async Task AddAsync(RouteTemplate template)
        {
            _context.RouteTemplates.Add(template);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(RouteTemplate template)
        {
            await _context.SaveChangesAsync();
        }

        public async Task DeactivateAsync(int id)
        {
            var entity = await _context.RouteTemplates.FindAsync(id);
            if (entity == null) return;

            entity.IsActive = false;
            entity.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        public async Task ActivateAsync(int id)
        {
            var entity = await _context.RouteTemplates.FindAsync(id);
            if (entity == null) return;

            entity.IsActive = true;
            entity.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        private static RouteTemplateDto Map(RouteTemplate template)
        {
            return new RouteTemplateDto
            {
                TemplateId = template.TemplateId,
                TemplateName = template.TemplateName,
                Description = template.Description,
                DefaultDriverUserId = template.DefaultDriverUserId,
                DefaultDriverName = template.DefaultDriverUser == null
                    ? null
                    : $"{template.DefaultDriverUser.UserName} {template.DefaultDriverUser.UserLastname}".Trim(),
                IsActive = template.IsActive,
                StopCount = template.Stops?.Count ?? 0,
                Stops = template.Stops?
                    .OrderBy(s => s.StopOrder)
                    .Select(s => new RouteTemplateStopDto
                    {
                        TemplateStopId = s.TemplateStopId,
                        ClientId = s.ClientId,
                        ClientNameSnapshot = s.ClientNameSnapshot,
                        AddressSnapshot = s.AddressSnapshot,
                        StopOrder = s.StopOrder,
                        Latitude = s.Latitude,
                        Longitude = s.Longitude,
                        Notes = s.Notes
                    })
                    .ToList() ?? new List<RouteTemplateStopDto>()
            };
        }
    }
}