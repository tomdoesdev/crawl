namespace Crawl.DataStructures;

public class Pool<T>(uint maxCapacity = Pool<T>.DefaultMaxCapacity)
    where T : notnull
{
    private const uint DefaultMaxCapacity = 10000;
    private readonly HashSet<T> _items = [];

    public int Count => _items.Count;

    public T Pluck()
    {
        foreach (var item in _items)
        {
            _items.Remove(item);
            return item;
        }

        throw new InvalidOperationException("Pool is empty");
    }

    public bool TryPluck(out T value)
    {
        foreach (var item in _items)
        {
            _items.Remove(item);
            value = item;
            return true;
        }

        value = default!;
        return false;
    }

    public bool Add(T item)
    {
        return _items.Count < maxCapacity && _items.Add(item);
    }

    public bool Contains(T item) => _items.Contains(item);

    public void Clear() => _items.Clear();
}