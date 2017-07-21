using Common.Diagnostics.Contracts;
using Common.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using VideoConverter.Common;

namespace VideoConverter.Master.Model
{
    using FileDictionary = IReadOnlyDictionary<ConverterFileStatus, Dictionary<string, ConverterFileInfo>>;
    using FileSubdictionary = Dictionary<string, ConverterFileInfo>;

    internal sealed class ConverterEngine
    {
        private static StringComparer GetPathComparer(bool isCaseSensitive) => isCaseSensitive ?
            StringComparer.Ordinal :
            StringComparer.OrdinalIgnoreCase;

        private static void ResetFileDictionary(FileDictionary files) => files.ForEach(item => item.Value.Clear());

        private readonly StringComparer pathComparer;

        private readonly string sourceDirectory;
        private readonly string searchPattern;
        private readonly string destDirectory;

        private readonly FileDictionary files;
        private readonly FileDictionary completedFiles;
        private readonly FileDictionary uncompletedFiles;

        private bool isScanningSourceDirectory;

        public int MaxProcessingAttemps => 10;

        public ConverterEngine(
            string sourceDirectory,
            string searchPattern,
            string destDirectory,
            bool isCaseSensitive = true)
        {
            Contract.RequiresArgumentNotNull(sourceDirectory, nameof(sourceDirectory));
            Contract.RequiresArgumentNotNull(searchPattern, nameof(searchPattern));
            Contract.RequiresArgumentNotNull(destDirectory, nameof(destDirectory));

            this.sourceDirectory = sourceDirectory;
            this.searchPattern = searchPattern;
            this.destDirectory = destDirectory;

            this.pathComparer = GetPathComparer(isCaseSensitive);

            this.files =
                new[]
                {
                    ConverterFileStatus.None,
                    ConverterFileStatus.Processing,
                    ConverterFileStatus.ProcessedSuccessfully,
                    ConverterFileStatus.Failed
                }
                .ToDictionary(status => status, status => new FileSubdictionary(this.pathComparer));

            FileDictionary SelectFiles(params ConverterFileStatus[] statuses) =>
                statuses
                .ToDictionary(status => status, status => this.files[status]);

            this.completedFiles = SelectFiles(
                ConverterFileStatus.ProcessedSuccessfully,
                ConverterFileStatus.Failed);

            this.uncompletedFiles = SelectFiles(
                ConverterFileStatus.None,
                ConverterFileStatus.Processing);
        }

        public IReadOnlyDictionary<string, ConverterFileInfo> this[ConverterFileStatus status] => this.files[status];

        public void Reset() => ResetFileDictionary(this.files);

        public void ResetCompleted() => ResetFileDictionary(this.completedFiles);

        public void ResetUncompleted() => ResetFileDictionary(this.uncompletedFiles);

        public void Reset(ConverterFileStatus status) => this.files[status].Clear();

        public bool IsCompleted() => this.uncompletedFiles.All(item => item.Value.Count == 0);

        public void StartFreeFileProcessing(ConverterFileInfo freeFile)
        {
            Contract.RequiresArgumentNotNull(freeFile, nameof(freeFile));

            Contract.RequiresArgument(this.files[ConverterFileStatus.None].ContainsKey(freeFile.SourcePath));

            this.files[ConverterFileStatus.None].Remove(freeFile.SourcePath);

            this.files[ConverterFileStatus.Processing].Add(freeFile.SourcePath, freeFile);
        }

        public ConverterFileStatus FinishFileProcessing(ConverterFileInfo processingFile, bool processedSuccessfully)
        {
            Contract.RequiresArgumentNotNull(processingFile, nameof(processingFile));

            Contract.RequiresArgument(this.files[ConverterFileStatus.Processing].ContainsKey(processingFile.SourcePath), "File is not a processing file.");

            this.files[ConverterFileStatus.Processing].Remove(processingFile.SourcePath);

            ConverterFileStatus GetNewStatus()
            {
                if (processedSuccessfully)
                    return ConverterFileStatus.ProcessedSuccessfully;

                if (processingFile.ProcessingAttemps < this.MaxProcessingAttemps)
                    return ConverterFileStatus.None;

                return ConverterFileStatus.Failed;
            }

            ConverterFileStatus newStatus = GetNewStatus();
            
            this.files[newStatus].Add(processingFile.SourcePath, processingFile);

            return newStatus;
        }

        public Task ScanSourceDirectory()
        {
            string[] GetSourceFiles() => Directory.GetFiles(
                this.sourceDirectory,
                this.searchPattern,
                SearchOption.AllDirectories);

            string GetRelativePath(string sourceDirectory, string sourcePath)
            {
                bool IsSeparator(char c) => PathUtils.NativeDirectorySeparatorChars.Contains(c);

                return new string(
                    sourcePath
                    .Skip(this.sourceDirectory.Length)
                    .SkipWhile(c => IsSeparator(c))
                    .ToArray());
            }

            void AddSourceFile(string sourcePath, string sourcePathSystemIndependent)
            {
                string relativePath = GetRelativePath(this.sourceDirectory, sourcePath);
                string destPath = Path.Combine(this.destDirectory, relativePath);
                string destPathSystemIndependent = PathUtils.GetSystemIndependentPath(destPath);

                var converterFileInfo = new ConverterFileInfo(sourcePathSystemIndependent, destPathSystemIndependent);
                this.files[ConverterFileStatus.None].Add(sourcePathSystemIndependent, converterFileInfo);
            }

            if (this.isScanningSourceDirectory)
                return Task.CompletedTask;

            this.isScanningSourceDirectory = true;
            try
            {
                var nonProcessingFiles = this.files
                    .Where(item => item.Key != ConverterFileStatus.Processing)
                    .Select(item => item.Value)
                    .ToArray();

                var sourceFiles = GetSourceFiles();

                sourceFiles.ForEach(
                    sourcePath =>
                    {
                        var sourcePathSystemIndependent = PathUtils.GetSystemIndependentPath(sourcePath);

                        if (this.files.Any(item => item.Value.ContainsKey(sourcePathSystemIndependent)))
                            return;

                        AddSourceFile(sourcePath, sourcePathSystemIndependent);
                    });
            }
            finally
            {
                this.isScanningSourceDirectory = false;
            }

            return Task.CompletedTask;
        }
    }
}
