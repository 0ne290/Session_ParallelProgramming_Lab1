namespace ParallelProgrammingLab1;

public class MyThreadConfiguration
{
    public string Name { get; set; }
    
    public int Priority { get; set; }
    
    public int CpuBurst { get; set; }
    
    public int Quantity { get; set; }
    
    public List<string> ResourceNames { get; set; }
}