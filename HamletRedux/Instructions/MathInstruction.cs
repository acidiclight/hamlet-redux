using System.Collections;
using HamletRedux.Runtime;

namespace HamletRedux.Instructions;

public class MathInstruction : ConversationInstruction
{
    private string _variable;
    private int _delta;
    private MathOperation _operation;
    private string _variableToRead;
    
    private MathInstruction(ChatConversation context, string variableName, int delta) 
        : base(context)
    {
        _variable = variableName;
        _delta = delta;
    }

    private MathInstruction(ChatConversation context, string variable, MathOperation operation, string variableToRead)
        : base(context)
    {
        _variable = variable;
        _operation = operation;
        _variableToRead = variableToRead;
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
        else if (context.IsVariableDefined(valueString))
        {
            return new MathInstruction(context, variable, MathOperation.Add, valueString);
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
        else if (context.IsVariableDefined(valueString))
        {
            return new MathInstruction(context, variable, MathOperation.Subtract, valueString);
        }

        throw new InvalidOperationException("Expected a numeric integer value in the subtract command.");
    }

    
    public override IEnumerator PerformInstruction()
    {
        var value = Context.GetVariableValue(_variable);
        if (string.IsNullOrWhiteSpace(_variableToRead))
        {
            Context.SetVariableValue(_variable, value + _delta);
        }
        else
        {
            var valueToApply = Context.GetVariableValue(_variableToRead);

            switch (_operation)
            {
                case MathOperation.Add:
                    Context.SetVariableValue(_variable, value + valueToApply);
                    break;
                case MathOperation.Subtract:
                    Context.SetVariableValue(_variable, value - valueToApply);
                    break;
                case MathOperation.Multiply:
                    Context.SetVariableValue(_variable, value * valueToApply);
                    break;
                case MathOperation.Divide:
                    Context.SetVariableValue(_variable, value / valueToApply);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        yield break;
    }

    public enum MathOperation
    {
        Add,
        Subtract,
        Multiply,
        Divide
    }
}