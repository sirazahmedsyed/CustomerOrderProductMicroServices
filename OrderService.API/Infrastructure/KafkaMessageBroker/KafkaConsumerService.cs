using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Microsoft.Extensions.Options;
using OrderService.API.Infrastructure.DTOs;
using System.Text.Json;

namespace OrderService.API.Infrastructure.KafkaMessageBroker
{
    public class KafkaConsumerService : BackgroundService
    {
        private readonly IConsumer<string, string> _consumer;
        private readonly ILogger<KafkaConsumerService> _logger;
        private readonly KafkaSettings _kafkaSettings;
        private readonly IAdminClient _adminClient;

        public KafkaConsumerService(IOptions<KafkaSettings> kafkaSettings, ILogger<KafkaConsumerService> logger)
        {
            _kafkaSettings = kafkaSettings.Value;
            _logger = logger;

            // Create admin client for topic management
            var adminConfig = new AdminClientConfig
            {
                BootstrapServers = _kafkaSettings.BootstrapServers
            };
            _adminClient = new AdminClientBuilder(adminConfig).Build();

            var config = new ConsumerConfig
            {
                BootstrapServers = _kafkaSettings.BootstrapServers,
                GroupId = "order-service-group",
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false
            };

            _consumer = new ConsumerBuilder<string, string>(config).Build();
            // Ensure topics exist before starting
           // EnsureTopicsExist().GetAwaiter().GetResult();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                // Attempt to ensure topics exist when the service starts
                await EnsureTopicsExist(stoppingToken);

                var topics = new[]
                {
                    _kafkaSettings.Topics.OrderCreated,
                    _kafkaSettings.Topics.OrderUpdated,
                    _kafkaSettings.Topics.OrderDeleted
                };

                _consumer.Subscribe(topics);
                _logger.LogInformation($"Subscribed to topics: {string.Join(", ", topics)}");

                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        var consumeResult = _consumer.Consume(stoppingToken);
                        if (consumeResult != null && consumeResult.Message != null)
                        {
                            await HandleMessage(consumeResult);
                            _consumer.Commit(consumeResult);
                        }
                    }
                    catch (ConsumeException ex)
                    {
                        _logger.LogError(ex, "Error consuming from Kafka: {0}", ex.Error.Reason);
                        // Add delay to prevent tight loop in case of persistent errors
                        await Task.Delay(1000, stoppingToken);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Normal shutdown, don't treat as error. This is expected when the service is stopping
                _logger.LogInformation("Kafka consumer stopping due to cancellation request.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Kafka consumer");
                throw;
            }
            finally
            {
                // Close the consumer when the service is stopping
                _consumer.Close();
                _logger.LogInformation("Kafka consumer closed.");
            }
        }

        private async Task EnsureTopicsExist(CancellationToken cancellationToken)
        {
            try
            {
                    var topics = new[]
                    {
                    _kafkaSettings.Topics.OrderCreated,
                    _kafkaSettings.Topics.OrderUpdated,
                    _kafkaSettings.Topics.OrderDeleted
                    };

                // Since GetMetadata is synchronous, we wrap it in Task.Run to make it async
                var metadata = await Task.Run(() => _adminClient.GetMetadata(TimeSpan.FromSeconds(60)), cancellationToken);
                if (metadata == null)
                {
                    _logger.LogError("Failed to retrieve metadata from Kafka.");
                    return; // Continue service operation but log the failure
                }
                var existingTopics = metadata.Topics.Select(t => t.Topic).ToList();
                _logger.LogInformation($"Existing topics: {string.Join(", ", existingTopics)}");
                _logger.LogInformation($"Topics to check: {string.Join(", ", topics)}");

                var topicsToCreate = topics.Where(topic => !existingTopics.Contains(topic)).ToList();
                _logger.LogInformation($"Topics to create: {string.Join(", ", topicsToCreate)}");

                if (topicsToCreate.Any())
                {
                    var topicSpecs = topicsToCreate.Select(topic => new TopicSpecification
                    {
                        Name = topic,
                        NumPartitions = 1,
                        ReplicationFactor = 1
                    });

                    // CreateTopicsAsync does not accept CancellationToken directly, so using a timeout
                    var createOptions = new CreateTopicsOptions { RequestTimeout = TimeSpan.FromSeconds(30) };
                    await _adminClient.CreateTopicsAsync(topicSpecs, createOptions);
                    _logger.LogInformation($"Created Kafka topics: {string.Join(", ", topicsToCreate)}");
                }
            }
            catch (Exception ex)
            {
                // Log the error but continue running the service
                _logger.LogError(ex, "Error ensuring Kafka topics exist");
            }
        }
       
        private async Task HandleMessage(ConsumeResult<string, string> consumeResult)
        {
            try
            {
                var orderDto = JsonSerializer.Deserialize<OrderDTO>(consumeResult.Message.Value);
                switch (consumeResult.Topic)
                {
                    case var t when t == _kafkaSettings.Topics.OrderCreated:
                        _logger.LogInformation($"Order created: {orderDto.OrderId}");
                        break;

                    case var t when t == _kafkaSettings.Topics.OrderUpdated:
                        _logger.LogInformation($"Order updated: {orderDto.OrderId}");
                        break;

                    case var t when t == _kafkaSettings.Topics.OrderDeleted:
                        _logger.LogInformation($"Order deleted: {orderDto.OrderId}");
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error processing message from topic {consumeResult.Topic}");
                throw;
            }
        }

        public override void Dispose()
        {
           // _consumer.Close();
            _consumer.Dispose();
            _adminClient?.Dispose();
            base.Dispose();
        }
    }
}
