using Common.Diagnostics.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Common.Linq
{
    public static class EnumerableExtensions
    {
        public static void ForEach<TSource>(this IEnumerable<TSource> source, Action<TSource> action)
        {
            Contract.RequiresArgumentNotNull(source, nameof(source));
            Contract.RequiresArgumentNotNull(action, nameof(action));

            foreach (var item in source)
                action(item);
        }

        public static IEnumerable<TSource> Yield<TSource>(this TSource source)
        {
            yield return source;
        }

        public static IEnumerable<TSource> Concat<TSource>(this TSource first, IEnumerable<TSource> second) =>
            first.Yield().Concat(second);

        public static IEnumerable<TSource> Concat<TSource>(this IEnumerable<TSource> first, TSource second) =>
            first.Concat(second.Yield());

        public static bool IsNullOrEmpty<TSource>(this IReadOnlyCollection<TSource> collection) =>
            collection is null || collection.Count == 0;

        public static bool IsNullOrEmpty<TSource>(this IEnumerable<TSource> collection) =>
            collection is null || !collection.Any();

        public static bool IsIn<TSource>(this TSource source, IEnumerable<TSource> collection) =>
            collection.Contains(source);

        public static bool IsIn<TSource>(this TSource source, params TSource[] collection) =>
            IsIn(source, (IEnumerable<TSource>)collection);

        public static bool IsIn<TSource>(this TSource source, IEqualityComparer<TSource> comparer, IEnumerable<TSource> collection) =>
            collection.Contains(source, comparer);

        public static bool IsIn<TSource>(this TSource source, IEqualityComparer<TSource> comparer, params TSource[] collection) =>
            IsIn(source, comparer, (IEnumerable<TSource>)collection);
    }
}
