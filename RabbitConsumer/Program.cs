using MassTransit;
using MassTransit.Logging;
using Messages;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

IBusControl bus = Bus.Factory.CreateUsingRabbitMq(options =>
{
    options.Host(new Uri($"amqps://guest:guest@localhost:5672/"));
    options.UseDelayedMessageScheduler();
    
    options.ReceiveEndpoint(nameof(TextMessage) + "_Low", x =>
    {    
        x.ConfigureConsumeTopology = false;
    
        x.Consumer<MyLowTextMessageConsumer>();
        
        x.Bind<TextMessage>(s => 
        {
            s.RoutingKey = "Low";
            s.ExchangeType = ExchangeType.Direct;
        });
    });

    options.ReceiveEndpoint(nameof(TextMessage) + "_High", x =>
    {
        x.ConfigureConsumeTopology = false;
        
        x.Consumer<MyHighTextMessageConsumer>();
        
        x.Bind<TextMessage>(s => 
        {
            s.RoutingKey = "High";
            s.ExchangeType = ExchangeType.Direct;
        });
    });
    
    LogContext.ConfigureCurrentLogContext(new TextWriterLoggerFactory(Console.Out, 
        new OptionsWrapper<TextWriterLoggerOptions>(new TextWriterLoggerOptions()
        {
            LogLevel = LogLevel.Debug
        })));
});
bus.Start();
Console.WriteLine("Listening for messages. Hit <return> to quit.");
Console.ReadLine();
bus.Stop();


class MyHighTextMessageConsumer : IConsumer<TextMessage>
{
    public async Task Consume(ConsumeContext<TextMessage> context)
    {
        Console.WriteLine("Got High message: {0} {1} {2}", DateTime.UtcNow, context.Message.Text, context.Message.Priority);
    }
}

class MyLowTextMessageConsumer : IConsumer<TextMessage>
{
    public async Task Consume(ConsumeContext<TextMessage> context)
    {
        Console.WriteLine("Got Low message: {0} {1} {2}", DateTime.UtcNow, context.Message.Text, context.Message.Priority);
    }
}