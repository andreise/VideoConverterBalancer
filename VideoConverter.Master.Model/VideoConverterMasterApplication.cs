using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.IO;
using System.Threading.Tasks;
using VideoConverter.Common;
using static System.Console;
using static System.FormattableString;

namespace VideoConverter.Master.Model
{
    public sealed partial class VideoConverterMasterApplication : VideoConverterApplicationBase
    {
        public VideoConverterMasterApplication(string mqServerHost, string[] args)
            : base(mqServerHost, args, ArgKeys.GetAll())
        {
        }

        private bool ValidateDirectories()
        {
            if (string.IsNullOrWhiteSpace(this.argDictionary[ArgKeys.SourceDirectory]))
            {
                WriteLine("Source directory not specified.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(this.argDictionary[ArgKeys.DestDirectory]))
            {
                WriteLine("Destination directory not specified.");
                return false;
            }

            if (!Directory.Exists(this.argDictionary[ArgKeys.SourceDirectory]))
            {
                WriteLine("Source directory not exists.");
                return false;
            }

            if (!Directory.Exists(this.argDictionary[ArgKeys.DestDirectory]))
            {
                try
                {
                    Directory.CreateDirectory(this.argDictionary[ArgKeys.DestDirectory]);
                }
                catch (Exception e)
                {
                    WriteLine(e.GetExtendedMessage("Cannot create destination directory."));
                    return false;
                }
            }

            return true;
        }

        public override async Task RunAsync()
        {
            WriteLine("Video Converter Master");

            if (!this.ValidateDirectories())
                return;

            var converterEngine = new ConverterEngine(
                this.argDictionary[ArgKeys.SourceDirectory],
                this.argDictionary[ArgKeys.SearchPattern] ?? Consts.DefaultSearchPattern,
                this.argDictionary[ArgKeys.DestDirectory]);

            try
            {
                await converterEngine.ScanSourceDirectory();
            }
            catch (Exception e)
            {
                WriteLine(e.GetExtendedMessage("Unexpected error"));
                return;
            }

            WriteLine(Invariant($"Total input files count: {converterEngine[ConverterFileStatus.None].Count}"));

            var factory = new ConnectionFactory() { HostName = this.mqServerHost };

            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(
                    exchange: MQConsts.Exchanges.VideoConverterDefault,
                    type: MQConsts.ExchangeTypes.Fanout,
                    durable: false,
                    autoDelete: false,
                    arguments: null);

                void DeclareQueue(string queueName) => channel.QueueDeclare(
                    queueName,
                    durable: false,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);

                DeclareQueue(MQConsts.Queues.IsCompleted);
                DeclareQueue(MQConsts.Queues.FreeFiles);
                DeclareQueue(MQConsts.Queues.FilesProcessedSuccessfully);
                DeclareQueue(MQConsts.Queues.FilesFailed);

                var consumer = new EventingBasicConsumer(channel);

                // Processing Messages From Nodes

                void Consumer_Received(object sender, BasicDeliverEventArgs e)
                {
                    if (e.Exchange != MQConsts.Exchanges.VideoConverterDefault)
                        return;

                    switch (e.RoutingKey)
                    {
                        case MQConsts.Queues.FilesProcessingStarted:
                            {
                                var file = DeserializeFile(e.Body);
                                if (!(file is null) && converterEngine[ConverterFileStatus.None].ContainsKey(file.SourcePath))
                                {
                                    converterEngine.StartFreeFileProcessing(file);
                                    WriteLine($"File '{file.SourcePath}' processing started.");
                                }
                            }
                            break;

                        case MQConsts.Queues.FilesProcessedSuccessfully:
                            {
                                var file = DeserializeFile(e.Body);
                                if (!(file is null))
                                {
                                    converterEngine.FinishFileProcessing(file, processedSuccessfully: true);
                                    WriteLine($"File '{file.SourcePath}' processed successfully.");
                                }
                            }
                            break;

                        case MQConsts.Queues.FilesFailed:
                            {
                                var file = DeserializeFile(e.Body);
                                if (!(file is null) && converterEngine[ConverterFileStatus.Processing].ContainsKey(file.SourcePath))
                                {
                                    converterEngine.FinishFileProcessing(file, processedSuccessfully: false);
                                    WriteLine(Invariant(
                                        $"File '{file.SourcePath}' processing failed at {file.ProcessingAttemps} attempt. Max attempts count is {converterEngine.MaxProcessingAttemps}."));
                                }
                            }
                            break;
                    }
                }

                consumer.Received += Consumer_Received;

                // Sending All Files To Queue For Nodes

                foreach(var freeFile in converterEngine[ConverterFileStatus.None].Values)
                {
                    channel.BasicPublish(
                        exchange: MQConsts.Exchanges.VideoConverterDefault,
                        routingKey: MQConsts.Queues.FreeFiles,
                        basicProperties: null,
                        body: SerializeFile(freeFile));
                }

                // Waiting Until Complete

                // TODO: Implement timeout for long converting operations
                // and changing corresponding files statuses from processing to free
                while (!converterEngine.IsCompleted())
                {
                    await Task.Delay(0);
                }

                consumer.Received -= Consumer_Received;

                channel.BasicPublish(
                    exchange: MQConsts.Exchanges.VideoConverterDefault,
                        routingKey: MQConsts.Queues.IsCompleted,
                        basicProperties: null,
                        body: null);

                WriteLine(Invariant($"Successfully converted files count: {converterEngine[ConverterFileStatus.ProcessedSuccessfully].Count}"));
                WriteLine(Invariant($"Failed files count: {converterEngine[ConverterFileStatus.Failed].Count}"));
            }
        }
    }
}
