using System.Collections;

namespace HamletRedux;

public class GotoInstruction : ConversationInstruction
{
    private string _branchName;
    
    private GotoInstruction(ChatConversation context, string branchName) 
        : base(context)
    {
        _branchName = branchName;
    }

    public static ConversationInstruction Parse(ChatConversation context, string[] arguments)
    {
        if (arguments.Length < 1)
            throw new InvalidOperationException("Expected name of branch to go to.");

        var branch = arguments.First();

        if (context.BranchExists(branch))
        {
            return new GotoInstruction(context, branch);
        }

        throw new InvalidCastException($"Branch ID {branch} does not exist.");
    }

    public override IEnumerator PerformInstruction()
    {
        yield return Context.StartCoroutine(Context.RunBranch(_branchName));
    }
}