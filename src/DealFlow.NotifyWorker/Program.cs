using DealFlow.NotifyWorker.Consumers;
using MassTransit;
using Serilog;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSerilog(cfg =>
    cfg.ReadFrom.Configuration(builder.Configuration)
       .WriteTo.Console());

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<DealScoredConsumer>();

    x.UsingRabbitMq((ctx, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMQ:Host"] ?? "localhost", "/", h =>
        {
            h.Username(builder.Configuration["RabbitMQ:Username"] ?? "guest");
            h.Password(builder.Configuration["RabbitMQ:Password"] ?? "guest");
        });

        cfg.ReceiveEndpoint("deal-scored-notify", e =>
        {
            e.UseMessageRetry(r => r.Incremental(3, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2)));
            e.ConfigureConsumer<DealScoredConsumer>(ctx);
        });
    });
});

var host = builder.Build();
host.Run();
