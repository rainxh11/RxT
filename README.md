# Rx\<T\>
.NET Reactive Types that provide similar functionalities inspired from: (`reactive`, `ref` & `computed`) found in **Vue.js**

### This library provides 4 types: `Reactive<T>`, `ConcurrentReactive<T>`, `Computed<T>`, `Computed<T,R>`
# Example Usage:
## `Reactive<T>`
a reactive wrapper object that track changes of the underlying object it is wrapping.
1. Create a reactive object:
```csharp
using RxT;

var searchQuery = new Reactive<string>("");
```
2. Change the state:
```csharp
searchQuery.Value = "search";
```
3. Track its state changes as an `IObservable<T>`
```csharp
searchQuery
    .Subscribe(x => Console.WriteLine($"Search Query: {x}"));
```
### There is also an additional `ConcurrentReactive<T>` a thread-safe version of `Reactive<T>`.
## `Computed<T>`
a read-only reactive object that track changes of a `Reactive<T>` object while applying a custom filter to its underlying `IObservable<T>`

1. Create a computed object from previous `searchQuery` with 500ms debouncing filter
```csharp
using RxT;

var searchQuery = new Reactive<string>("");
var searchQueryDebounced = searchQuery
                            .SpawnComputed(obs => obs.Throttle(500));
```
3. Track its state changes as an `IObservable<T>`
```csharp
searchQueryDebounced
    .Subscribe(x => Console.WriteLine($"Search Query Debounced: {x}"));
```

Both `Reactive<T>` & `Computed<T>` implements the `IObservable<T>` interface

## `Computed<T,R>`
same as `Computed<T>` but with different type for `Value`
```csharp
using RxT;

var searchQuery = new Reactive<string>("");
var searchLength = searchQuery
    .SpawnComputed(
         // Transform function to apply each time state changes
        query => query.Length,
        obs => obs.DistinctUntilChanged());
```

## Changing values of nested properties:
changing `Reactive<T>.Value` value directly will only trigger state change if `T` is a primitive type or `string`, nested properties will not trigger any state change.

### Solution:
use `Reactive<T>.Modify()` method
```csharp
using RxT;

var pagination = new Reactive<Pagination>(new Pagination()
{
    Page = 1,
    PageSize = 25,
    SortBy = "+createdAt"
});

pagination.Modify((p, triggerChange) =>
{
    p.Page = 2;
    triggerChange();
});


record Pagination
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public string SortBy { get; set; }
}
```
you can also omit `triggerChange()` and state change will be triggered automatically
```csharp
pagination.Modify(p => p.Page = 2);
```
same thing can be done to `Computed<T>`, keep in mind it will change the source `Reactive<T>.Value` as it is passed by reference
```csharp
paginationComputed = pagination.SpawnComputed(obs => obs)
paginationComputed.ModifySource(p => p.Page = 2);
```