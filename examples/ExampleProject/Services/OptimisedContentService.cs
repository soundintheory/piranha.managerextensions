/*
 * Copyright (c) .NET Foundation and Contributors
 *
 * This software may be modified and distributed under the terms
 * of the MIT license. See the LICENSE file for details.
 *
 * https://github.com/piranhacms/piranha.core
 *
 */

using Microsoft.EntityFrameworkCore;
using Piranha;
using Piranha.Cache;
using Piranha.Models;

namespace ExampleProject.Services
{
    public class OptimisedContentService
    {
        private readonly IApi _api;
        private readonly IDb _db;
        private readonly ICache _cache;

        public OptimisedContentService(IApi api, IDb db, ICache cache)
        {
            _api = api;
            _db = db;
            _cache = cache;
        }

        public async Task<IEnumerable<T>> GetAllAsync<T>(Guid? languageId = null) where T : GenericContent
        {
            var models = new List<T>();
            var all = await GetQuery<T>()
                .OrderBy(c => c.Title)
                .ThenBy(c => c.LastModified)
                .Select(c => c.Id)
                .ToListAsync()
                .ConfigureAwait(false);

            foreach (var contentId in all)
            {
                var content = await _api.Content.GetByIdAsync<T>(contentId, languageId).ConfigureAwait(false);

                if (content != null)
                {
                    models.Add(content);
                }
            }
            return models;
        }

        public async Task<T?> GetFirstOfTypeAsync<T>(Guid? languageId = null) where T : GenericContent
        {
            var models = new List<T>();
            var contentId = await GetQuery<T>()
                .Select(c => c.Id)
                .FirstOrDefaultAsync()
                .ConfigureAwait(false);

            if (contentId == Guid.Empty)
            {
                return null;
            }

            return await _api.Content.GetByIdAsync<T>(contentId, languageId).ConfigureAwait(false);
        }

        private IQueryable<Piranha.Data.Content> GetQuery<T>() where T : GenericContent
        {
            var query = _db.Content
                .AsNoTracking();

            var clrType = typeof(T).AssemblyQualifiedName;
            var contentType = App.ContentTypes.FirstOrDefault(x => x.CLRType == clrType);

            if (contentType != null)
            {
                query = query
                    .Where(c => c.TypeId == contentType.Id);
            }

            return query;
        }
    }
}
