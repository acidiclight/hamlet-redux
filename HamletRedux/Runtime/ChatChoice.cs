using System.Collections;
using HamletRedux.Instructions;
using HamletRedux.UnitedStatesOfWitchcraft;

namespace HamletRedux.Runtime;

public class ChatChoice
{
    private string _message;
    private ConversationInstruction _action;
    private ChatConversation _context;

    public ChatChoice(ChatConversation context, string message, ConversationInstruction instruction)
    {
        _context = context;
        _message = message;
        _action = instruction;
    }

    public string Text => _message;

    public IEnumerator RunChoice()
    {
        yield return _context.StartCoroutine(_action.PerformInstruction());
    }
}