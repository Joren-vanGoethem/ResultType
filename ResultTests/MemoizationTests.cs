using JV.ResultUtilities;
using JV.ResultUtilities.Extensions;
using JV.ResultUtilities.Memoization;
using JV.ResultUtilities.Memoization.Extensions;
using JV.ResultUtilities.ValidationMessage;

namespace ResultTests;

/// <summary>
/// Tests for the memoization functionality in JV.ResultUtilities.
/// These tests demonstrate how memoization can be used to cache expensive operations
/// and improve performance by avoiding redundant calculations.
/// </summary>
public class MemoizationTests
{
    /// <summary>
    /// Tests that a memoized function with no parameters is only executed once.
    /// </summary>
    [Fact]
    public void Memoize_FunctionWithNoParameters_ExecutesOnlyOnce()
    {
        // Arrange
        var executionCount = 0;
        Func<int> expensiveFunction = () =>
        {
            executionCount++;
            Thread.Sleep(10); // Simulate expensive operation
            return 42;
        };

        var memoizedFunction = expensiveFunction.Memoize();

        // Act
        var result1 = memoizedFunction();
        var result2 = memoizedFunction();
        var result3 = memoizedFunction();

        // Assert
        Assert.Equal(42, result1);
        Assert.Equal(42, result2);
        Assert.Equal(42, result3);
        Assert.Equal(1, executionCount); // Function should only execute once
    }

    /// <summary>
    /// Tests that a memoized function with one parameter caches results correctly.
    /// </summary>
    [Fact]
    public void Memoize_FunctionWithOneParameter_CachesResultsCorrectly()
    {
        // Arrange
        var executionCounts = new Dictionary<int, int>();
        Func<int, int> expensiveFunction = x =>
        {
            executionCounts[x] = executionCounts.GetValueOrDefault(x, 0) + 1;
            Thread.Sleep(10); // Simulate expensive operation
            return x * x;
        };

        var memoizedFunction = expensiveFunction.Memoize();

        // Act
        var result1 = memoizedFunction(5);
        var result2 = memoizedFunction(5); // Same input, should use cache
        var result3 = memoizedFunction(3); // Different input, should execute
        var result4 = memoizedFunction(5); // Same as first, should use cache
        var result5 = memoizedFunction(3); // Same as third, should use cache

        // Assert
        Assert.Equal(25, result1);
        Assert.Equal(25, result2);
        Assert.Equal(9, result3);
        Assert.Equal(25, result4);
        Assert.Equal(9, result5);
        
        Assert.Equal(1, executionCounts[5]); // Should execute only once for input 5
        Assert.Equal(1, executionCounts[3]); // Should execute only once for input 3
    }

    /// <summary>
    /// Tests that a memoized function with two parameters works correctly.
    /// </summary>
    [Fact]
    public void Memoize_FunctionWithTwoParameters_WorksCorrectly()
    {
        // Arrange
        var executionCount = 0;
        Func<int, int, int> addFunction = (x, y) =>
        {
            executionCount++;
            Thread.Sleep(10); // Simulate expensive operation
            return x + y;
        };

        var memoizedFunction = addFunction.Memoize();

        // Act
        var result1 = memoizedFunction(2, 3);
        var result2 = memoizedFunction(2, 3); // Same inputs, should use cache
        var result3 = memoizedFunction(3, 2); // Different order, should execute again

        // Assert
        Assert.Equal(5, result1);
        Assert.Equal(5, result2);
        Assert.Equal(5, result3);
        Assert.Equal(2, executionCount); // Should execute twice (different parameter order)
    }

    /// <summary>
    /// Tests the configurable memoized function with statistics tracking.
    /// </summary>
    [Fact]
    public void ConfigurableMemoizedFunction_TracksStatisticsCorrectly()
    {
        // Arrange
        Func<string, int> lengthFunction = s => s.Length;
        var memoizedFunction = MemoizationFactory.CreateConfigurable(lengthFunction);

        // Act
        var result1 = memoizedFunction.Invoke("hello");
        var result2 = memoizedFunction.Invoke("hello"); // Cache hit
        var result3 = memoizedFunction.Invoke("world"); // Cache miss
        var result4 = memoizedFunction.Invoke("hello"); // Cache hit

        // Assert
        Assert.Equal(5, result1);
        Assert.Equal(5, result2);
        Assert.Equal(5, result3);
        Assert.Equal(5, result4);
        
        Assert.Equal(2, memoizedFunction.HitCount);
        Assert.Equal(2, memoizedFunction.MissCount);
        Assert.Equal(4, memoizedFunction.TotalAccesses);
        Assert.Equal(50.0, memoizedFunction.HitRatio); // 2 hits out of 4 accesses = 50%
        Assert.Equal(2, memoizedFunction.CacheSize); // "hello" and "world"
    }

    /// <summary>
    /// Tests cache management operations like clearing and removing items.
    /// </summary>
    [Fact]
    public void ConfigurableMemoizedFunction_CacheManagement_WorksCorrectly()
    {
        // Arrange
        Func<int, string> toStringFunction = i => i.ToString();
        var memoizedFunction = MemoizationFactory.CreateConfigurable(toStringFunction);

        // Act & Assert - Initial state
        memoizedFunction.Invoke(1);
        memoizedFunction.Invoke(2);
        Assert.Equal(2, memoizedFunction.CacheSize);
        Assert.True(memoizedFunction.ContainsKey(1));
        Assert.True(memoizedFunction.ContainsKey(2));

        // Remove specific item
        var removed = memoizedFunction.RemoveFromCache(1);
        Assert.True(removed);
        Assert.Equal(1, memoizedFunction.CacheSize);
        Assert.False(memoizedFunction.ContainsKey(1));
        Assert.True(memoizedFunction.ContainsKey(2));

        // Clear all cache
        memoizedFunction.ClearCache();
        Assert.Equal(0, memoizedFunction.CacheSize);
        Assert.Equal(0, memoizedFunction.HitCount);
        Assert.Equal(0, memoizedFunction.MissCount);
        Assert.False(memoizedFunction.ContainsKey(2));
    }

    [Fact]
    public void MemoizeResultWithKey_CachesResultByKey()
    {
        // Arrange
        var executionCount = 0;
        Func<string, Result<int>> parseFunction = input =>
        {
            executionCount++;
            return int.TryParse(input, out var val) ? Result.Ok(val) : Result.Error(
                ValidationKeyDefinition.Create("parse.error").WithStringParameter("input"), input);
        };

        // Act - memoize by first character as key
        var memoized = parseFunction.MemoizeResultWithKey(s => s[0]);

        var result1 = memoized("123");
        var result2 = memoized("1abc"); // Same key '1', should use cache
        var result3 = memoized("456"); // Different key '4', should execute

        // Assert
        Assert.True(result1.IsSuccessful);
        Assert.Equal(123, result1.Value);
        Assert.True(result2.IsSuccessful); // Cached from result1
        Assert.Equal(123, result2.Value);
        Assert.True(result3.IsSuccessful);
        Assert.Equal(456, result3.Value);
        Assert.Equal(2, executionCount); // Only 2 executions
    }

    [Fact]
    public void ExpiringMemoizedFunction_TracksStatistics()
    {
        // Arrange
        var expiring = MemoizationFactory.CreateConfigurable<int, string>(
            i => i.ToString(),
            maxCacheSize: 10,
            expiration: TimeSpan.FromMinutes(5));

        // Act
        expiring.Invoke(1);
        expiring.Invoke(1); // hit
        expiring.Invoke(2); // miss

        // Assert
        Assert.Equal(1, expiring.HitCount);
        Assert.Equal(2, expiring.MissCount);
        Assert.Equal(3, expiring.TotalAccesses);
        Assert.Equal(2, expiring.CacheSize);
    }

    [Fact]
    public async Task MemoizeAsync_CachesAsyncResults()
    {
        // Arrange
        var executionCount = 0;
        Func<int, Task<int>> asyncFunction = async key =>
        {
            Interlocked.Increment(ref executionCount);
            await Task.Delay(10);
            return key * key;
        };

        var memoized = asyncFunction.MemoizeAsync();

        // Act
        var result1 = await memoized(5);
        var result2 = await memoized(5); // Should use cache
        var result3 = await memoized(3); // Different key

        // Assert
        Assert.Equal(25, result1);
        Assert.Equal(25, result2);
        Assert.Equal(9, result3);
        Assert.Equal(2, executionCount);
    }

    [Fact]
    public async Task AsyncMemoizedFunction_TracksStatistics()
    {
        // Arrange
        var memoized = MemoizationFactory.CreateMemoizedAsync<string, int>(
            async key =>
            {
                await Task.Delay(1);
                return key.Length;
            });

        // Act
        await memoized.InvokeAsync("hello");
        await memoized.InvokeAsync("hello"); // hit
        await memoized.InvokeAsync("world"); // miss

        // Assert
        Assert.Equal(1, memoized.HitCount);
        Assert.Equal(2, memoized.MissCount);
        Assert.Equal(2, memoized.CacheSize);
    }

    /// <summary>
    /// Tests memoization applied to the User validation scenario from your existing code.
    /// This demonstrates a practical use case where validation results could be cached.
    /// </summary>
    [Fact]
    public void Memoization_WithUserValidation_ImprovesPerformance()
    {
        // Arrange
        var userService = new UserService();
        var validationCallCount = 0;
        
        // Create a wrapper that counts validation calls
        Func<User, Result<User>> validationFunction = user =>
        {
            validationCallCount++;
            return userService.ValidateUser(user);
        };

        // Memoize the validation function based on user properties
        // Note: In real scenarios, you'd want a proper key generation strategy
        var memoizedValidation = ((Func<string, Result<User>>)(userKey =>
        {
            // Parse user from key (simplified for demo)
            var parts = userKey.Split('|');
            var user = new User
            {
                Username = parts[0],
                Email = parts[1],
                Age = int.Parse(parts[2])
            };
            return validationFunction(user);
        })).Memoize();

        var userKey1 = "johndoe|john@example.com|25";
        var userKey2 = "janedoe|jane@example.com|30";

        // Act
        var result1 = memoizedValidation(userKey1);
        var result2 = memoizedValidation(userKey1); // Same user, should use cache
        var result3 = memoizedValidation(userKey2); // Different user, should validate
        var result4 = memoizedValidation(userKey1); // Same as first, should use cache

        // Assert
        Assert.True(result1.IsSuccessful);
        Assert.True(result2.IsSuccessful);
        Assert.True(result3.IsSuccessful);
        Assert.True(result4.IsSuccessful);
        
        // Validation should only be called twice: once for each unique user
        Assert.Equal(2, validationCallCount);
    }
}
