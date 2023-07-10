using System.Diagnostics;
using System.Text.Json.Serialization;
using Contracts;
using MassTransit;
using Microsoft.AspNetCore.Http.Json;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

using WebApplication2.Data;

// Define some important constants to initialize tracing with
var serviceName = "MyCompany.MyProduct.MyService";
var serviceVersion = "1.0.0";

var builder = WebApplication.CreateBuilder(args);

// Configure important OpenTelemetry settings, the console exporter, and instrumentation library
builder.Services.AddOpenTelemetryTracing(tracerProviderBuilder =>
{
    tracerProviderBuilder
    .AddConsoleExporter()
    .AddZipkinExporter(o =>
     {
        var zipkinUrl = builder.Configuration["ZIPKIN_URL"] ?? "http://localhost:9411";
        o.Endpoint = new Uri($"{zipkinUrl}/api/v2/spans");
        o.ExportProcessorType = OpenTelemetry.ExportProcessorType.Simple;
     })
    .AddSource(serviceName)
    .AddSource("MassTransit")
    .SetResourceBuilder(
        ResourceBuilder.CreateDefault()
            .AddService(serviceName: serviceName, serviceVersion: serviceVersion))
    .AddHttpClientInstrumentation()
    .AddAspNetCoreInstrumentation()
    .AddSqlClientInstrumentation()
    .AddMassTransitInstrumentation();
});

// Add services to the container.

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSqlServer<AppDbContext>(
    builder.Configuration.GetConnectionString("mssql") ?? builder.Configuration.GetConnectionString("AppDbConnection"));

builder.Services.AddMassTransit(x =>
{
    x.SetKebabCaseEndpointNameFormatter();

    x.AddConsumers(typeof(Program).Assembly);

    x.UsingRabbitMq((context, cfg) =>
    {
        var rabbitmqHost = builder.Configuration["RABBITMQ_HOST"] ?? "localhost";

        cfg.Host(rabbitmqHost, "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });

        cfg.ConfigureEndpoints(context);
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

var MyActivitySource = new ActivitySource(serviceName);

app.MapGet("/hello", () =>
{
    // Track work inside of the request
    using var activity = MyActivitySource.StartActivity("SayHello");
    activity?.SetTag("foo", 1);
    activity?.SetTag("bar", "Hello, World!");
    activity?.SetTag("baz", new int[] { 1, 2, 3 });

    return "Hello, World!";
});

HttpClient client = new()
{
    BaseAddress = app.Configuration.GetServiceUri("some-service", "https") ?? new Uri(app.Configuration["SOMESERVICE_URL"]!)
};

app.MapGet("/test", async () =>
{
    return await client.GetStringAsync("/weatherforecast");
});

app.MapPost("/test2", async (IPublishEndpoint publishEndpoint, IRequestClient<Foo> requestClient, string text) =>
{
    var response = await requestClient.GetResponse<FooResponse>(new Foo(text));
    return response.Message;

    //await publishEndpoint.Publish(new Foo(text));
});

using (var scope = app.Services.CreateScope())
{
    using var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    //await context.Database.EnsureDeletedAsync();
    await context.Database.EnsureCreatedAsync();
}

await app.RunAsync();
