namespace HamletRedux;

public class ConversationSection
{
    private string _name;
    private List<string> _actions;

    public ConversationSection(string name)
    {
        _name = name;
        _actions = new List<string>();
    }

    public string Name => _name;
    public IEnumerable<string> Actions => _actions;

    public void AddAction(string action)
    {
        _actions.Add(action);
    }
}