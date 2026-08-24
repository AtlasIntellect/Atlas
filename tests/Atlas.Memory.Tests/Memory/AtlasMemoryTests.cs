using Atlas.Abstractions.Memory;
using Xunit;

namespace Atlas.Memory.Tests.Memory;

/// <summary>
/// Provides unit tests for the <see cref="AtlasMemory"/> class.
/// </summary>
public sealed class AtlasMemoryTests
{
    /// <summary>
    /// Verifies that <see cref="AtlasMemory.StoreAsync"/> stores a memory.
    /// </summary>
    [Fact]
    public async Task StoreAsync_Should_StoreMemory()
    {
        var memory = new AtlasMemory();
        var entry = CreateMemory();

        await memory.StoreAsync(
            entry,
            TestContext.Current.CancellationToken);

        var result = await memory.GetAsync(
            entry.Id,
            TestContext.Current.CancellationToken);

        Assert.Equal(entry, result);
    }

    /// <summary>
    /// Verifies that <see cref="AtlasMemory.GetAsync"/> returns a previously stored memory.
    /// </summary>
    [Fact]
    public async Task GetAsync_Should_ReturnStoredMemory()
    {
        var memory = new AtlasMemory();
        var entry = CreateMemory();

        await memory.StoreAsync(
            entry,
            TestContext.Current.CancellationToken);

        var result = await memory.GetAsync(
            entry.Id,
            TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.Equal(entry.Id, result.Id);
        Assert.Equal(entry.Content, result.Content);
        Assert.Equal(entry.CreatedAt, result.CreatedAt);
    }

    /// <summary>
    /// Verifies that <see cref="AtlasMemory.GetAsync"/> returns null
    /// when the requested memory does not exist.
    /// </summary>
    [Fact]
    public async Task GetAsync_Should_ReturnNull_WhenMemoryDoesNotExist()
    {
        var memory = new AtlasMemory();

        var result = await memory.GetAsync(
            Guid.NewGuid(),
            TestContext.Current.CancellationToken);

        Assert.Null(result);
    }

    /// <summary>
    /// Verifies that storing another memory with the same identifier replaces
    /// the previously stored memory.
    /// </summary>
    [Fact]
    public async Task StoreAsync_Should_ReplaceExistingMemoryWithSameId()
    {
        var memory = new AtlasMemory();
        var id = Guid.NewGuid();

        var first = new AtlasMemoryEntry
        {
            Id = id,
            Content = "First memory",
            CreatedAt = DateTimeOffset.UtcNow
        };

        var second = new AtlasMemoryEntry
        {
            Id = id,
            Content = "Updated memory",
            CreatedAt = first.CreatedAt.AddMinutes(1)
        };

        await memory.StoreAsync(
            first,
            TestContext.Current.CancellationToken);

        await memory.StoreAsync(
            second,
            TestContext.Current.CancellationToken);

        var result = await memory.GetAsync(
            id,
            TestContext.Current.CancellationToken);

        Assert.Equal(second, result);
    }

    /// <summary>
    /// Verifies that <see cref="AtlasMemory.StoreAsync"/> throws when the
    /// cancellation token has already been cancelled.
    /// </summary>
    [Fact]
    public async Task StoreAsync_Should_Throw_WhenCancellationRequested()
    {
        var memory = new AtlasMemory();
        var entry = CreateMemory();

        using var cancellationTokenSource = new CancellationTokenSource();
        await cancellationTokenSource.CancelAsync();

        await Assert.ThrowsAsync<OperationCanceledException>(
            () => memory.StoreAsync(
                entry,
                cancellationTokenSource.Token));
    }

    /// <summary>
    /// Verifies that <see cref="AtlasMemory.SearchAsync(string, CancellationToken)"/> returns memories
    /// containing the specified query.
    /// </summary>
    [Fact]
    public async Task SearchAsync_Should_ReturnMatchingMemories()
    {
        var memory = new AtlasMemory();

        var matchingEntry = new AtlasMemoryEntry
        {
            Id = Guid.NewGuid(),
            Content = "I bought a Canon camera",
            CreatedAt = DateTimeOffset.UtcNow
        };

        var nonMatchingEntry = new AtlasMemoryEntry
        {
            Id = Guid.NewGuid(),
            Content = "I like pizza",
            CreatedAt = DateTimeOffset.UtcNow
        };

        await memory.StoreAsync(
            matchingEntry,
            TestContext.Current.CancellationToken);

        await memory.StoreAsync(
            nonMatchingEntry,
            TestContext.Current.CancellationToken);

        var result = await memory.SearchAsync(
            "Canon",
            TestContext.Current.CancellationToken);

        var entry = Assert.Single(result);
        Assert.Equal(matchingEntry, entry);
    }

    /// <summary>
    /// Verifies that <see cref="AtlasMemory.SearchAsync(string, CancellationToken)"/> performs a
    /// case-insensitive search.
    /// </summary>
    [Fact]
    public async Task SearchAsync_Should_BeCaseInsensitive()
    {
        var memory = new AtlasMemory();

        var entry = new AtlasMemoryEntry
        {
            Id = Guid.NewGuid(),
            Content = "I bought a Canon camera",
            CreatedAt = DateTimeOffset.UtcNow
        };

        await memory.StoreAsync(
            entry,
            TestContext.Current.CancellationToken);

        var result = await memory.SearchAsync(
            "CANON",
            TestContext.Current.CancellationToken);

        var matchedEntry = Assert.Single(result);
        Assert.Equal(entry, matchedEntry);
    }

    /// <summary>
    /// Verifies that <see cref="AtlasMemory.SearchAsync(string, CancellationToken)"/> supports partial
    /// matches within memory content.
    /// </summary>
    [Fact]
    public async Task SearchAsync_Should_ReturnPartialMatches()
    {
        var memory = new AtlasMemory();

        var entry = new AtlasMemoryEntry
        {
            Id = Guid.NewGuid(),
            Content = "I bought a Canon camera",
            CreatedAt = DateTimeOffset.UtcNow
        };

        await memory.StoreAsync(
            entry,
            TestContext.Current.CancellationToken);

        var result = await memory.SearchAsync(
            "cam",
            TestContext.Current.CancellationToken);

        var matchedEntry = Assert.Single(result);
        Assert.Equal(entry, matchedEntry);
    }

    /// <summary>
    /// Verifies that <see cref="AtlasMemory.SearchAsync(string, CancellationToken)"/> returns an empty
    /// collection when no memories match the query.
    /// </summary>
    [Fact]
    public async Task SearchAsync_Should_ReturnEmpty_WhenNoMemoriesMatch()
    {
        var memory = new AtlasMemory();
        var entry = CreateMemory();

        await memory.StoreAsync(
            entry,
            TestContext.Current.CancellationToken);

        var result = await memory.SearchAsync(
            "nonexistent",
            TestContext.Current.CancellationToken);

        Assert.Empty(result);
    }

    /// <summary>
    /// Verifies that <see cref="AtlasMemory.SearchAsync(string, CancellationToken)"/> returns an empty
    /// collection when the query is empty or whitespace.
    /// </summary>
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public async Task SearchAsync_Should_ReturnEmpty_WhenQueryIsEmpty(
        string query)
    {
        var memory = new AtlasMemory();
        var entry = CreateMemory();

        await memory.StoreAsync(
            entry,
            TestContext.Current.CancellationToken);

        var result = await memory.SearchAsync(
            query,
            TestContext.Current.CancellationToken);

        Assert.Empty(result);
    }

    /// <summary>
    /// Verifies that <see cref="AtlasMemory.SearchAsync(string, CancellationToken)"/> throws when the
    /// cancellation token has already been cancelled.
    /// </summary>
    [Fact]
    public async Task SearchAsync_Should_Throw_WhenCancellationRequested()
    {
        var memory = new AtlasMemory();

        using var cancellationTokenSource = new CancellationTokenSource();
        await cancellationTokenSource.CancelAsync();

        await Assert.ThrowsAsync<OperationCanceledException>(
            () => memory.SearchAsync(
                "test",
                cancellationTokenSource.Token));
    }

    /// <summary>
    /// Verifies that <see cref="AtlasMemory.SearchAsync(string, CancellationToken)"/> matches memories
    /// containing all terms in a multi-word query.
    /// </summary>
    [Fact]
    public async Task SearchAsync_Should_MatchAllTermsInQuery()
    {
        var memory = new AtlasMemory();

        var matchingEntry = new AtlasMemoryEntry
        {
            Id = Guid.NewGuid(),
            Content = "I bought a Canon camera recently",
            CreatedAt = DateTimeOffset.UtcNow
        };

        var partialMatchEntry = new AtlasMemoryEntry
        {
            Id = Guid.NewGuid(),
            Content = "I bought a Canon camera",
            CreatedAt = DateTimeOffset.UtcNow
        };

        await memory.StoreAsync(
            matchingEntry,
            TestContext.Current.CancellationToken);

        await memory.StoreAsync(
            partialMatchEntry,
            TestContext.Current.CancellationToken);

        var result = await memory.SearchAsync(
            "camera recently",
            TestContext.Current.CancellationToken);

        var entry = Assert.Single(result);

        Assert.Equal(
            matchingEntry,
            entry);
    }

    /// <summary>
    /// Verifies that <see cref="AtlasMemory.SearchAsync(string, CancellationToken)"/> ignores repeated
    /// whitespace when processing a multi-word query.
    /// </summary>
    [Fact]
    public async Task SearchAsync_Should_IgnoreRepeatedWhitespaceInQuery()
    {
        var memory = new AtlasMemory();

        var entry = new AtlasMemoryEntry
        {
            Id = Guid.NewGuid(),
            Content = "I bought a Canon camera recently",
            CreatedAt = DateTimeOffset.UtcNow
        };

        await memory.StoreAsync(
            entry,
            TestContext.Current.CancellationToken);

        var result = await memory.SearchAsync(
            "camera    recently",
            TestContext.Current.CancellationToken);

        var matchedEntry = Assert.Single(result);

        Assert.Equal(
            entry,
            matchedEntry);
    }

    /// <summary>
    /// Verifies that <see cref="AtlasMemory.SearchAsync(string, CancellationToken)"/> returns matching
    /// memories with the most recently created memory first.
    /// </summary>
    [Fact]
    public async Task SearchAsync_Should_ReturnNewestMemoriesFirst()
    {
        var memory = new AtlasMemory();

        var olderEntry = new AtlasMemoryEntry
        {
            Id = Guid.NewGuid(),
            Content = "I bought a Canon camera",
            CreatedAt = DateTimeOffset.UtcNow.AddMinutes(-10)
        };

        var newerEntry = new AtlasMemoryEntry
        {
            Id = Guid.NewGuid(),
            Content = "I still have my Canon camera",
            CreatedAt = DateTimeOffset.UtcNow
        };

        await memory.StoreAsync(
            olderEntry,
            TestContext.Current.CancellationToken);

        await memory.StoreAsync(
            newerEntry,
            TestContext.Current.CancellationToken);

        var result = await memory.SearchAsync(
            "Canon camera",
            TestContext.Current.CancellationToken);

        Assert.Equal(
            [newerEntry, olderEntry],
            result);
    }

    /// <summary>
    /// Verifies that an exact content match is ranked before partial matches.
    /// </summary>
    [Fact]
    public async Task SearchAsync_Should_RankExactMatchFirst()
    {
        var memory = new AtlasMemory();

        var partialMatch = new AtlasMemoryEntry
        {
            Id = Guid.NewGuid(),
            Content = "I bought a Canon camera.",
            CreatedAt = DateTimeOffset.UtcNow
        };

        var exactMatch = new AtlasMemoryEntry
        {
            Id = Guid.NewGuid(),
            Content = "camera",
            CreatedAt = DateTimeOffset.UtcNow.AddSeconds(1)
        };

        await memory.StoreAsync(
            partialMatch,
            TestContext.Current.CancellationToken);

        await memory.StoreAsync(
            exactMatch,
            TestContext.Current.CancellationToken);

        var result = await memory.SearchAsync(
            "camera",
            TestContext.Current.CancellationToken);

        Assert.Equal(
            exactMatch,
            result[0]);

        Assert.Equal(
            partialMatch,
            result[1]);
    }

    /// <summary>
    /// Verifies that memories containing the query more frequently are ranked higher.
    /// </summary>
    [Fact]
    public async Task SearchAsync_Should_RankMoreRelevantMemoryFirst()
    {
        var memory = new AtlasMemory();

        var lessRelevant = new AtlasMemoryEntry
        {
            Id = Guid.NewGuid(),
            Content = "I bought a camera.",
            CreatedAt = DateTimeOffset.UtcNow
        };

        var moreRelevant = new AtlasMemoryEntry
        {
            Id = Guid.NewGuid(),
            Content = "I bought a camera because I wanted a camera for photography.",
            CreatedAt = DateTimeOffset.UtcNow.AddSeconds(1)
        };

        await memory.StoreAsync(
            lessRelevant,
            TestContext.Current.CancellationToken);

        await memory.StoreAsync(
            moreRelevant,
            TestContext.Current.CancellationToken);

        var result = await memory.SearchAsync(
            "camera",
            TestContext.Current.CancellationToken);

        Assert.Equal(
            moreRelevant,
            result[0]);

        Assert.Equal(
            lessRelevant,
            result[1]);
    }

    /// <summary>
    /// Verifies that the <see cref="AtlasMemoryEntry.Type"/> property
    /// defaults to <see cref="AtlasMemoryType.Fact"/>.
    /// </summary>
    [Fact]
    public void Type_Should_DefaultToFact()
    {
        var entry = new AtlasMemoryEntry
        {
            Id = Guid.NewGuid(),
            Content = "I bought a Canon EOS 350D.",
            CreatedAt = DateTimeOffset.UtcNow
        };

        Assert.Equal(
            AtlasMemoryType.Fact,
            entry.Type);
    }

    /// <summary>
    /// Verifies that the <see cref="AtlasMemoryEntry.Type"/> property supports
    /// all defined memory types in <see cref="AtlasMemoryType"/>.
    /// </summary>
    /// <param name="type">The memory type to test.</param>
    [Theory]
    [InlineData(AtlasMemoryType.Fact)]
    [InlineData(AtlasMemoryType.Preference)]
    [InlineData(AtlasMemoryType.Task)]
    [InlineData(AtlasMemoryType.Conversation)]
    public void Type_Should_SupportAllMemoryTypes(
        AtlasMemoryType type)
    {
        var entry = new AtlasMemoryEntry
        {
            Id = Guid.NewGuid(),
            Content = "Test memory",
            CreatedAt = DateTimeOffset.UtcNow,
            Type = type
        };

        Assert.Equal(
            type,
            entry.Type);
    }

    /// <summary>
    /// Verifies that a memory query can filter memories by type.
    /// </summary>
    [Fact]
    public async Task SearchAsync_Should_FilterByMemoryType()
    {
        var memory = new AtlasMemory();

        await memory.StoreAsync(
            new AtlasMemoryEntry
            {
                Id = Guid.NewGuid(),
                Content = "Buy milk.",
                CreatedAt = DateTimeOffset.UtcNow,
                Type = AtlasMemoryType.Task
            },
            TestContext.Current.CancellationToken);

        await memory.StoreAsync(
            new AtlasMemoryEntry
            {
                Id = Guid.NewGuid(),
                Content = "My favorite food is pizza.",
                CreatedAt = DateTimeOffset.UtcNow,
                Type = AtlasMemoryType.Preference
            },
            TestContext.Current.CancellationToken);

        await memory.StoreAsync(
            new AtlasMemoryEntry
            {
                Id = Guid.NewGuid(),
                Content = "I bought a Canon EOS 350D.",
                CreatedAt = DateTimeOffset.UtcNow,
                Type = AtlasMemoryType.Fact
            },
            TestContext.Current.CancellationToken);

        var results = await memory.SearchAsync(
            new AtlasMemoryQuery
            {
                Type = AtlasMemoryType.Task
            },
            TestContext.Current.CancellationToken);

        var entry = Assert.Single(results);

        Assert.Equal(
            "Buy milk.",
            entry.Content);

        Assert.Equal(
            AtlasMemoryType.Task,
            entry.Type);
    }

    /// <summary>
    /// Verifies that a memory query can filter memories by both text and type.
    /// </summary>
    [Fact]
    public async Task SearchAsync_Should_FilterByTextAndMemoryType()
    {
        var memory = new AtlasMemory();

        await memory.StoreAsync(
            new AtlasMemoryEntry
            {
                Id = Guid.NewGuid(),
                Content = "Buy milk.",
                CreatedAt = DateTimeOffset.UtcNow,
                Type = AtlasMemoryType.Task
            },
            TestContext.Current.CancellationToken);

        await memory.StoreAsync(
            new AtlasMemoryEntry
            {
                Id = Guid.NewGuid(),
                Content = "Buy milk for the weekend.",
                CreatedAt = DateTimeOffset.UtcNow,
                Type = AtlasMemoryType.Fact
            },
            TestContext.Current.CancellationToken);

        await memory.StoreAsync(
            new AtlasMemoryEntry
            {
                Id = Guid.NewGuid(),
                Content = "Buy bread.",
                CreatedAt = DateTimeOffset.UtcNow,
                Type = AtlasMemoryType.Task
            },
            TestContext.Current.CancellationToken);

        var results = await memory.SearchAsync(
            new AtlasMemoryQuery
            {
                Text = "milk",
                Type = AtlasMemoryType.Task
            },
            TestContext.Current.CancellationToken);

        var entry = Assert.Single(results);

        Assert.Equal(
            "Buy milk.",
            entry.Content);

        Assert.Equal(
            AtlasMemoryType.Task,
            entry.Type);
    }

    /// <summary>
    /// Verifies that a memory query without filters returns all memories.
    /// </summary>
    [Fact]
    public async Task SearchAsync_Should_ReturnAllMemories_WhenQueryHasNoFilters()
    {
        var memory = new AtlasMemory();

        await memory.StoreAsync(
            new AtlasMemoryEntry
            {
                Id = Guid.NewGuid(),
                Content = "Buy milk.",
                CreatedAt = DateTimeOffset.UtcNow,
                Type = AtlasMemoryType.Task
            },
            TestContext.Current.CancellationToken);

        await memory.StoreAsync(
            new AtlasMemoryEntry
            {
                Id = Guid.NewGuid(),
                Content = "My favorite food is pizza.",
                CreatedAt = DateTimeOffset.UtcNow,
                Type = AtlasMemoryType.Preference
            },
            TestContext.Current.CancellationToken);

        var results = await memory.SearchAsync(
            new AtlasMemoryQuery(),
            TestContext.Current.CancellationToken);

        Assert.Equal(
            2,
            results.Count);

        Assert.Contains(
            results,
            memoryEntry => memoryEntry.Content == "Buy milk.");

        Assert.Contains(
            results,
            memoryEntry => memoryEntry.Content ==
                           "My favorite food is pizza.");
    }

    /// <summary>
    /// Verifies that a text query only matches memories containing all query terms.
    /// </summary>
    [Fact]
    public async Task SearchAsync_Should_ExcludeMemoriesMissingQueryTerms()
    {
        var memory = new AtlasMemory();

        var completeMatch = new AtlasMemoryEntry
        {
            Id = Guid.NewGuid(),
            Content = "I bought a Canon camera recently",
            CreatedAt = DateTimeOffset.UtcNow
        };

        var missingTerm = new AtlasMemoryEntry
        {
            Id = Guid.NewGuid(),
            Content = "I bought a Canon camera",
            CreatedAt = DateTimeOffset.UtcNow
        };

        var missingOtherTerm = new AtlasMemoryEntry
        {
            Id = Guid.NewGuid(),
            Content = "I recently bought a laptop",
            CreatedAt = DateTimeOffset.UtcNow
        };

        await memory.StoreAsync(
            completeMatch,
            TestContext.Current.CancellationToken);

        await memory.StoreAsync(
            missingTerm,
            TestContext.Current.CancellationToken);

        await memory.StoreAsync(
            missingOtherTerm,
            TestContext.Current.CancellationToken);

        var results = await memory.SearchAsync(
            "camera recently",
            TestContext.Current.CancellationToken);

        var result = Assert.Single(results);

        Assert.Equal(
            completeMatch,
            result);
    }

    /// <summary>
    /// Verifies that duplicate query terms do not artificially increase relevance.
    /// </summary>
    [Fact]
    public async Task SearchAsync_Should_IgnoreDuplicateQueryTerms()
    {
        var memory = new AtlasMemory();

        var cameraMemory = new AtlasMemoryEntry
        {
            Id = Guid.NewGuid(),
            Content = "I bought a camera",
            CreatedAt = DateTimeOffset.UtcNow
        };

        var cameraPhotographyMemory = new AtlasMemoryEntry
        {
            Id = Guid.NewGuid(),
            Content = "I bought a camera for photography",
            CreatedAt = DateTimeOffset.UtcNow.AddSeconds(1)
        };

        await memory.StoreAsync(
            cameraMemory,
            TestContext.Current.CancellationToken);

        await memory.StoreAsync(
            cameraPhotographyMemory,
            TestContext.Current.CancellationToken);

        var results = await memory.SearchAsync(
            "camera camera",
            TestContext.Current.CancellationToken);

        Assert.Equal(
            2,
            results.Count);

        Assert.Equal(
            cameraPhotographyMemory,
            results[0]);

        Assert.Equal(
            cameraMemory,
            results[1]);
    }

    /// <summary>
    /// Verifies that multi-term searches are case-insensitive.
    /// </summary>
    [Fact]
    public async Task SearchAsync_Should_BeCaseInsensitive_ForMultipleTerms()
    {
        var memory = new AtlasMemory();

        var entry = new AtlasMemoryEntry
        {
            Id = Guid.NewGuid(),
            Content = "I bought a Canon camera recently",
            CreatedAt = DateTimeOffset.UtcNow
        };

        await memory.StoreAsync(
            entry,
            TestContext.Current.CancellationToken);

        var results = await memory.SearchAsync(
            "CANON CAMERA",
            TestContext.Current.CancellationToken);

        var result = Assert.Single(results);

        Assert.Equal(
            entry,
            result);
    }

    /// <summary>
    /// Verifies that a multi-term query returns no memories when none contain all terms.
    /// </summary>
    [Fact]
    public async Task SearchAsync_Should_ReturnEmpty_WhenNoMemoryContainsAllQueryTerms()
    {
        var memory = new AtlasMemory();

        await memory.StoreAsync(
            new AtlasMemoryEntry
            {
                Id = Guid.NewGuid(),
                Content = "I bought a Canon camera",
                CreatedAt = DateTimeOffset.UtcNow
            },
            TestContext.Current.CancellationToken);

        await memory.StoreAsync(
            new AtlasMemoryEntry
            {
                Id = Guid.NewGuid(),
                Content = "I recently bought a laptop",
                CreatedAt = DateTimeOffset.UtcNow
            },
            TestContext.Current.CancellationToken);

        var results = await memory.SearchAsync(
            "Canon laptop",
            TestContext.Current.CancellationToken);

        Assert.Empty(results);
    }

    /// <summary>
    /// Verifies that memory type filtering excludes otherwise matching memories of another type.
    /// </summary>
    [Fact]
    public async Task SearchAsync_Should_ExcludeMatchingMemoriesOfWrongType()
    {
        var memory = new AtlasMemory();

        var taskMemory = new AtlasMemoryEntry
        {
            Id = Guid.NewGuid(),
            Content = "Buy a camera",
            CreatedAt = DateTimeOffset.UtcNow,
            Type = AtlasMemoryType.Task
        };

        var factMemory = new AtlasMemoryEntry
        {
            Id = Guid.NewGuid(),
            Content = "I bought a camera",
            CreatedAt = DateTimeOffset.UtcNow,
            Type = AtlasMemoryType.Fact
        };

        await memory.StoreAsync(
            taskMemory,
            TestContext.Current.CancellationToken);

        await memory.StoreAsync(
            factMemory,
            TestContext.Current.CancellationToken);

        var results = await memory.SearchAsync(
            new AtlasMemoryQuery
            {
                Text = "camera",
                Type = AtlasMemoryType.Fact
            },
            TestContext.Current.CancellationToken);

        var result = Assert.Single(results);

        Assert.Equal(
            factMemory,
            result);
    }

    /// <summary>
    /// Verifies that an unrestricted memory query returns memories newest first.
    /// </summary>
    [Fact]
    public async Task SearchAsync_Should_ReturnNewestMemoriesFirst_WhenQueryHasNoFilters()
    {
        var memory = new AtlasMemory();

        var olderMemory = new AtlasMemoryEntry
        {
            Id = Guid.NewGuid(),
            Content = "Older memory",
            CreatedAt = DateTimeOffset.UtcNow.AddMinutes(-10)
        };

        var newerMemory = new AtlasMemoryEntry
        {
            Id = Guid.NewGuid(),
            Content = "Newer memory",
            CreatedAt = DateTimeOffset.UtcNow
        };

        await memory.StoreAsync(
            olderMemory,
            TestContext.Current.CancellationToken);

        await memory.StoreAsync(
            newerMemory,
            TestContext.Current.CancellationToken);

        var results = await memory.SearchAsync(
            new AtlasMemoryQuery(),
            TestContext.Current.CancellationToken);

        Assert.Equal(
            [newerMemory, olderMemory],
            results);
    }

    /// <summary>
    /// Verifies that an exact multi-word query match receives a higher relevance
    /// score than a memory containing the same terms separately.
    /// </summary>
    [Fact]
    public async Task SearchAsync_Should_RankExactPhraseMatchHigher()
    {
        var memory = new AtlasMemory();

        var exactPhrase = new AtlasMemoryEntry
        {
            Id = Guid.NewGuid(),
            Content = "I use a Canon camera.",
            CreatedAt = DateTimeOffset.UtcNow.AddMinutes(-1),
            Type = AtlasMemoryType.Fact
        };

        var separateTerms = new AtlasMemoryEntry
        {
            Id = Guid.NewGuid(),
            Content = "My Canon lens works with every camera.",
            CreatedAt = DateTimeOffset.UtcNow,
            Type = AtlasMemoryType.Fact
        };

        await memory.StoreAsync(
            exactPhrase,
            TestContext.Current.CancellationToken);

        await memory.StoreAsync(
            separateTerms,
            TestContext.Current.CancellationToken);

        var results = await memory.SearchAsync(
            "Canon camera",
            TestContext.Current.CancellationToken);

        Assert.Equal(
            exactPhrase.Id,
            results[0].Id);
    }

    /// <summary>
    /// Verifies that a memory containing a search term multiple times receives
    /// a higher relevance score.
    /// </summary>
    [Fact]
    public async Task SearchAsync_Should_RankMultipleOccurrencesHigher()
    {
        var memory = new AtlasMemory();

        var repeated = new AtlasMemoryEntry
        {
            Id = Guid.NewGuid(),
            Content = "I like cameras. My camera is a Canon camera.",
            CreatedAt = DateTimeOffset.UtcNow.AddMinutes(-1),
            Type = AtlasMemoryType.Fact
        };

        var single = new AtlasMemoryEntry
        {
            Id = Guid.NewGuid(),
            Content = "I bought a Canon camera.",
            CreatedAt = DateTimeOffset.UtcNow,
            Type = AtlasMemoryType.Fact
        };

        await memory.StoreAsync(
            repeated,
            TestContext.Current.CancellationToken);

        await memory.StoreAsync(
            single,
            TestContext.Current.CancellationToken);

        var results = await memory.SearchAsync(
            "camera",
            TestContext.Current.CancellationToken);

        Assert.Equal(
            repeated.Id,
            results[0].Id);
    }

    /// <summary>
    /// Verifies that a memory must contain every search term to be returned.
    /// </summary>
    [Fact]
    public async Task SearchAsync_Should_RequireAllQueryTerms()
    {
        var memory = new AtlasMemory();

        await memory.StoreAsync(
            new AtlasMemoryEntry
            {
                Id = Guid.NewGuid(),
                Content = "I bought a Canon camera.",
                CreatedAt = DateTimeOffset.UtcNow,
                Type = AtlasMemoryType.Fact
            },
            TestContext.Current.CancellationToken);

        await memory.StoreAsync(
            new AtlasMemoryEntry
            {
                Id = Guid.NewGuid(),
                Content = "I bought a Canon lens.",
                CreatedAt = DateTimeOffset.UtcNow,
                Type = AtlasMemoryType.Fact
            },
            TestContext.Current.CancellationToken);

        var results = await memory.SearchAsync(
            "Canon camera",
            TestContext.Current.CancellationToken);

        Assert.Single(results);
        Assert.Contains(
            "camera",
            results[0].Content,
            StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Verifies that type filtering is applied before relevance ordering.
    /// </summary>
    [Fact]
    public async Task SearchAsync_Should_FilterByTypeBeforeRankingResults()
    {
        var memory = new AtlasMemory();

        var fact = new AtlasMemoryEntry
        {
            Id = Guid.NewGuid(),
            Content = "I own a Canon camera.",
            CreatedAt = DateTimeOffset.UtcNow.AddMinutes(-1),
            Type = AtlasMemoryType.Fact
        };

        var preference = new AtlasMemoryEntry
        {
            Id = Guid.NewGuid(),
            Content = "I really like my Canon camera.",
            CreatedAt = DateTimeOffset.UtcNow,
            Type = AtlasMemoryType.Preference
        };

        await memory.StoreAsync(
            fact,
            TestContext.Current.CancellationToken);

        await memory.StoreAsync(
            preference,
            TestContext.Current.CancellationToken);

        var results = await memory.SearchAsync(
            new AtlasMemoryQuery
            {
                Text = "Canon camera",
                Type = AtlasMemoryType.Fact
            },
            TestContext.Current.CancellationToken);

        Assert.Single(results);
        Assert.Equal(
            fact.Id,
            results[0].Id);
    }

    /// <summary>
    /// Verifies that an empty query returns memories ordered by creation time.
    /// </summary>
    [Fact]
    public async Task SearchAsync_Should_ReturnNewestMemoriesFirst_WhenQueryIsEmpty()
    {
        var memory = new AtlasMemory();

        var older = new AtlasMemoryEntry
        {
            Id = Guid.NewGuid(),
            Content = "Older memory",
            CreatedAt = DateTimeOffset.UtcNow.AddHours(-1),
            Type = AtlasMemoryType.Fact
        };

        var newer = new AtlasMemoryEntry
        {
            Id = Guid.NewGuid(),
            Content = "Newer memory",
            CreatedAt = DateTimeOffset.UtcNow,
            Type = AtlasMemoryType.Fact
        };

        await memory.StoreAsync(
            older,
            TestContext.Current.CancellationToken);

        await memory.StoreAsync(
            newer,
            TestContext.Current.CancellationToken);

        var results = await memory.SearchAsync(
            new AtlasMemoryQuery(),
            TestContext.Current.CancellationToken);

        Assert.Equal(
            [newer.Id, older.Id],
            results.Select(memoryEntry => memoryEntry.Id));
    }

    private static AtlasMemoryEntry CreateMemory()
    {
        return new AtlasMemoryEntry
        {
            Id = Guid.NewGuid(),
            Content = "Test memory",
            CreatedAt = DateTimeOffset.UtcNow
        };
    }
}