# Memoization
## What is Memoization?

Memoization is an optimization technique that **caches the results of expensive function calls** and returns the cached
result when the same inputs occur again. It's essentially a form of dynamic programming that trades memory for
computational speed.

When you memoize a function:

1. **First call**: The function executes normally and stores both the input parameters as a cache key and the result
2. **Subsequent calls**: If the same input is provided, the cached result is returned immediately without re-executing
   the function
3. **Different inputs**: New inputs trigger normal execution and create new cache entries

## Why Use Memoization?

Memoization provides several key benefits:

### 🚀 **Performance Improvement**

- **Eliminates redundant calculations**: Expensive operations are only performed once per unique input
- **Reduces response time**: Cached results are returned in O(1) time instead of the original function's complexity
- **Improves user experience**: Faster response times for repeated operations

### 💾 **Memory vs. CPU Trade-off**

- **Uses memory to save CPU cycles**: Stores results in memory to avoid recalculation
- **Predictable memory usage**: Cache size is bounded by the number of unique inputs
- **Configurable limits**: Advanced options allow you to control memory usage

### 🔄 **Ideal for Pure Functions**

- **Deterministic results**: Same input always produces the same output
- **No side effects**: Function behavior doesn't change based on external state
- **Thread-safe caching**: Multiple threads can safely access cached results

## When to Use Memoization

Memoization is most effective in these scenarios:

### ✅ **Expensive Computations**

```c#
// Mathematical calculations 
Func<int, long> fibonacci = null;
fibonacci = n => 
{
    if (n <= 1)
        return n;
    return fibonacci(n - 1) + fibonacci(n - 2);
};

var memoizedFib = fibonacci.Memoize(); // Dramatically faster for repeated calls

// Complex business logic 
Func<User, decimal> calculateUserScore = user => { 
    // Expensive calculation involving database queries, external APIs, etc. 
    return PerformComplexScoring(user); 
}; 

var memoizedScoring = calculateUserScore.Memoize();
```

### ✅ **Recursive Algorithms**

```c#
// Before memoization: O(2^n) time complexity 
// After memoization: O(n) time complexity 
var memoizedFibonacci = fibonacci.Memoize(); 
var result = memoizedFibonacci(40); // Fast, even for large numbers
``` 

### ✅ **Validation and Business Rules**

```c#
// Expensive validation that might be called multiple times 
Func<string, Result > validateBusinessRule = input => { 
    // Complex validation involving database lookups, external service calls 
    return PerformExpensiveValidation(input);
};

var memoizedValidation = validateBusinessRule.MemoizeResult();
``` 

### ✅ **Data Transformation**

```c# 
// Expensive data parsing or transformation 
Func<string, ComplexObject> parseData = jsonString => { 
    // Complex parsing, validation, and object construction 
    return DeserializeAndValidate(jsonString); 
};
var memoizedParser = parseData.Memoize();
``` 

## When NOT to Use Memoization

Avoid memoization in these situations:

### ❌ **Functions with Side Effects**

```c# 
// DON'T memoize - function has side effects
Func<string, bool> sendEmail = message => { 
    emailService.Send(message); 
    // Side effect!
    return true; 
}; // This would prevent emails from being sent on subsequent calls
``` 

### ❌ **Non-Deterministic Functions**

```c# 
// DON'T memoize - results change over time 
Func<string, DateTime> getCurrentTime = _ 
    => DateTime.Now; 

Func<int, int> getRandomNumber = seed 
    => new Random().Next();
``` 

### ❌ **Functions with Large Input/Output**

```c# 
// DON'T memoize - memory usage could be excessive 
Func<byte[], byte[]> processLargeFile = fileData => { 
    // Processing large files would consume too much cache memory 
    return ProcessedData; 
};
``` 

### ❌ **Rarely Called Functions**

```c# 
// DON'T memoize - no benefit if only called once 
Func<string, string> oneTimeInitialization = config => {
    // Only called during application startup 
    return InitializeFromConfig(config); 
};
``` 

## Basic Usage

### Simple Function Memoization

```c# 
// Memoize a function with no parameters 
Func expensiveCalculation = () => { 
    // Simulate expensive operation 
    Thread. Sleep(1000); 
    return 42; 
};

var memoized = expensiveCalculation.Memoize(); 
var result1 = memoized(); // Takes 1 second 
var result2 = memoized(); // Returns immediately (cached)
``` 

### Single Parameter Memoization

```c#
// Memoize a function with one parameter 
Func<int, int> square = x => 
{
    Console.WriteLine($"Computing square of {x}"); 
    return x * x;
};
var memoizedSquare = square.Memoize();
var result1 = memoizedSquare(5); 
// Prints: "Computing square of 5", returns 25 
var result2 = memoizedSquare(5); 
// Returns 25 immediately (cached) 
var result3 = memoizedSquare(3); 
// Prints: "Computing square of 3", returns 9
``` 

### Multiple Parameters

```c# 
// Memoize functions with multiple parameters 
Func<int, int, int> add = (x, y) => 
{
    Console.WriteLine($"Adding {x} + {y}"); 
    return x + y;
};
var memoizedAdd = add.Memoize(); 
var result1 = memoizedAdd(2, 3); 
// Prints: "Adding 2 + 3", returns 5 
var result2 = memoizedAdd(2, 3); 
// Returns 5 immediately (cached) 
var result3 = memoizedAdd(3, 2); 
// Prints: "Adding 3 + 2", returns 5 (different order = different cache key)
``` 

## Advanced Features

### Cache Statistics and Management

```c# 
// Create a configurable memoized function with statistics 
Func<string, int> lengthFunction = s => { 
    Thread.Sleep(100); // Simulate processing time 
    return s.Length;
};

var memoizedFunction = MemoizationFactory.CreateConfigurable(lengthFunction);
// Use the function 
var result1 = memoizedFunction.Invoke("hello"); 
var result2 = memoizedFunction.Invoke("hello"); // Cache hit 
var result3 = memoizedFunction.Invoke("world"); // Cache miss
// Check statistics 
Console.WriteLine("Cache hits: {memoizedFunction.HitCount}"); // 1 
Console.WriteLine("Cache misses: {memoizedFunction.MissCount}"); // 2 
Console.WriteLine("Hit ratio: {memoizedFunction.HitRatio:F1}%"); // 33.3% 
Console.WriteLine("Cache size: {memoizedFunction.CacheSize}"); // 2
// Cache management 
memoizedFunction.ClearCache(); // Clear all cached items 
memoizedFunction.RemoveFromCache("hello"); // Remove specific item 
bool exists = memoizedFunction.ContainsKey("world"); // Check if key exists
``` 

### Cache with Expiration and Size Limits

```c# 
// Create memoized function with expiration and size limits 
var memoizedWithLimits = MemoizationFactory.CreateMemoized<string, int>( s => s.Length, 
    maxCacheSize: 100, // Maximum 100 cached items 
    expiration: TimeSpan.FromMinutes(5) // Items expire after 5 minutes 
);

var result = memoizedWithLimits("test");
``` 

### Integration with Result Types

```c# 
// Memoize functions that return Result types 
Func<User, Result > validateUser = user => {
    // Expensive validation logic
    return PerformValidation(user);
};

var memoizedValidation = validateUser.MemoizeResult();
// Or create a configurable version with statistics
var configurableValidation = validateUser.CreateMemoizedResult(); 
var result = configurableValidation.Invoke(user);
Console.WriteLine($"Validation cache hit ratio: {configurableValidation.HitRatio:F1}%");
``` 

## Performance Considerations

### Memory Usage

- **Cache grows with unique inputs**: Each unique input creates a cache entry
- **Monitor cache size**: Use `CacheSize` property to track memory usage
- **Set limits when needed**: Use expiration and size limits for long-running applications

### Thread Safety

- **Fully thread-safe**: All memoization implementations use thread-safe collections
- **Concurrent access**: Multiple threads can safely call memoized functions simultaneously
- **No locks on cache hits**: Reading cached values doesn't require locking

### When to Clear Cache

```c# 
var memoized = MemoizationFactory.CreateConfigurable(expensiveFunction);
// Clear cache when: 
// 1. Memory usage becomes too high 
if (memoized.CacheSize > 1000) { memoized.ClearCache(); }
// 2. Underlying data changes 
(for data-dependent functions) OnDataChanged += () => memoized.ClearCache();
// 3. Specific entries become invalid 
OnUserUpdated += (userId) => memoized.RemoveFromCache(userId);
``` 

### Performance Metrics Example

```c# 
// Measure performance improvement 
var stopwatch = Stopwatch.StartNew();
// Without memoization 
for (int i = 0; i < 1000; i++) { 
    var result = ExpensiveFunction(i % 10); // Only 10 unique inputs 
} 

var timeWithoutMemo = stopwatch.ElapsedMilliseconds;
stopwatch.Restart();

// With memoization 
var memoizedFunc = ExpensiveFunction.Memoize(); 
for (int i = 0; i < 1000; i++) {
    var result = memoizedFunc(i % 10); // Same 10 unique inputs 
} 

var timeWithMemo = stopwatch.ElapsedMilliseconds;
Console.WriteLine("Without memoization: {timeWithoutMemo}ms"); 
Console.WriteLine("With memoization: {timeWithMemo}ms");
Console.WriteLine($"Speedup: {timeWithoutMemo / (double)timeWithMemo:F1}x");
```