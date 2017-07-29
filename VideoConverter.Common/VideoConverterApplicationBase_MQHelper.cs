using Common.Linq;
using Common.Diagnostics.Contracts;
using RabbitMQ.Client;

namespace VideoConverter.Common
{
    partial class VideoConverterApplicationBase
    {
        protected sealed class MQHelper
        {
            private readonly IModel channel;

            public MQHelper(IModel channel)
            {
                Contract.RequiresArgumentNotNull(channel, nameof(channel));

                this.channel = channel;
            }

            public void DeclareExchange() => this.channel.ExchangeDeclare(
                exchange: MQConsts.Exchanges.VideoConverterDefault,
                type: MQConsts.ExchangeTypes.Fanout,
                durable: false,
                autoDelete: false,
                arguments: null);

            public QueueDeclareOk DeclareQueue(string queueName) => this.channel.QueueDeclare(
                queue: queueName,
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            public void DeclareDefaults()
            {
                this.DeclareExchange();

                new[]
                {
                    MQConsts.Queues.IsCompleted,
                    MQConsts.Queues.FreeFiles,
                    MQConsts.Queues.FilesProcessedSuccessfully,
                    MQConsts.Queues.FilesFailed
                }
                .ForEach(queue => this.DeclareQueue(queue));
            }
        }
    }
}
