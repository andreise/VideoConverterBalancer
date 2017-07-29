using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VideoConverter.Common
{
    public abstract partial class VideoConverterApplicationBase
    {
        private static Encoding SerializationEncoding => Encoding.UTF8;

        protected static byte[] SerializeFile(ConverterFileInfo file) =>
            file is null ? null :
            SerializationEncoding.GetBytes(JsonConvert.SerializeObject(file));

        protected static ConverterFileInfo DeserializeFile(byte[] body) =>
            body is null ? null :
            (ConverterFileInfo)JsonConvert.DeserializeObject(SerializationEncoding.GetString(body), typeof(ConverterFileInfo));

        protected readonly string mqServerHost;

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

        public VideoConverterApplicationBase(string mqServerHost, string[] args, string[] argKeys)
        {
            this.mqServerHost = mqServerHost;

            T[] normalizeArray<T>(T[] array) => array ?? new T[] { };

            this.argDictionary = this.ParseArgs(
                normalizeArray(args),
                normalizeArray(argKeys));
        }

        public abstract Task RunAsync();
    }
}
