namespace HamletRedux.Runtime;

public class ConversationState
{
    private ChatConversation _context;
    private Dictionary<string, int> _variables = new Dictionary<string, int>();

    public ConversationState(ChatConversation context)
    {
        _context = context;
    }

    public bool IsDefined(string name)
        => _variables.ContainsKey(name);

    public void SetValue(string name, int value)
    {
        if (IsDefined(name))
            _variables[name] = value;
        else
            _variables.Add(name, value);
    }

    public int GetValue(string name)
    {
        if (!IsDefined(name))
            throw new InvalidOperationException($"Variable {name} is not defined.");

        return _variables[name];
    }

    public void Clear()
    {
        _variables.Clear();
    }
}