using System.Collections;

namespace HamletRedux;

public class MissionInstruction : ConversationInstruction
{
    private MissionInstruction(ChatConversation context) : base(context)
    {
    }

    public static ConversationInstruction Parse(ChatConversation context, string[] arguments)
    {
        // TODO: Mission system
        return new MissionInstruction(context);
    }
    
    public override IEnumerator PerformInstruction()
    {
        Console.WriteLine(" * Mission unlock instruction isn't implemented.");
        yield break;
    }
}