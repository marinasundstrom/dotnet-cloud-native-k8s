using Contracts;
using MassTransit;

namespace WebApplication2.Consumers;

public class FooConsumer : IConsumer<Foo>
{
    private readonly IConfiguration configuration;

    HttpClient client;

    public FooConsumer(IConfiguration configuration)
    {
        this.configuration = configuration;
        this.client = new()
        {
            BaseAddress = configuration.GetServiceUri("web", "https") ?? new Uri(configuration["WEBAPPLICATION2_URL"]!)
        };
    }

    //public Task Consume(ConsumeContext<Foo> context)
    //{
    //    Console.WriteLine($"Foo: {context.Message.Text}");

    //    return Task.CompletedTask;
    //}

    public async Task Consume(ConsumeContext<Foo> context)
    {
        Console.WriteLine($"Foo: {context.Message.Text}");

        await client.GetAsync("/test");

        await Task.Delay(Random.Shared.Next(0, 2000));

        await context.RespondAsync(new FooResponse($"Echo: \"{context.Message.Text}\", from {Environment.MachineName}"));
    }
}
