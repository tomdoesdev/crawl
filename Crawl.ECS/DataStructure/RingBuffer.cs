namespace Crawl.ECS.DataStructure;

public class RingBuffer<T>(int capacity)
{
    private readonly T[] _buffer = new T[capacity];
    private int _head, _tail, _count;

    public int Capacity => capacity;

    public void Push(T item)
    {
        _buffer[_head] = item;
        _head = (_head + 1) % _buffer.Length;
        if (_count < _buffer.Length) _count++;
        else _tail = (_tail + 1) % _buffer.Length;
    }

    public T Pop()
    {
        var item = _buffer[_tail];
        _tail = (_tail + 1) % _buffer.Length;
        _count--;
        return item;
    }

    public T Peek()
    {
        var item = _buffer[_tail];
        return item;
    }
}