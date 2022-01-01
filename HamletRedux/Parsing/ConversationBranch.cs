using System.Collections;
using HamletRedux.Instructions;
using HamletRedux.Runtime;
using HamletRedux.UnitedStatesOfWitchcraft;

namespace HamletRedux.Parsing;

public class ConversationBranch
{
    private List<ConversationInstruction> _instructions = new List<ConversationInstruction>();
    private string _name;
    private ChatConversation _context;
    
    public string Name => _name;
    
    
    public ConversationBranch(ChatConversation context, string name)
    {
        _context = context;
        _name = name;
    }

    public void AddInstruction(ConversationInstruction instruction)
    {
        _instructions.Add(instruction);
    }

    public IEnumerator RunBranch()
    {
        foreach (var instruction in _instructions)
            yield return _context.StartCoroutine(instruction.PerformInstruction());

        yield return _context.StartCoroutine(_context.WaitForChoice());
    }
}