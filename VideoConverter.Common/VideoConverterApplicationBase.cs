using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VideoConverter.Common
{
    public abstract class VideoConverterApplicationBase
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

        private static Encoding SerializationEncoding => Encoding.UTF8;

        protected static byte[] SerializeFile(ConverterFileInfo file) =>
            file is null ? null :
            SerializationEncoding.GetBytes(JsonConvert.SerializeObject(file));

        protected static ConverterFileInfo DeserializeFile(byte[] body) =>
            body is null ? null :
            (ConverterFileInfo)JsonConvert.DeserializeObject(SerializationEncoding.GetString(body), typeof(ConverterFileInfo));

        protected readonly string hostName;

        protected virtual IEqualityComparer<string> ArgKeyEqualityComparer => StringComparer.OrdinalIgnoreCase;

        protected readonly IReadOnlyDictionary<string, string> argDictionary;

        private IReadOnlyDictionary<string, string> ParseArgs(string[] args, string[] argKeys)
        {
            string getArgValue(string argKey) =>
                args
                .SkipWhile(arg => !this.ArgKeyEqualityComparer.Equals(arg, argKey))
                .Skip(1)
                .FirstOrDefault();

            return argKeys.ToDictionary(
                argKey => argKey,
                argKey => getArgValue(argKey),
                this.ArgKeyEqualityComparer);
        }

        public VideoConverterApplicationBase(string hostName, string[] args, string[] argKeys)
        {
            this.hostName = hostName;

            T[] normalizeArray<T>(T[] array) => array ?? new T[] { };

            this.argDictionary = this.ParseArgs(
                normalizeArray(args),
                normalizeArray(argKeys));
        }

        public abstract Task RunAsync();
    }
}
