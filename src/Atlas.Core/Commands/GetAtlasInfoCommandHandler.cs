using Atlas.Abstractions.Commands;
using Atlas.Abstractions.Runtime;

namespace Atlas.Core.Commands;

/// <summary>
/// Represents a command handler for retrieving information about the current Atlas instance.
/// </summary>
/// <param name="applicationContext">The application context.</param>
public sealed class GetAtlasInfoCommandHandler(IAtlasApplicationContext applicationContext)
    : IAtlasCommandHandler<GetAtlasInfoCommand, AtlasInfo>
{
    /// <inheritdoc />
    public Task<AtlasInfo> HandleAsync(
        GetAtlasInfoCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = new AtlasInfo
        {
            Name = applicationContext.Name,
            InstanceId = applicationContext.InstanceId
        };

        return Task.FromResult(result);
    }
}