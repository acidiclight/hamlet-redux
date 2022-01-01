using System.Collections;
using System.Text;
using HamletRedux.Runtime;

namespace HamletRedux.Instructions;

public class SayInstruction : ConversationInstruction
{
    private string _message;
    private ChatMember _author;
    private List<ChatMember> _mentions = new List<ChatMember>();

    private SayInstruction(ChatConversation context, ChatMember author, string message)
        : base(context)
    {
        _message = message;
        _author = author;
    }

    private void AddMention(string mention)
    {
        if (Context.GetMemberById(mention, out var member))
        {
            if (!_mentions.Contains(member))
                _mentions.Add(member);
        }
        else
        {
            throw new InvalidOperationException($"Member ID '{mention}' not found.");
        }
    }
    
    public static ConversationInstruction Parse(ChatConversation context, string[] arguments)
    {
        if (arguments.Length < 1)
            throw new InvalidOperationException("Expected message text.");

        var currentAgent = context.GetPossessedMember();

        if (currentAgent == null)
            throw new InvalidOperationException("Cannot send a message without a possessed agent.");
        
        var message = arguments.First();
        var messageArguments = arguments.Skip(1).ToArray();

        var instruction = new SayInstruction(context, currentAgent, message);
        
        if (messageArguments.Any())
        {
            var messageCommand = messageArguments.First();
            var messageCommandArgs = messageArguments.Skip(1).ToArray();

            if (messageCommand == "to")
            {
                var mentionString = string.Join(" ", messageCommandArgs);
                var mentionList = mentionString.Split(", ", StringSplitOptions.RemoveEmptyEntries);

                if (!mentionList.Any())
                    throw new InvalidOperationException("Expected mention list after 'to' in say command.");

                foreach (var mention in mentionList)
                    instruction.AddMention(mention);
            }
            else
            {
                throw new InvalidOperationException($"Unexpected text {messageCommand} after message text.");
            }
        }

        return instruction;
    }
    
    public override IEnumerator PerformInstruction()
    {
        var message = EvaluateMessageVariables(_message);
        
        var typeText = $"@{_author.DisplayName} is typing...";
        var typeDelay = 100 * message.Length;
        Console.Write(typeText);
        Thread.Sleep(typeDelay);

        Console.CursorLeft = 0;
        for (var i = 0; i < typeText.Length; i++)
            Console.Write(" ");
        Console.CursorLeft = 0;


        Console.ForegroundColor = ConsoleColor.Red;
        Console.Write("<{0}> ", _author.DisplayName);
        Console.ForegroundColor = ConsoleColor.Gray;

        foreach (var mention in _mentions)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("@{0} ", mention.DisplayName);
            Console.ForegroundColor = ConsoleColor.Gray;
            yield return null;
        }

        Console.WriteLine(message);
    }

    private string EvaluateMessageVariables(string message)
    {
        var sb = new StringBuilder();
        
        var variableLeft = "${";
        var variableRight = "}";

        var i = 0;
        while (i < message.Length)
        {
            var messageLeft = message.Substring(i);
            var variableStart = messageLeft.IndexOf(variableLeft, StringComparison.Ordinal);
            var variableEnd = messageLeft.IndexOf(variableRight, StringComparison.Ordinal);
            
            if (variableStart < 0 || variableEnd < 0)
            {
                sb.Append(messageLeft);
                i = message.Length;
            }
            else
            {
                var toVariable = messageLeft.Substring(0, variableStart);
                if (!string.IsNullOrEmpty(toVariable))
                    sb.Append(toVariable);
                i += toVariable.Length;

                var variableNameStart = variableStart + variableLeft.Length;

                var variableName = messageLeft.Substring(variableNameStart, variableEnd - variableNameStart);

                i += variableLeft.Length + variableName.Length + variableRight.Length;

                if (Context.IsVariableDefined(variableName))
                {
                    var value = Context.GetVariableValue(variableName);

                    sb.Append(value.ToString());
                }
                else
                {
                    sb.Append("undefined");
                }
            }
        }
        
        return sb.ToString();
    }
}