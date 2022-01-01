using System.Collections;
using HamletRedux.Runtime;

namespace HamletRedux.Instructions;

public class MathInstruction : ConversationInstruction
{
    private string _variable;
    private int _delta;
    
    private MathInstruction(ChatConversation context, string variableName, int delta) 
        : base(context)
    {
        _variable = variableName;
        _delta = delta;
    }

    public static ConversationInstruction ParseIncrement(ChatConversation context, string[] arguments)
    {
        if (arguments.Length < 1)
            throw new InvalidOperationException("Expected name of variable to increment.");

        var variable = arguments.First();

        if (!context.IsVariableDefined(variable))
            throw new InvalidOperationException($"{variable} is undefined.");

        return new MathInstruction(context, variable, 1);
    }
    
    public static ConversationInstruction ParseDecrement(ChatConversation context, string[] arguments)
    {
        if (arguments.Length < 1)
            throw new InvalidOperationException("Expected name of variable to decrement.");

        var variable = arguments.First();

        if (!context.IsVariableDefined(variable))
            throw new InvalidOperationException($"{variable} is undefined.");

        return new MathInstruction(context, variable, -1);
    }

    public static ConversationInstruction ParseAddition(ChatConversation context, string[] arguments)
    {
        if (arguments.Length < 2)
            throw new InvalidOperationException("Expected name of variable and value to add.");

        var variable = arguments.First();
        var valueString = arguments[1];

        if (!context.IsVariableDefined(variable))
            throw new InvalidOperationException($"{variable} is undefined.");

        if (int.TryParse(valueString, out var value))
        {
            return new MathInstruction(context, variable, value);
        }

        throw new InvalidOperationException("Expected a numeric integer value in the add command.");
    }

    public static ConversationInstruction ParseSubtraction(ChatConversation context, string[] arguments)
    {
        if (arguments.Length < 2)
            throw new InvalidOperationException("Expected name of variable and value to subtract.");

        var variable = arguments.First();
        var valueString = arguments[1];

        if (!context.IsVariableDefined(variable))
            throw new InvalidOperationException($"{variable} is undefined.");

        if (int.TryParse(valueString, out var value))
        {
            return new MathInstruction(context, variable, -value);
        }

        throw new InvalidOperationException("Expected a numeric integer value in the subtract command.");
    }

    
    public override IEnumerator PerformInstruction()
    {
        var value = Context.GetVariableValue(_variable);
        Context.SetVariableValue(_variable, value + _delta);
        yield break;
    }
}