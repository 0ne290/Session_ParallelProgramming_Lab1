namespace ParallelProgrammingLab1;

public class InputData
{
    public bool Pa { get; set; }
    
    public int Qt { get; set; }
    
    public int MaxT { get; set; }
    
    public int MaxP { get; set; }
    
    public int Nr { get; set; }
    
    public int Np { get; set; }
    
    public List<ResourceConfiguration> Resources { get; set; }
    
    public List<MyThreadConfiguration> Threads { get; set; }
}