namespace Crawl.ECS;

public class DuplicateComponentException : Exception
{
    public DuplicateComponentException() : base() { }

    public DuplicateComponentException(string message) : base(message) { }

    public DuplicateComponentException(string message, Exception innerException)
        : base(message, innerException) { }
}

public class ComponentNotFoundException : Exception
{
    public ComponentNotFoundException() : base() { }

    public ComponentNotFoundException(string message) : base(message) { }

    public ComponentNotFoundException(string message, Exception innerException)
        : base(message, innerException) { }
}


public class SparseSet<T> where T : struct, IComponent
{
    private T[] _components;
    private Entity[] _entities;
    private readonly Dictionary<uint,int> _sparseEntities; //Entity.Id -> Component array index
    private int _count;
    private int _capacity;
    private readonly int _growCapacity;

    public SparseSet(int initialCapacity = 1000, int growCapacity = 1000)
    {
        if (initialCapacity <= 0)
        {
            throw new ArgumentException("initial capacity must be greater than 0");
        }

        if (growCapacity <= 0)
        {
            throw new ArgumentException("grow capacity must be greater than 0");
        }
        
        _components = new T[initialCapacity];
        _entities = new Entity[initialCapacity];
        _sparseEntities = new Dictionary<uint, int>();
        _count = 0;
        _capacity = initialCapacity;
        _growCapacity = growCapacity;
    }

    public void Add(Entity entity, T value)
    {
        if (_count >= _capacity)
        {
            _capacity += _growCapacity;
            
            Array.Resize(ref _components, _capacity);
            Array.Resize(ref _entities, _capacity);
        }

        if (_sparseEntities.ContainsKey(entity.Id))
            throw new DuplicateComponentException($"Entity {entity.Id} already has component {typeof(T).Name}");

        
        _components[_count] = value;
        _entities[_count] = entity;
        _sparseEntities.Add(entity.Id,_count);
        
        _count++; // Move pointer to next slot
    }

    public bool TryAdd(Entity entity, T value)
    {
        if (_count >= _capacity)
        {
            _capacity += _growCapacity;
            
            Array.Resize(ref _components, _capacity);
            Array.Resize(ref _entities, _capacity);
        }

        if (_sparseEntities.ContainsKey(entity.Id))
            return false;

        
        _components[_count] = value;
        _entities[_count] = entity;
        _sparseEntities.Add(entity.Id,_count);
        
        _count++; // Move pointer to next slot
        return true;
    } 

    public void Remove(Entity entity)
    {
        if (!_sparseEntities.TryGetValue(entity.Id, out var deletingIdx) || _count <= 0)
        {
            return;
        }

        var lastIndex = _count - 1;

        if (lastIndex != deletingIdx)
        {
            // (x,y) = (y,x). Swap deleting entity with 'tail'.
            (_entities[deletingIdx], _entities[lastIndex]) = (_entities[lastIndex], _entities[deletingIdx]);
        
            // swap deleting component with tail
            (_components[deletingIdx], _components[lastIndex]) =
                (_components[lastIndex], _components[deletingIdx]);
            
            var movedEntity = _entities[deletingIdx];
            _sparseEntities[movedEntity.Id] = deletingIdx;
        }

        _sparseEntities.Remove(entity.Id);
        _count--;
    }

    public void Clear()
    {
        _count = 0;
        _sparseEntities.Clear();
    }
    

    public bool Has(Entity entity)
    {
        return _sparseEntities.ContainsKey(entity.Id);
    }
    
    public ReadOnlySpan<T> GetAll()
    {
        return new ReadOnlySpan<T>(_components, 0, _count);
    }

    public T Get(Entity entity)
    {
        return !_sparseEntities.TryGetValue(entity.Id, out var componentIndex) ? 
            throw new ComponentNotFoundException($"Entity {entity.Id} does not exist") 
            : _components[componentIndex];
    }
    
    public bool TryGet(Entity entity, out T component)
    {
        if (_sparseEntities.TryGetValue(entity.Id, out var index))
        {
            component = _components[index];
            return true;
        }
        component = default;
        return false;
    }
}