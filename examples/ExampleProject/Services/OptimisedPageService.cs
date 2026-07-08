using Microsoft.EntityFrameworkCore;
using Piranha;
using Piranha.Cache;
using Piranha.Models;

namespace ExampleProject.Services
{
    public class OptimisedPageService
    {
        private readonly IApi _api;
        private readonly IDb _db;
        private readonly ICache _cache;

        public OptimisedPageService(IApi api, IDb db, ICache cache)
        {
            _api = api;
            _db = db;
            _cache = cache;
        }

        /// <summary>
        /// Gets all published pages of all types, optionally scoped to a site.
        /// </summary>
        public async Task<IEnumerable<PageBase>> GetAllAsync(Guid? siteId = null)
        {
            var ids = await GetBaseQuery(siteId)
                .Select(p => p.Id)
                .ToListAsync()
                .ConfigureAwait(false);

            var models = new List<PageBase>();
            foreach (var id in ids)
            {
                var page = await _api.Pages.GetByIdAsync(id).ConfigureAwait(false);
                if (page != null)
                    models.Add(page);
            }
            return models;
        }

        /// <summary>
        /// Gets all published pages of a specific type, optionally scoped to a site.
        /// </summary>
        public async Task<IEnumerable<T>> GetAllAsync<T>(Guid? siteId = null) where T : PageBase
        {
            var ids = await GetBaseQuery(siteId)
                .Where(p => p.PageTypeId == GetPageTypeId<T>())
                .Select(p => p.Id)
                .ToListAsync()
                .ConfigureAwait(false);

            var models = new List<T>();
            foreach (var id in ids)
            {
                var page = await _api.Pages.GetByIdAsync<T>(id).ConfigureAwait(false);
                if (page != null)
                    models.Add(page);
            }
            return models;
        }

        /// <summary>
        /// Gets a single published page by id.
        /// </summary>
        public Task<T> GetByIdAsync<T>(Guid id) where T : PageBase
        {
            return _api.Pages.GetByIdAsync<T>(id);
        }

        /// <summary>
        /// Gets the first published page of a specific type, optionally scoped to a site.
        /// </summary>
        public async Task<T?> GetFirstOfTypeAsync<T>(Guid? siteId = null) where T : PageBase
        {
            var id = await GetBaseQuery(siteId)
                .Where(p => p.PageTypeId == GetPageTypeId<T>())
                .Select(p => p.Id)
                .FirstOrDefaultAsync()
                .ConfigureAwait(false);

            if (id == Guid.Empty)
                return null;

            return await _api.Pages.GetByIdAsync<T>(id).ConfigureAwait(false);
        }

        private IQueryable<Piranha.Data.Page> GetBaseQuery(Guid? siteId)
        {
            var query = _db.Pages
                .AsNoTracking()
                .Where(p => p.Published != null);

            if (siteId.HasValue)
                query = query.Where(p => p.SiteId == siteId.Value);

            return query;
        }

        private static string? GetPageTypeId<T>() where T : PageBase
        {
            var clrType = typeof(T).AssemblyQualifiedName;
            return App.PageTypes.FirstOrDefault(t => t.CLRType == clrType)?.Id;
        }
    }
}
