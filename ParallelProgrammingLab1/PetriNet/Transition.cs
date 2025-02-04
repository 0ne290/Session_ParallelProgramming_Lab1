namespace ParallelProgrammingLab1.PetriNet;

public class Transition
{
    public Transition(IEnumerable<Place> inputPlaces, IEnumerable<Place> outputPlaces)
    {
        _inputPlaces = inputPlaces.ToArray();
        _outputPlaces = outputPlaces.ToArray();
    }

    public override string ToString()
    {
        var res = "Output Places:";

        foreach (var outputPlace in _outputPlaces)
            res += " " + outputPlace;

        res += "; Input Places:";
        
        foreach (var inputPlace in _inputPlaces)
            res += " " + inputPlace;

        return res;
    }

    public void Execute()
    {
        foreach (var place in _inputPlaces)
            place.Unload();
        
        foreach (var place in _outputPlaces)
            place.Load();
    }

    public bool IsAvailable()
    {
        foreach (var place in _inputPlaces)
            if (!place.IsAvailable())
                return false;

        return true;
    }
    
    private readonly Place[] _inputPlaces;
    
    private readonly Place[] _outputPlaces;
}