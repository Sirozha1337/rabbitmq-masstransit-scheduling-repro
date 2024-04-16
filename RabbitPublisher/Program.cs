using MassTransit;
using MassTransit.Logging;
using Messages;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

IBusControl bus = Bus.Factory.CreateUsingRabbitMq(options =>
{
    options.Host(new Uri($"amqps://guest:guest@localhost:5672/"));
    
    options.Send<TextMessage>(c =>
    {
        c.UseRoutingKeyFormatter(msg => msg.Message.Priority);
    });
    options.UseDelayedMessageScheduler();
    options.Publish<TextMessage>(c =>
    {
        c.ExchangeType = ExchangeType.Direct;
    });
    
    LogContext.ConfigureCurrentLogContext(new TextWriterLoggerFactory(Console.Out, 
        new OptionsWrapper<TextWriterLoggerOptions>(new TextWriterLoggerOptions()
    {
        LogLevel = LogLevel.Debug
    })));
});

bus.Start();

Console.WriteLine("Publishing first message");
await bus.Publish(new TextMessage { Text = $"High Priority", Priority = "High" });
Console.WriteLine("Published first message");

Console.WriteLine("Publishing second message");
await bus.Publish(new TextMessage { Text = $"Low Priority 1", Priority = "Low" });
Console.WriteLine("Published second message");

Console.WriteLine("Publishing third message with 30 seconds delay");
var scheduler = bus.CreateDelayedMessageScheduler();
await scheduler.SchedulePublish(DateTime.UtcNow.AddSeconds(30), new TextMessage { Text = $"Low Priority 2", Priority = "Low" });
Console.WriteLine("Published third message");

bus.Stop();

