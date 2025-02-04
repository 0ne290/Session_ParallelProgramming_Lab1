namespace ParallelProgrammingLab1.PetriNet;

public class Semaphore
{
    public Semaphore(string name, int capacity)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            name = $"R{_semaphoreCounter}";
            _semaphoreCounter++;
        }
        
        if (Semaphores.Any(r => r.Name == name))
            throw new Exception($"Семафор с именем {name} уже существует.");
        
        Name = name;
        _semaphorePlace = new Place(capacity);
        Semaphores.Add(this);
    }
    
    //public override string ToString()
    //{
    //    var res = "Semaphore " + _name + Environment.NewLine;
//
    //    foreach (var holdingTransition in _holdingTransitions)
    //        res += "Holding Transition For Thread " + holdingTransition.Key.State + " " + holdingTransition.Key.Name + ": " + holdingTransition.Value + Environment.NewLine;
    //    
    //    foreach (var releasingTransition in _releasingTransitions)
    //        res += "Releasing Transition For Thread " + releasingTransition.Key.State + " " + releasingTransition.Key.Name + ": " + releasingTransition.Value + Environment.NewLine;
//
    //    return res;
    //}
    
    public override string ToString() => $"{string.Join(" ", _namesOfHoldingThreads)};";

    public bool IsAvailable(MyThread thread)
    {
        lock (_locker)
        {
            return _holdingTransitions[thread].IsAvailable();
        }
    }

    public void Hold(MyThread thread)
    {
        lock (_locker)
        {
            while (!_holdingTransitions[thread].IsAvailable())
                Thread.Yield();
            
            _namesOfHoldingThreads.Add(thread.Name);
            
            _holdingTransitions[thread].Execute();
        }
    }

    public void Release(MyThread thread)
    {
        lock (_locker1)
        {
            _namesOfHoldingThreads.Remove(thread.Name);
            
            _releasingTransitions[thread].Execute();
        }
    }
    
    public bool TryAddUser(MyThread thread)
    {
        lock (_locker)
        {
            if (_holdingTransitions.ContainsKey(thread))
                return false;

            var holdingPlace = new Place(1);
            var releasingPlace = new Place();

            _holdingTransitions.Add(thread,
                new Transition(new[] { holdingPlace, _semaphorePlace }, new[] { releasingPlace }));
            _releasingTransitions.Add(thread,
                new Transition(new[] { releasingPlace }, new[] { holdingPlace, _semaphorePlace }));

            return true;
        }
    }

    public static Semaphore GetByName(string name) => Semaphores.Find(s => s.Name == name) ?? throw new Exception($"Ресурса с именем \"{name}\" нет.");

    public string Name { get; }

    private readonly Place _semaphorePlace;
    
    private readonly Dictionary<MyThread, Transition> _holdingTransitions = new();
    
    private readonly Dictionary<MyThread, Transition> _releasingTransitions = new();
    
    private readonly object _locker = new();
    
    private readonly object _locker1 = new();
    
    private readonly List<string> _namesOfHoldingThreads = new();
    
    private static int _semaphoreCounter = 1;
    
    public static List<Semaphore> Semaphores { get; } = new ();
}