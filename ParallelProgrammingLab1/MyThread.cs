using System.Diagnostics;
using System.Text;
using Semaphore = ParallelProgrammingLab1.PetriNet.Semaphore;

namespace ParallelProgrammingLab1;

public class MyThread
{
    public MyThread(string name, int priority, int cpuBurst, int quantity, IEnumerable<Semaphore> semaphores)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            name = $"T{_threadCounter}";
            _threadCounter++;
        }

        if (Threads.Any(r => r.Name == name))
            throw new Exception($"Поток с именем {name} уже существует.");
        
        Name = name;
        Priority = priority;
        CpuBurst = cpuBurst;
        _quantity = quantity;
        _semaphores = semaphores.ToArray();

        foreach (var semaphore in _semaphores)
            semaphore.TryAddUser(this);
        
        _threadAction = obj =>
        {
            var timeslice = ((int[])obj!)[0];
            var timesliceNumber = ((int[])obj)[1];

            State = ThreadState.Running;
            //_protocol += $", {State} {_semaphores[_currentSemaphoreIndex].Name}";
            if (timesliceNumber == 1)
                Reseter.WaitOne();
            else
            {
                _stopwatch.Restart();
                while (_stopwatch.ElapsedMilliseconds < timeslice * timesliceNumber)
                    Thread.Yield();
            }
            
            _cpuBurstCompleted += timesliceNumber;
            
            _semaphores[_currentSemaphoreIndex].Release(this);

            if (_cpuBurstCompleted < CpuBurst)
            {
                State = ThreadState.InQueue;

                //_protocol += $", {State} {_semaphores[_currentSemaphoreIndex].Name}";

                return;
            }

            _cpuBurstCompleted = 0;

            _currentSemaphoreIndex++;

            if (_currentSemaphoreIndex < _semaphores.Length)
            {
                State = ThreadState.InQueue;
                
                //_protocol += $", {State} {_semaphores[_currentSemaphoreIndex].Name}";

                return;
            }

            _currentSemaphoreIndex = 0;

            _quantity--;

            if (_quantity < 1)
            {
                State = ThreadState.Completed;
                
                //_protocol += $", {State}";
            }
            else
            {
                State = ThreadState.InQueue;
                
                //_protocol += $", {State} {_semaphores[_currentSemaphoreIndex].Name}";
            }
        };
        
        _protocol = new StringBuilder(1024);
        
        Threads.Add(this);
    }

    private bool _notCompleted = true;
    
    public void Log()
    {
        if (_notCompleted)
        {
            string log;
            if (State == ThreadState.Completed)
            {
                log = $"{State}";
                _notCompleted = false;
            }
            else
            {
                log = $"{State} {_semaphores[_currentSemaphoreIndex].Name}";
            }

            _protocol.Append($", {log}");
        }
    }

    public static void PrintPrefixes(StreamWriter outputFile, bool preemptive)
    {
        if (preemptive)
        {
            foreach (var thread in Threads)
            {
                outputFile.WriteLine($"\t{thread.Name}:");

                for (var i = 0; i < thread._semaphores.Length; i++)
                {
                    if (i != thread._semaphores.Length - 1)
                    {
                        var x = $"{thread.Name}{new string('\'', i)}";
                        
                        var y = $"{x} = ((InQueue {thread._semaphores[i].Name} | Running {thread._semaphores[i].Name}) -> {x} | (InQueue {thread._semaphores[i + 1].Name} | Running {thread._semaphores[i + 1].Name}) -> {thread.Name}{new string('\'', i + 1)})";
                        
                        outputFile.WriteLine($"\t\t{y}");
                    }
                    else
                    {
                        var x = $"{thread.Name}{new string('\'', i)}";
                        
                        var y = $"{x} = ((InQueue {thread._semaphores[i].Name} | Running {thread._semaphores[i].Name}) -> {x} | Completed -> STOP)";
                        
                        outputFile.WriteLine($"\t\t{y}");
                    }
                }
            }
        }
        else
        {
            foreach (var thread in Threads)
            {
                outputFile.WriteLine($"\t{thread.Name}:");

                for (var i = 0; i < thread._semaphores.Length; i++)
                {
                    if (i != thread._semaphores.Length - 1)
                    {
                        var x = $"{thread.Name}{new string('\'', i)}";
                        var y = $"{x} = ((InQueue {thread._semaphores[i].Name} -> {x}) | ";
                        for (var j = 0; j < thread.CpuBurst - 1; j++)
                        {
                            y += $"Running {thread._semaphores[i].Name} -> ";
                        }

                        y +=
                            $"Running {thread._semaphores[i].Name} -> {thread.Name}{new string('\'', i + 1)})";

                        outputFile.WriteLine($"\t\t{y}");
                    }
                    else
                    {
                        var x = $"{thread.Name}{new string('\'', i)}";
                        var y = $"{x} = ((InQueue {thread._semaphores[i].Name} -> {x}) | ";
                        for (var j = 0; j < thread.CpuBurst - 1; j++)
                        {
                            y += $"Running {thread._semaphores[i].Name} -> ";
                        }

                        y += $"Running {thread._semaphores[i].Name} -> Completed)";

                        outputFile.WriteLine($"\t\t{y}");
                    }
                }
            }
        }
    }

    public static void PrintAlphabet(StreamWriter outputFile)
    {
        foreach (var thread in Threads)
        {
            var visitedSemaphores = new HashSet<string> { thread._semaphores[^1].Name };

            var alphabet = string.Empty;
            for (var i = 0; i < thread._semaphores.Length - 1; i++)
            {
                if (!visitedSemaphores.Contains(thread._semaphores[i].Name))
                {
                    alphabet += $"InQueue {thread._semaphores[i].Name}, Running {thread._semaphores[i].Name}, ";

                    visitedSemaphores.Add(thread._semaphores[i].Name);
                }
            }
            
            alphabet += $"InQueue {thread._semaphores[^1].Name}, Running {thread._semaphores[^1].Name}, Completed";
            
            outputFile.WriteLine($"\ta{thread.Name} = {{{alphabet}}}");
        }
    }
    
    public static void PrintParallelComposition(StreamWriter outputFile)
    {
        var parallelComposition = string.Empty;
        
        foreach (var thread in Threads)
        {
            parallelComposition += $"{thread.Name} || ";
        }
        
        outputFile.WriteLine($"\nПараллельная композиция: {parallelComposition[..^4]}");
    }
    
    public static void PrintProtocols(StreamWriter outputFile)
    {
        foreach (var thread in Threads)
            outputFile.WriteLine($"\t{thread.Name}: {thread}");
    }

    public AutoResetEvent Reseter { get; } = new(false);

    public override string ToString() => $"<{_protocol.ToString(2, _protocol.Length - 2)}>";

    private readonly StringBuilder _protocol;
    
    public void Execute(int timeslice, int timesliceNumber)
    {
        var thread = new Thread(_threadAction);
        _semaphores[_currentSemaphoreIndex].Hold(this);
        thread.Start(new[] { timeslice, timesliceNumber });
    }

    public bool IsAvailable() => _semaphores[_currentSemaphoreIndex].IsAvailable(this);

    public int GetRestOfCpuBurst() => CpuBurst - _cpuBurstCompleted;

    public IEnumerable<string> GetSemaphoreNames() => _semaphores.Select(s => s.Name);

    public ThreadState State { get; private set; } = ThreadState.InQueue;

    public string Name { get; }

    public int Priority { get; }

    public int CpuBurst { get; }

    public static List<MyThread> Threads { get; } = new();

    // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
    private readonly Semaphore[] _semaphores;
    
    private readonly ParameterizedThreadStart _threadAction;

    private readonly Stopwatch _stopwatch = new();

    // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
    private int _quantity;

    private int _cpuBurstCompleted;

    private int _currentSemaphoreIndex;

    private static int _threadCounter = 1;
}
