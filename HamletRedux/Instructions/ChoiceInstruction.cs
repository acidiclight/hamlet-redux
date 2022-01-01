using System.Collections;
using HamletRedux.Runtime;

namespace HamletRedux.Instructions;

public class ChoiceInstruction : ConversationInstruction
{
    private string _message;
    private ConversationInstruction _instruction;
    
    private ChoiceInstruction(ChatConversation context, string message, ConversationInstruction instruction) 
        : base(context)
    {
        _message = message;
        _instruction = instruction;
    }

    public static ConversationInstruction Parse(ChatConversation context, string[] arguments)
    {
        if (arguments.Length < 2)
            throw new InvalidCastException("Expected choice message and command.");

        var message = arguments.First();
        var choiceCommandLine = arguments.Skip(1).ToArray();

        var commandName = choiceCommandLine.First();
        var commandArgs = choiceCommandLine.Skip(1).ToArray();

        var choiceInstruction = ConversationInstruction.CreateFromTokens(context, commandName, commandArgs);

        return new ChoiceInstruction(context, message, choiceInstruction);
    }
    
    public override IEnumerator PerformInstruction()
    {
        Context.AddChoice(_message, _instruction);
        yield break;
    }
}