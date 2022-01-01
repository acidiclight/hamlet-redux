using System.Collections;
using HamletRedux.Runtime;
using HamletRedux.UnitedStatesOfWitchcraft;

namespace HamletRedux.Instructions;

public class DoInstruction : ConversationInstruction
{
    private ConversationInstruction _instruction;
    private Condition _condition;
    private string _variable;
    private int _literalCheck;
    private string _variableCheck;
    
    private DoInstruction(ChatConversation context, ConversationInstruction instruction, Condition condition, string variable, int literalCheck) 
        : base(context)
    {
        _instruction = instruction;
        _condition = condition;
        _variable = variable;
        _literalCheck = literalCheck;
    }
    
    private DoInstruction(ChatConversation context, ConversationInstruction instruction, Condition condition, string variable, string variableCheck) 
        : base(context)
    {
        _instruction = instruction;
        _condition = condition;
        _variable = variable;
        _variableCheck = variableCheck;
    }

    public static ConversationInstruction Parse(ChatConversation context, string[] arguments)
    {
        if (FindCondition(arguments, out var conditionIndex, out var conditionType, out var condition))
        {
            var command = arguments.Take(conditionIndex).ToArray();

            if (!command.Any())
            {
                throw new InvalidOperationException("Expected command to execute in conditional.");
            }

            var commandName = command.First();
            var commandArgs = command.Skip(1).ToArray();

            var commandInstruction = ConversationInstruction.CreateFromTokens(context, commandName, commandArgs);

            var equalSign = condition.IndexOf("=", StringComparison.Ordinal);

            if (equalSign < 0)
                throw new InvalidOperationException("Expected an equality comparison in the conditional clause.");
            
            var conditionLeft = condition.Substring(0, equalSign).Trim();
            var conditionRight = condition.Substring(equalSign + 1).Trim();

            if (string.IsNullOrWhiteSpace(conditionLeft))
                throw new InvalidOperationException("Expected left-hand operand in conditional clause.");
            
            if (string.IsNullOrWhiteSpace(conditionRight))
                throw new InvalidOperationException("Expected right-hand operand in conditional clause.");

            if (!context.IsVariableDefined(conditionLeft))
                throw new InvalidOperationException($"Left-hand operand must be a defined variable.");

            if (int.TryParse(conditionRight, out var literalCheck))
            {
                return new DoInstruction(context, commandInstruction, conditionType, conditionLeft, literalCheck);
            }
            else if (context.IsVariableDefined(conditionRight))
            {
                return new DoInstruction(context, commandInstruction, conditionType, conditionLeft, conditionRight);
            }

            throw new InvalidOperationException("Right-hand operand must be a number or a defined variable.");
        }
        else
        {
            throw new InvalidOperationException("Expected condition clause.");
        }
    }

    public override IEnumerator PerformInstruction()
    {
        switch (_condition)
        {
            case Condition.If:
                yield return Context.StartCoroutine(PerformIf());
                break;
            case Condition.While:
                yield return Context.StartCoroutine(PerformWhile());
                break;
            case Condition.Until:
                yield return Context.StartCoroutine(PerformUntil());
                break;
        }
    }

    private IEnumerator PerformIf()
    {
        var left = Context.GetVariableValue(_variable);
        var right = string.IsNullOrWhiteSpace(_variableCheck)
            ? _literalCheck
            : Context.GetVariableValue(_variableCheck);

        if (left == right)
        {
            yield return Context.StartCoroutine(_instruction.PerformInstruction());
        }
    }

    private IEnumerator PerformWhile()
    {
        var left = Context.GetVariableValue(_variable);
        var right = string.IsNullOrWhiteSpace(_variableCheck)
            ? _literalCheck
            : Context.GetVariableValue(_variableCheck);

        var areEqual = left == right;
        while (areEqual)
        {
            yield return Context.StartCoroutine(_instruction.PerformInstruction());
            left = Context.GetVariableValue(_variable);
            right = string.IsNullOrWhiteSpace(_variableCheck)
                ? _literalCheck
                : Context.GetVariableValue(_variableCheck);

            areEqual = left == right;
        }
    }

    private IEnumerator PerformUntil()
    {
        var left = Context.GetVariableValue(_variable);
        var right = string.IsNullOrWhiteSpace(_variableCheck)
            ? _literalCheck
            : Context.GetVariableValue(_variableCheck);

        var areEqual = left == right;
        while (!areEqual)
        {
            yield return Context.StartCoroutine(_instruction.PerformInstruction());
            left = Context.GetVariableValue(_variable); 
            right = string.IsNullOrWhiteSpace(_variableCheck)
                ? _literalCheck
                : Context.GetVariableValue(_variableCheck);

            areEqual = left == right;
        }
    }


    private static bool FindCondition(string[] arguments, out int conditionIndex, out Condition conditionType, out string condition)
    {
        conditionType = Condition.If;
        condition = string.Empty;
        conditionIndex = 0;

        var i = -1;
        var result = false;
        for (i = arguments.Length - 1; i >= 0; i--)
        {
            var arg = arguments[i];

            if (arg == "if")
            {
                conditionType = Condition.If;
                result = true;
                break;
            }
            else if (arg == "while")
            {
                conditionType = Condition.While;
                result = true;
                break;
            }
            else if (arg == "until")
            {
                conditionType = Condition.Until;
                result = true;
                break;
            }
        }

        if (result)
        {
            conditionIndex = i;
            
            var afterCondition = arguments.Skip(i + 1).ToArray();

            condition = string.Join(" ", afterCondition);

            result = !string.IsNullOrWhiteSpace(condition);
        }

        return result;
    }
    
    public enum Condition
    {
        If,
        While,
        Until
    }
}