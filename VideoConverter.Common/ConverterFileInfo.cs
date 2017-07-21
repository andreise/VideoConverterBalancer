using Common.Diagnostics.Contracts;
using Newtonsoft.Json;

namespace VideoConverter.Common
{
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public sealed class ConverterFileInfo
    {
        [JsonProperty(PropertyName = "sourcePath")]
        public string SourcePath { get; }

        [JsonProperty(PropertyName = "destPath")]
        public string DestPath { get; }

        public int ProcessingAttemps { get; private set; }

        public void ResetProcessingAttemps() => this.ProcessingAttemps = 0;

        public void AddProcessingAttemp() => this.ProcessingAttemps++;

        public ConverterFileInfo(string sourcePath, string destPath)
        {
            Contract.RequiresArgumentNotNull(sourcePath, nameof(sourcePath));
            Contract.RequiresArgumentNotNull(destPath, nameof(destPath));

            this.SourcePath = sourcePath;
            this.DestPath = destPath;
        }
    }
}
