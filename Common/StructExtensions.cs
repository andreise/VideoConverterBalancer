namespace Common
{
    public static class StructExtensions
    {
        public static TStruct? AsNullable<TStruct>(this TStruct value) where TStruct : struct => value;
    }
}
