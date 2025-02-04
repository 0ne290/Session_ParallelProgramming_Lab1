using System.Timers;
//using Semaphore = ParallelProgrammingLab1.PetriNet.Semaphore;
using Timer = System.Timers.Timer;

namespace ParallelProgrammingLab1;

public class ThreadScheduler : IDisposable
{
    public ThreadScheduler(int timeslice, bool preemptive, StreamWriter outputFile)
    {
        _timeslice = timeslice;
        
        _threads = new List<MyThread>(MyThread.Threads);
        
        Preemptive = preemptive;

        _outputFile = outputFile;
        
        //_timer = new Timer(_timeslice / 2.0);
        _timer1 = new Timer(_timeslice);
        //_timer.AutoReset = false;
        _timer1.AutoReset = false;
        //_timer.Elapsed += OnTimedEvent;
        _timer1.Elapsed += OnTimedEvent1;
        
        _comparator = Preemptive
                    ? (x, y) =>
                    {
                        var ret = y.Priority.CompareTo(x.Priority);
                        return ret != 0 ? ret : x.GetRestOfCpuBurst().CompareTo(y.GetRestOfCpuBurst());
                    }
                    : (x, y) => x.CpuBurst.CompareTo(y.CpuBurst);
    }
    
    public void Execute()
    {
        //_timer.Start();
        _timer1.Start();

        _locker.WaitOne();
    }
    
    /*private void OnTimedEvent(object? source, ElapsedEventArgs e)
    {
        Interlocked.Increment(ref _sync);

        var semaphores = Semaphore.Semaphores.Aggregate("", (current, resourse) => current + " " + resourse);

        var threads = MyThread.Threads.Aggregate("", (current, thread) => current + " " + thread);

        _outputFile.WriteLine($"\t{_quantumNumber, -4} | {semaphores, -60} | {threads}");
        
        _timer.Start();

        Interlocked.Decrement(ref _sync);
    }*/
    
    private void OnTimedEvent1(object? source, ElapsedEventArgs e)
    {
        //Interlocked.Increment(ref _sync);
        
        if (Preemptive)
        {
            foreach (var thread in _threads)
            {
                if (thread.State == ThreadState.Running)
                    thread.Reseter.Set();
                while (thread.State == ThreadState.Running)
                {
                }
            }
        }
        
        _threads.Sort(_comparator);
        foreach (var thread in _threads)
            if (thread.State == ThreadState.InQueue && thread.IsAvailable())
                thread.Execute(_timeslice, Preemptive ? 1 : thread.CpuBurst);
        
        //_quantumNumber++;

        if (_threads.All(t => t.State == ThreadState.Completed))
        {
            /*_timer.Stop();
            
            Interlocked.Decrement(ref _sync);

            while (_sync > 0)
                Thread.Yield();
            
            var semaphores1 = Semaphore.Semaphores.Aggregate("", (current, resourse) => current + " " + resourse);

            var threads1 = MyThread.Threads.Aggregate("", (current, thread) => current + " " + thread);

            _outputFile.WriteLine($"\t{_quantumNumber, -4} | {semaphores1, -60} | {threads1}");*/
            
            foreach (var thread in _threads)
                thread.Log();
            
            _locker.Set();

            return;
        }
        
        foreach (var thread in _threads)
            thread.Log();
        
        /*Interlocked.Increment(ref _sync);

        var semaphores = Semaphore.Semaphores.Aggregate("", (current, resourse) => current + " " + resourse);

        var threads = MyThread.Threads.Aggregate("", (current, thread) => current + " " + thread);

        _outputFile.WriteLine($"\t{_quantumNumber, -4} | {semaphores, -60} | {threads}");
        
        _timer.Start();*/
        
        _timer1.Start();

        //Interlocked.Decrement(ref _sync);
    }
    
    public void Dispose()
    {
        _locker.Dispose();
        
        _outputFile.Dispose();

        foreach (var thread in _threads)
            thread.Reseter.Dispose();
        
        //_timer.Elapsed -= OnTimedEvent;
        _timer1.Elapsed -= OnTimedEvent1;
        //_timer.Dispose();
        _timer1.Dispose();
    }
    
    private readonly int _timeslice;
    
    public bool Preemptive { get; }

    private readonly List<MyThread> _threads;
    
    private readonly Comparison<MyThread> _comparator;
    
    //private readonly Timer _timer;
    
    private readonly Timer _timer1;
    
    private readonly StreamWriter _outputFile;
    
    private readonly AutoResetEvent _locker = new(false);
    
    //private int _quantumNumber;
    
    //private int _sync;
}
