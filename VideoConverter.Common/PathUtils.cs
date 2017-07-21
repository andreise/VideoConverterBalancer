using Common.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;

namespace VideoConverter.Common
{
    public static class PathUtils
    {
        static PathUtils()
        {
        }

        public static readonly char IndependentDirectorySeparatorChar = '/';

        public static IReadOnlyList<char> NativeDirectorySeparatorChars { get; } = new ReadOnlyCollection<char>(
            new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar });

        public static IReadOnlyList<char> NondefaultDirectorySeparatorChars { get; } = new ReadOnlyCollection<char>(
            new[] { IndependentDirectorySeparatorChar, Path.AltDirectorySeparatorChar });

        private static string TransformPath(string path, Action<Lazy<StringBuilder>> transformAction)
        {
            if (string.IsNullOrEmpty(path))
                return path;

            var resultBuilder = new Lazy<StringBuilder>(
                () => new StringBuilder(path),
                isThreadSafe: false);

            transformAction(resultBuilder);

            if (!resultBuilder.IsValueCreated)
                return path;

            return resultBuilder.Value.ToString();
        }

        private static void ReplaceChar(Lazy<StringBuilder> pathBuilder, char oldChar, char newChar)
        {
            if (oldChar != newChar)
                pathBuilder.Value.Replace(oldChar, newChar);
        }

        public static string GetSystemIndependentPath(string path) => TransformPath(
            path,
            resultBuilder =>
            {
                void ReplaceDependentDirectorySeparatorChar(char separator) =>
                    ReplaceChar(resultBuilder, separator, IndependentDirectorySeparatorChar);

                NativeDirectorySeparatorChars.ForEach(
                    separator => ReplaceDependentDirectorySeparatorChar(separator));
            });

        public static string GetSystemDependentPath(string path) => TransformPath(
            path,
            resultBuilder =>
            {
                void ReplaceIndependentDirectorySeparatorChar(char separator) =>
                    ReplaceChar(resultBuilder, separator, Path.DirectorySeparatorChar);

                NondefaultDirectorySeparatorChars.ForEach(
                    separator => ReplaceIndependentDirectorySeparatorChar(separator));
            });
    }
}
