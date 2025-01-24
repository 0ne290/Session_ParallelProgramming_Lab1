namespace ParallelProgrammingLab1.PetriNet;

public class Place
{
    public Place(int tokens = 0)
    {
        if (tokens < 0)
            throw new Exception("Кол-во фишек в позиции не может быть меньше 0. Была попытка создать такую позицию.");
        
        _tokens = tokens;
    }

    public override string ToString() => _tokens.ToString();

    public void Load() => Interlocked.Increment(ref _tokens);
    
    public void Unload()
    {
        lock (_locker)
        {
            if (_tokens < 1)
                throw new Exception("Кол-во фишек в позиции не может быть меньше 0. Возможно, была попытка выполнить какой-то недоступный переход.");

            _tokens--;
        }
    }
    
    public bool IsAvailable() => _tokens > 0;
    
    private int _tokens;

    private readonly object _locker = new();
}