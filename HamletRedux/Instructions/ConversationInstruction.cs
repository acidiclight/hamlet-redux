using System.Collections;
using HamletRedux.Runtime;

namespace HamletRedux.Instructions;

public abstract class ConversationInstruction
{
    private static Dictionary<string, Func<ChatConversation, string[], ConversationInstruction>> _parsers =
        new Dictionary<string, Func<ChatConversation, string[], ConversationInstruction>>();

    private ChatConversation _context;

    protected ChatConversation Context => _context;
    
    protected ConversationInstruction(ChatConversation context)
    {
        _context = context;
    }
    
    static ConversationInstruction()
    {
        _parsers.Add("possess", PossessInstruction.Parse);
        _parsers.Add("return", ReturnInstruction.Parse);
        _parsers.Add("say", SayInstruction.Parse);
        _parsers.Add("invite", InviteInstruction.Parse);
        _parsers.Add("leave", LeaveInstruction.Parse);
        _parsers.Add("choice", ChoiceInstruction.Parse);
        _parsers.Add("goto", GotoInstruction.Parse);
        _parsers.Add("block", BlockInstruction.Parse);
        _parsers.Add("mission", MissionInstruction.Parse);
        _parsers.Add("clear_choices", ClearChoicesInstruction.Parse);
        _parsers.Add("increment", MathInstruction.ParseIncrement);
        _parsers.Add("decrement", MathInstruction.ParseDecrement);
        _parsers.Add("add", MathInstruction.ParseAddition);
        _parsers.Add("subtract", MathInstruction.ParseSubtraction);
        _parsers.Add("do", DoInstruction.Parse);
    }

    public static ConversationInstruction CreateFromTokens(ChatConversation context, string type, string[] arguments)
    {
        if (_parsers.TryGetValue(type, out var parser))
        {
            try
            {
                var instruction = parser(context, arguments);
                if (instruction == null)
                    throw new InvalidOperationException($"Command {type} isn't recognized.");
                return instruction;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Command '{type}' has invalid arguments.", ex);
            }
        }
        
        throw new InvalidOperationException($"Command {type} isn't recognized.");
    }

    public abstract IEnumerator PerformInstruction();
}

public class BlockInstruction : ConversationInstruction
{
    private ChatMember _blocker;
    private ChatMember _blocked;
    
    private BlockInstruction(ChatConversation context, ChatMember blocker, ChatMember blocked)
        : base(context)
    {
        _blocker = blocker;
        _blocked = blocked;
    }

    public static ConversationInstruction Parse(ChatConversation context, string[] arguments)
    {
        if (arguments.Length < 1)
            throw new InvalidOperationException("Expected member ID to block.");

        var blockedMemberId = arguments.First();

        var agent = context.GetPossessedMember();
        if (agent == null)
            throw new InvalidOperationException("You must possess a member before you can block another member.");

        if (context.GetMemberById(blockedMemberId, out var blocked))
        {
            return new BlockInstruction(context, agent, blocked);
        }

        throw new InvalidOperationException($"Member ID {blockedMemberId} not defined.");
    }
    
    public override IEnumerator PerformInstruction()
    {
        yield break;
    }
}