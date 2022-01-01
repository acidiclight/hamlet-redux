using System.Collections;
using HamletRedux.Runtime;

namespace HamletRedux.Instructions;

public class ReturnInstruction : ConversationInstruction
{
    private ReturnInstruction(ChatConversation context) : base(context)
    {
    }

    public static ConversationInstruction Parse(ChatConversation context, string[] args)
    {
        if (args.Any())
            throw new InvalidOperationException("This command doesn't accept any arguments.");

        return new ReturnInstruction(context);
    }
    
    public override IEnumerator PerformInstruction()
    {
        yield break;
    }
}