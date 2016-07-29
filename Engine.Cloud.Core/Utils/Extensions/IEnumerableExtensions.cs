using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utils
{
    public static class IEnumerableExtensions
    {
        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            Throw.IfIsNull(action);

            foreach (T item in source)
                action(item);
        }

        public static void ForEach<T>(this IEnumerable<T> source, Action<T, int> action)
        {
            Throw.IfIsNull(action);

            int index = 0;
            foreach (T item in source)
            {
                action(item, index);
                index++;
            }
        }

        public static void ForEachElse<T>(this IEnumerable<T> source, Action<T> action, Action @else)
        {
            Throw.IfIsNull(action);
            Throw.IfIsNull(@else);

            if (source.Count() > 0)
                source.ForEach(action);
            else
                @else();
        }

        public static void ForEachElse<T>(this IEnumerable<T> source, Action<T, int> action, Action @else)
        {
            Throw.IfIsNull(action);
            Throw.IfIsNull(@else);

            if (source.Count() > 0)
                source.ForEach(action);
            else
                @else();
        }

        public static IEnumerable<T> Randomize<T>(this IEnumerable<T> self)
        {
            Random random = new Random();
            return self.OrderBy(x => (random.Next()));
        }

        public static IEnumerable<T> GetPage<T>(this IEnumerable<T> source, int page, int recordsPerPage, out double totalPages)
        {
            Throw.IfLessThanOrEqZero(recordsPerPage);

            int skip = (page - 1) * recordsPerPage;
            var totalRecords = source.Count();

            var tp = totalRecords / (double)recordsPerPage;
            totalPages = Math.Ceiling(tp);

            return source.Skip(skip).Take(recordsPerPage);
        }

        public static IPagedList<T> ToPagedList<T>(this IEnumerable<T> source, int pageIndex, int pageSize, int? totalCount = null)
        {
            return new PagedList<T>(source, pageIndex, pageSize, totalCount);
        }


        public static string Concatenate<T>(this IEnumerable<T> source, string separator)
        {
            return Concatenate(source, i => i.ToString(), separator);
        }

        public static string Concatenate<T>(this IEnumerable<T> source, Func<T, string> selector, string separator)
        {
            var builder = new StringBuilder();
            foreach (var item in source)
            {
                if (builder.Length > 0)
                    builder.Append(separator);

                builder.Append(selector(item));
            }
            return builder.ToString();
        }

        public static string ToCsv<T>(this IEnumerable<T> source)
        {
            return Concatenate(source, i => i.ToString(), ",");
        }

        public static string ToCsv<T>(this IEnumerable<T> source, Func<T, string> selector)
        {
            return Concatenate(source, selector, ",");
        }

    }

    public interface IPagedList<T> : IList<T>
    {
        int PageCount { get; }
        int TotalItemCount { get; }
        int PageIndex { get; }
        int PageNumber { get; }
        int PageSize { get; }
        bool HasPreviousPage { get; }
        bool HasNextPage { get; }
        bool IsFirstPage { get; }
        bool IsLastPage { get; }
    }

    public class PagedList<T> : List<T>, IPagedList<T>
    {
        public int PageCount { get; private set; }
        public int TotalItemCount { get; private set; }
        public int PageIndex { get; private set; }
        public int PageNumber { get { return this.PageIndex + 1; } }
        public int PageSize { get; private set; }
        public bool HasPreviousPage { get; private set; }
        public bool HasNextPage { get; private set; }
        public bool IsFirstPage { get; private set; }
        public bool IsLastPage { get; private set; }

        public PagedList(IEnumerable<T> source, int index, int pageSize, int? totalCount = null)
            : this(source.AsQueryable(), index, pageSize, totalCount)
        {
        }

        public PagedList(IQueryable<T> source, int index, int pageSize, int? totalCount = null)
        {
            Throw.IfLessThanZero(index);
            Throw.IfLessThanOrEqZero(pageSize);

            if (source == null)
                source = new List<T>().AsQueryable();

            var realTotalCount = source.Count();

            this.PageSize = pageSize;
            this.PageIndex = index;
            this.TotalItemCount = totalCount.HasValue ? totalCount.Value : realTotalCount;
            this.PageCount = this.TotalItemCount > 0 ? (int)Math.Ceiling(this.TotalItemCount / (double)this.PageSize) : 0;

            this.HasPreviousPage = (this.PageIndex > 0);
            this.HasNextPage = (this.PageIndex < (this.PageCount - 1));
            this.IsFirstPage = (this.PageIndex <= 0);
            this.IsLastPage = (this.PageIndex >= (this.PageCount - 1));

            if (this.TotalItemCount <= 0)
                return;

            var realTotalPages = (int)Math.Ceiling(realTotalCount / (double)this.PageSize);

            if (realTotalCount < this.TotalItemCount && realTotalPages <= this.PageIndex)
                this.AddRange(source.Skip((realTotalPages - 1) * this.PageSize).Take(this.PageSize));
            else
                this.AddRange(source.Skip(this.PageIndex * this.PageSize).Take(this.PageSize));
        }
    }
}