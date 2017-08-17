using System.Linq;

namespace Common.Linq
{
    public static class QueryableHelpers
    {
        public static IQueryable<TResult> Empty<TResult>() => Enumerable.Empty<TResult>().AsQueryable();
    }
}
