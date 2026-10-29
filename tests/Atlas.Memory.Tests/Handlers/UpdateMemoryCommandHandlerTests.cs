using Atlas.Memory.Commands;
using Atlas.Memory.Handlers;
using Atlas.Memory.Interfaces;
using Atlas.Memory.Models;
using Xunit;

namespace Atlas.Memory.Tests.Handlers;

/// <summary>
/// Unit tests for the <see cref="UpdateMemoryCommandHandler"/> class.
/// </summary>
public sealed class UpdateMemoryCommandHandlerTests
{
    /// <summary>
    /// Tests that the <see cref="UpdateMemoryCommandHandler.HandleAsync(UpdateMemoryCommand, CancellationToken)"/>
    /// method correctly updates the memory entry and preserves its metadata.
    /// </summary>
    [Fact]
    public async Task HandleAsync_Should_Update_Memory_And_Preserve_Metadata()
    {
        // Arrange
        var createdAt = DateTimeOffset.UtcNow.AddHours(-1);

        var interpretation = new AtlasMemoryInterpretation
        {
            Data = new AtlasTaskData
            {
                Description = "Call the dentist"
            }
        };

        var original = new AtlasMemoryEntry
        {
            Id = Guid.NewGuid(),
            Content = "Original content",
            CreatedAt = createdAt,
            Type = AtlasMemoryType.Fact,
            Interpretation = interpretation,
            LifecycleState = AtlasMemoryLifecycleState.Active
        };

        var memory = new RecordingMemory(original);
        var handler = new UpdateMemoryCommandHandler(memory);

        var command = new UpdateMemoryCommand(
            original.Id,
            "Updated content",
            AtlasMemoryType.Task);

        // Act
        var result = await handler.HandleAsync(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(memory.UpdatedMemory);
        Assert.Equal(original.Id, memory.UpdatedMemory.Id);
        Assert.Equal("Updated content", memory.UpdatedMemory.Content);
        Assert.Equal(createdAt, memory.UpdatedMemory.CreatedAt);
        Assert.Equal(AtlasMemoryType.Task, memory.UpdatedMemory.Type);
        Assert.Same(interpretation, memory.UpdatedMemory.Interpretation);
        Assert.Equal(
            AtlasMemoryLifecycleState.Active,
            memory.UpdatedMemory.LifecycleState);

        Assert.NotNull(result.UpdatedAt);
        Assert.Equal(original.Id, result.Id);
        Assert.Equal("Updated content", result.Content);
        Assert.Equal(createdAt, result.CreatedAt);
    }

    /// <summary>
    /// Tests that the <see cref="UpdateMemoryCommandHandler.HandleAsync(UpdateMemoryCommand, CancellationToken)"/>
    /// method correctly replaces the interpretation data when new data is supplied in the command.
    /// </summary>
    [Fact]
    public async Task HandleAsync_Should_Replace_Interpretation_When_Data_Is_Supplied()
    {
        // Arrange
        var original = CreateMemory(
            new AtlasTaskData
            {
                Description = "Call the dentist"
            });

        var replacement = new AtlasTaskData
        {
            Description = "Call the veterinarian"
        };

        var memory = new RecordingMemory(original);
        var handler = new UpdateMemoryCommandHandler(memory);

        var command = new UpdateMemoryCommand(
            original.Id,
            "Updated content",
            AtlasMemoryType.Task,
            replacement);

        // Act
        await handler.HandleAsync(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(memory.UpdatedMemory);
        Assert.NotNull(memory.UpdatedMemory.Interpretation);

        Assert.Same(
            replacement,
            memory.UpdatedMemory.Interpretation.Data);
    }

    /// <summary>
    /// Tests that the <see cref="UpdateMemoryCommandHandler.HandleAsync(UpdateMemoryCommand, CancellationToken)"/>
    /// throws a <see cref="KeyNotFoundException"/> when the memory entry to be updated does not exist.
    /// </summary>
    [Fact]
    public async Task HandleAsync_Should_Throw_When_Memory_Does_Not_Exist()
    {
        // Arrange
        var memory = new RecordingMemory(null);
        var handler = new UpdateMemoryCommandHandler(memory);

        var command = new UpdateMemoryCommand(
            Guid.NewGuid(),
            "Updated content",
            AtlasMemoryType.Fact);

        // Act
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => handler.HandleAsync(command, TestContext.Current.CancellationToken));

        // Assert
        Assert.Contains(command.MemoryId.ToString(), exception.Message);
        Assert.Null(memory.UpdatedMemory);
    }

    private static AtlasMemoryEntry CreateMemory(
        IAtlasMemoryData data)
    {
        return new AtlasMemoryEntry
        {
            Id = Guid.NewGuid(),
            Content = "Original content",
            CreatedAt = DateTimeOffset.UtcNow.AddMinutes(-10),
            Type = AtlasMemoryType.Task,
            Interpretation = new AtlasMemoryInterpretation
            {
                Data = data
            }
        };
    }

    private sealed class RecordingMemory(
        AtlasMemoryEntry? existingMemory) : IAtlasMemory
    {
        private AtlasMemoryEntry? _currentMemory = existingMemory;

        public AtlasMemoryEntry? UpdatedMemory { get; private set; }

        public Task StoreAsync(
            AtlasMemoryEntry memory,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task UpdateAsync(
            AtlasMemoryEntry memory,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            UpdatedMemory = memory;

            _currentMemory = new AtlasMemoryEntry
            {
                Id = memory.Id,
                Content = memory.Content,
                CreatedAt = memory.CreatedAt,
                UpdatedAt = DateTimeOffset.UtcNow,
                Type = memory.Type,
                Interpretation = memory.Interpretation,
                LifecycleState = memory.LifecycleState
            };

            return Task.CompletedTask;
        }

        public Task<AtlasMemoryEntry?> GetAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(
                _currentMemory?.Id == id
                    ? _currentMemory
                    : null);
        }

        public Task<IReadOnlyList<AtlasMemoryEntry>> SearchAsync(
            string query,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<IReadOnlyList<AtlasMemoryEntry>> SearchAsync(
            AtlasMemoryQuery query,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task ArchiveAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException(
                "ArchiveAsync is not supported by this test double.");
        }

        public Task DeleteAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException(
                "DeleteAsync is not supported by this test double.");
        }
    }
}
