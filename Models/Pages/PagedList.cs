using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace dZENcode_backend.Models.Pages
{
    public class PagedList<T> : List<T>
    {
        private int Page { get; set; }
        private int PageSize { get; set; }
        public int TotalPages { get; set; }
        public QueryOptions Options { get; set; }

        public static async Task<PagedList<T>> CreateAsync(IQueryable<T> query, QueryOptions? options = null)
        {
            var page = options.Page;
            var pageSize = 25;

            if (options != null && !string.IsNullOrEmpty(options.OrderBy))
                query = Order(query, options.OrderBy, options.Desc);

            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((decimal)totalItems / pageSize);

            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            return new PagedList<T>(items, totalPages, page, pageSize, options);
        }

        private PagedList(List<T> items, int totalPages, int page, int pageSize, QueryOptions options)
        {
            Page = page;
            PageSize = pageSize;
            TotalPages = totalPages;
            Options = options;
            AddRange(items);
        }

        private static IQueryable<T> Order(IQueryable<T> query, string propertyName, bool desc)
        {
            var parameter = Expression.Parameter(typeof(T), "x");
            var source = propertyName.Split('.').Aggregate((Expression)parameter, Expression.Property);
            var lambda = Expression.Lambda(typeof(Func<,>).MakeGenericType(typeof(T), source.Type), source, parameter);
            return typeof(Queryable).GetMethods().Single(e => e.Name == (desc ? "OrderBy" : "OrderByDescending") &&
                                                              e.IsGenericMethodDefinition &&
                                                              e.GetGenericArguments().Length == 2 &&
                                                              e.GetParameters().Length == 2)
                .MakeGenericMethod(typeof(T), source.Type)
                .Invoke(null, new object[] { query, lambda }) as IQueryable<T>;
        }
    }
}
