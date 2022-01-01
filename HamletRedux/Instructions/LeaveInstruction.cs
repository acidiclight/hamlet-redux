using System.Collections;
using HamletRedux.Runtime;

namespace HamletRedux.Instructions;

public class LeaveInstruction : ConversationInstruction
{
    private ChatMember _memberToLeave;
    
    private LeaveInstruction(ChatConversation context, ChatMember memberToLeave) 
        : base(context)
    {
        _memberToLeave = memberToLeave;
    }

    public static ConversationInstruction Parse(ChatConversation context, string[] arguments)
    {
        if (arguments.Any())
            throw new InvalidOperationException("This command doesn't have any arguments.");

        var currentAgent = context.GetPossessedMember();
        if (currentAgent == null)
            throw new InvalidOperationException("No member has been possessed, so that member can't leave the chat.");

        context.MarkMemberOffline(currentAgent);
        return new LeaveInstruction(context, currentAgent);
    }
    
    public override IEnumerator PerformInstruction()
    {
        Console.WriteLine(" * {0} left the chat.", _memberToLeave.DisplayName);

        Context.MarkMemberOffline(_memberToLeave);
        
        yield break;
    }
}