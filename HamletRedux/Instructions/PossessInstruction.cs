using System.Collections;
using HamletRedux.Runtime;

namespace HamletRedux.Instructions;

public class PossessInstruction : ConversationInstruction
{
    private ChatMember _memberToPossess;
    
    private PossessInstruction(ChatConversation context, ChatMember memberToPossess) : 
        base(context)
    {
        _memberToPossess = memberToPossess;
    }

    public static ConversationInstruction Parse(ChatConversation context, string[] arguments)
    {
        if (arguments.Length < 1)
            throw new InvalidOperationException("Expected member ID to possess.");

        var memberId = arguments.First();

        if (context.GetMemberById(memberId, out var member))
        {
            // Possess the member now so we can validate instructions that require a possessed member.
            context.PossessMember(member);
            return new PossessInstruction(context, member);
        }

        throw new InvalidOperationException("Member ID not defined.");
    }
    
    public override IEnumerator PerformInstruction()
    {
        Context.PossessMember(_memberToPossess);
        yield break;
    }
}