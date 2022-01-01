// See https://aka.ms/new-console-template for more information

using HamletRedux;

var chatFile = Path.Combine(Environment.CurrentDirectory, "conversation.chat");

Console.WriteLine("Loading {0}...", chatFile);

var chat = ChatConversation.FromFile(chatFile);

FakeCoroutineRunner.FakeCoroutine(chat.RunConversation());