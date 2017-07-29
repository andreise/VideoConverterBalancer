using Common.Diagnostics.Contracts;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using VideoConverter.Common;
using static System.Console;
using static System.FormattableString;

namespace VideoConverter.Node.Model
{
    public sealed class VideoConverterNodeApplication : VideoConverterApplicationBase
    {
        private readonly string converterCommandLineFormat;

        public VideoConverterNodeApplication(string mqServerHost, string converterCommandLineFormat, string[] args)
            : base(mqServerHost, args, new string[] { })
        {
            Contract.RequiresArgumentNotNull(converterCommandLineFormat, nameof(converterCommandLineFormat));

            this.converterCommandLineFormat = converterCommandLineFormat;
        }

        public override async Task RunAsync()
        {
            WriteLine("Video Converter Node");

            if (this.converterCommandLineFormat.Length == 0)
            {
                WriteLine("Converter command line format is unspecified.");
                return;
            }

            int processedFiles = 0;

            // TODO: Implement max count processing files at the same time instead one file
            bool isBusy = false;

            bool isCompleted = false;

            var factory = new ConnectionFactory() { HostName = this.mqServerHost };

            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                new MQHelper(channel).DeclareDefaults();

                var consumer = new EventingBasicConsumer(channel);

                void Consumer_Received(object sender, BasicDeliverEventArgs e)
                {
                    if (e.Exchange != MQConsts.Exchanges.VideoConverterDefault)
                        return;

                    switch (e.RoutingKey)
                    {
                        case MQConsts.Queues.IsCompleted:
                            {
                                isCompleted = true;
                            }
                            break;

                        case MQConsts.Queues.FreeFiles:
                            {
                                ConverterFileInfo file;
                                if (!isBusy && !((file = DeserializeFile(e.Body)) is null))
                                {
                                    isBusy = true;

                                    // TODO: Parse commandLine in constructor
                                    string[] commandLineParts = this.converterCommandLineFormat.Split((char[])null, 2, StringSplitOptions.RemoveEmptyEntries);
                                    string converterPath = commandLineParts.First();
                                    string converterArgsFormat = commandLineParts.Skip(1).First();
                                    string converterArgs = string.Format(CultureInfo.InvariantCulture, file.SourcePath, file.DestPath);

                                    // TODO: await until process finished and determine success or failure result
                                    Process.Start(converterPath, converterArgs);
                                    
                                    // TODO: await until process finished and determine success or failure result
                                    bool IsSuccess() => true;

                                    bool isSuccess = IsSuccess();

                                    processedFiles++;

                                    channel.BasicPublish(
                                        exchange: MQConsts.Exchanges.VideoConverterDefault,
                                        routingKey: isSuccess ? MQConsts.Queues.FilesProcessedSuccessfully : MQConsts.Queues.FilesFailed,
                                        basicProperties: null,
                                        body: SerializeFile(file)); // e.Body

                                    isBusy = false;
                                }
                            }
                            break;
                    }
                }

                consumer.Received += Consumer_Received;

                while (!isCompleted)
                {
                    await Task.Delay(0);
                }

                consumer.Received -= Consumer_Received;

                WriteLine(Invariant($"Completed. Processed files count by this node: {processedFiles}."));
            }
        }
    }
}
