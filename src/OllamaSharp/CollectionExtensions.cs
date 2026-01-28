namespace OllamaSharp;

/// <summary>
/// Provides extension methods for working with collections.
/// </summary>
internal static class CollectionExtensions
{
	/// <summary>
	/// Adds the elements of the specified collection to the end of the list if the collection is not <c>null</c>.
	/// </summary>
	/// <typeparam name="T">The type of elements in the list and collection.</typeparam>
	/// <param name="list">The list to which the elements should be added.</param>
	/// <param name="items">
	/// The collection whose elements should be added to the list.
	/// If <c>null</c>, no operations are performed.
	/// </param>
	/// <example>
	/// Example usage:
	/// <code>
	/// List&lt;int> myList = new List&lt;int> { 1, 2, 3 };
	/// IEnumerable&lt;int>? additionalItems = new List&lt;int> { 4, 5, 6 };
	/// myList.AddRangeIfNotNull(additionalItems);
	/// // myList now contains { 1, 2, 3, 4, 5, 6 }
	/// IEnumerable&lt;int>? nullItems = null;
	/// myList.AddRangeIfNotNull(nullItems);
	/// // myList remains unchanged { 1, 2, 3, 4, 5, 6 }
	/// </code>
	/// </example>
	public static void AddRangeIfNotNull<T>(this List<T> list, IEnumerable<T>? items)
	{
		if (items is not null)
			list.AddRange(items);
	}

	/// <summary>
	/// Executes the specified action for each item in the provided collection.
	/// </summary>
	/// <typeparam name="T">The type of the elements in the collection.</typeparam>
	/// <param name="collection">
	/// The enumerable collection whose elements the action will be performed upon.
	/// </param>
	/// <param name="action">
	/// An <see cref="Action{T}"/> delegate to perform on each element of the collection.
	/// </param>
	/// <example>
	/// Example usage:
	/// <code>
	/// List&lt;string> fruits = new List&lt;string> { "apple", "banana", "cherry" };
	/// fruits.ForEachItem(fruit => Console.WriteLine(fruit));
	/// // Output:
	/// // apple
	/// // banana
	/// // cherry
	/// IEnumerable&lt;int> numbers = new List&lt;int> { 1, 2, 3 };
	/// numbers.ForEachItem(number => Console.WriteLine(number * 2));
	/// // Output:
	/// // 2
	/// // 4
	/// // 6
	/// </code>
	/// </example>
	public static void ForEachItem<T>(this IEnumerable<T> collection, Action<T> action)
	{
		foreach (var item in collection)
			action(item);
	}
}