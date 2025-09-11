using Microsoft.EntityFrameworkCore;

namespace eCommerce.Application.Services
{
    public class PaginationService
    {
        public async Task<PagedResult<T>> PaginateAsync<T>(
            IQueryable<T> query, int pageNumber, int pageSize) where T : class
        {
            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize <= 0) pageSize = 10;

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<T>(items, totalCount, pageNumber, pageSize);
        }
    }
}