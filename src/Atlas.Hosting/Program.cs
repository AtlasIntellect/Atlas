using Atlas.Hosting.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddAtlas();

using var host = builder.Build();

await host.RunAsync();