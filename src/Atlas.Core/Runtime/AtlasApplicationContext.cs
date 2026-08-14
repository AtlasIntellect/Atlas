using Atlas.Abstractions.Configuration;
using Atlas.Abstractions.Runtime;
using Microsoft.Extensions.Options;

namespace Atlas.Core.Runtime;

/// <inheritdoc/>
public sealed class AtlasApplicationContext(
    IOptions<AtlasOptions> options) : IAtlasApplicationContext
{
    /// <inheritdoc/>
    public string Name => options.Value.Name;

    /// <inheritdoc/>
    public Guid InstanceId { get; } = Guid.NewGuid();
}