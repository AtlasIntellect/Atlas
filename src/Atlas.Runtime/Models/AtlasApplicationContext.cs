using Atlas.Abstractions.Configuration;
using Atlas.Runtime.Interfaces;
using Microsoft.Extensions.Options;

namespace Atlas.Runtime.Models;

/// <inheritdoc/>
public sealed class AtlasApplicationContext(
    IOptions<AtlasOptions> options) : IAtlasApplicationContext
{
    /// <inheritdoc/>
    public string Name => options.Value.Name;

    /// <inheritdoc/>
    public Guid InstanceId { get; } = Guid.NewGuid();
}