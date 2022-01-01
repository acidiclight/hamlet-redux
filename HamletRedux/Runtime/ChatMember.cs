namespace HamletRedux.Runtime;

public class ChatMember
{
    private string _id;
    private string _displayName;

    public string Id => _id;
    public string DisplayName => _displayName;
    
    private ChatMember(string fallbackId)
    {
        _id = fallbackId;
        _displayName = _id;
    }
    
    // TODO: Socially Distant agents
    public static ChatMember Fallback(string id)
    {
        return new ChatMember(id);
    }
}