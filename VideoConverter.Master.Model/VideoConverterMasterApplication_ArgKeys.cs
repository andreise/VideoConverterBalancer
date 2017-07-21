namespace VideoConverter.Master.Model
{
    partial class VideoConverterMasterApplication
    {
        private static class ArgKeys
        {
            public static string SourceDirectory = "--source_dir";
            public static string SearchPattern = "--search_pattern";
            public static string DestDirectory = "--dest_dir";

            public static string[] GetAll() => new[]
            {
                SourceDirectory,
                SearchPattern,
                DestDirectory
            };
        }
    }
}
