using Atlas.Hosting.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Atlas.Testing.Persistence;

/// <summary>
/// Provides an isolated SQLite environment for persistent Atlas memory integration tests.
/// </summary>
public sealed class PersistentMemoryTestEnvironment : IDisposable
{
    private bool _disposed;

    /// <summary>
    /// Initializes a new isolated SQLite database environment.
    /// </summary>
    public PersistentMemoryTestEnvironment()
    {
        DatabasePath =
            Path.Combine(
                Path.GetTempPath(),
                $"atlas-memory-{Guid.NewGuid():N}.db");

        ConnectionString =
            $"Data Source={DatabasePath};Pooling=False";
    }

    /// <summary>
    /// Gets the path to the isolated SQLite database.
    /// </summary>
    public string DatabasePath { get; }

    /// <summary>
    /// Gets the SQLite connection string for the isolated database.
    /// </summary>
    public string ConnectionString { get; }

    /// <summary>
    /// Creates a new Atlas application builder configured to use
    /// this environment's SQLite database.
    /// </summary>
    /// <returns>A configured Atlas application builder.</returns>
    public HostApplicationBuilder CreateBuilder()
    {
        ThrowIfDisposed();

        var builder =
            Host.CreateApplicationBuilder();

        builder.Configuration.AddInMemoryCollection(
            new Dictionary<string, string?>
            {
                ["Atlas:Memory:StorageMode"] = "Sqlite",
                ["ConnectionStrings:AtlasMemory"] =
                    ConnectionString,
                ["Atlas:Interaction:InterpreterMode"] =
                    "Deterministic"
            });

        builder.Services.AddAtlas(
            builder.Configuration);

        return builder;
    }

    /// <summary>
    /// Disposes the environment and removes its database files.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        DeleteDatabaseFiles();
    }

    private void DeleteDatabaseFiles()
    {
        var files =
            new[]
            {
                DatabasePath,
                $"{DatabasePath}-shm",
                $"{DatabasePath}-wal"
            };

        foreach (var file in files.Where(File.Exists))
        {
            File.Delete(file);
        }
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(
            _disposed,
            this);
    }
}
