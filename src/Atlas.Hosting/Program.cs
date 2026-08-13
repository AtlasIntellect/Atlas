using Atlas.Abstractions.Events;
using Atlas.Abstractions.Runtime;
using Atlas.Core.Events;
using Atlas.Core.Runtime;
using Atlas.Hosting.Runtime;
using Atlas.Hosting.Startup;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .AddSingleton<IAtlasEventDispatcher, AtlasEventDispatcher>()
    .AddSingleton<IAtlasRuntime, AtlasRuntime>()
    .AddSingleton<IAtlasEventHandlerBase, StartupHandler>()
    .AddHostedService<AtlasRuntimeHostedService>();

using var host = builder.Build();

await host.RunAsync();