using System.Collections;

namespace HamletRedux;

public class InviteInstruction : ConversationInstruction
{
    private ChatMember _inviter;
    private ChatMember _invitee;
    
    private InviteInstruction(ChatConversation context, ChatMember inviter, ChatMember invitee)
        : base(context)
    {
        _inviter = inviter;
        _invitee = invitee;
    }

    public static ConversationInstruction Parse(ChatConversation context, string[] arguments)
    {
        if (arguments.Length < 1)
            throw new InvalidOperationException("Expected member ID to invite.");

        var memberId = arguments.First();
        if (context.GetMemberById(memberId, out var member))
        {
            var inviter = context.GetPossessedMember();
            if (inviter == null)
                throw new InvalidOperationException("You must possess a member before inviting a new member.");

            context.MarkMemberOnline(member);
            return new InviteInstruction(context, inviter, member);
        }
        else
        {
            throw new InvalidOperationException("Member ID not found.");
        }
    }
    
    public override IEnumerator PerformInstruction()
    {
        Console.WriteLine(" * {0} invited {1} to the chat.", _inviter.DisplayName, _invitee.DisplayName);

        Context.MarkMemberOnline(_invitee);
        
        yield break;
    }
}