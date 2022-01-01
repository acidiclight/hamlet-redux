﻿using System.Collections;

namespace HamletRedux;

public class ClearChoicesInstruction : ConversationInstruction
{
    private ClearChoicesInstruction(ChatConversation context) : base(context)
    {
    }

    public static ConversationInstruction Parse(ChatConversation context, string[] arguments)
    {
        return new ClearChoicesInstruction(context);
    }
    
    public override IEnumerator PerformInstruction()
    {
        Context.ClearChoices();
        yield break;
    }
}