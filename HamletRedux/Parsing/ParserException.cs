namespace HamletRedux.Parsing;

public class ParserException : Exception
{
    public ParserException(string line, int index, string message)
        : base($"{line} at position {index}: {message}")
    {
        
    }
}