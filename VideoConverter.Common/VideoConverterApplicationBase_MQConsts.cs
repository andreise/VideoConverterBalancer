namespace VideoConverter.Common
{
    partial class VideoConverterApplicationBase
    {
        protected static class MQConsts
        {
            public static class ExchangeTypes
            {
                public const string Fanout = "fanout";
            }

            public static class Exchanges
            {
                public const string VideoConverterDefault = "video_converter";
            }

            public static class Queues
            {
                public const string IsCompleted = "is_completed";
                public const string FreeFiles = "free_files";
                public const string FilesProcessingStarted = "files_processing_started";
                public const string FilesProcessedSuccessfully = "files_processed_successfully";
                public const string FilesFailed = "file_failed";
            }
        }
    }
}
