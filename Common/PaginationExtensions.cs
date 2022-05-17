using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public static class PaginationExtensions
    {
        public static IQueryable<T> Paginate<T>(this IQueryable<T> source, int pageNumber, int pageSize)
        {
            if (pageSize <= 0) throw new ArgumentOutOfRangeException("pageSize");
            if (pageNumber <= 0) throw new ArgumentOutOfRangeException("pageNumber");

            return source
                .Skip(pageSize * (pageNumber - 1))
                .Take(pageSize);
        }

        public static PagedResult<T> GetPagedResult<T>(this IEnumerable<T> source, int totalCount)
        {
            return new PagedResult<T>(source, totalCount);
        }
    }
    public class PagedResult<T>
    {
        public IEnumerable<T> Objects { get; set; }
        public int TotalCount { get; set; }
        public PagedResult(IEnumerable<T> objects, int totalCount)
        {
            Objects = objects;
            TotalCount = totalCount;
        }
    }
}
