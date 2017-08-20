using System;

namespace Common
{
    public static class StructExtensions
    {
        public static TStruct? AsNullable<TStruct>(this TStruct value) where TStruct : struct => value;

        public static bool IsNullOrDefault<TStruct>(this TStruct? value) where TStruct : struct, IEquatable<TStruct> =>
            !value.HasValue || value.Value.Equals(default(TStruct));

        public static bool IsNullOrDefault<TStruct>(this TStruct? value, TStruct defaultValue) where TStruct : struct, IEquatable<TStruct> =>
            !value.HasValue || value.Value.Equals(defaultValue);
    }
}
