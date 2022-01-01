using System.Collections;
using HamletRedux.Instructions;
using HamletRedux.Parsing;
using HamletRedux.UnitedStatesOfWitchcraft;

namespace HamletRedux.Runtime;

public class ChatConversation
{
    private string _name;
    // Ideally, this would store the Socially Distant agent data for each member...
    private Dictionary<string, ChatMember> _members = new Dictionary<string, ChatMember>();
    private Dictionary<string, ConversationBranch> _branches = new Dictionary<string, ConversationBranch>();
    private List<string> _onlineIds = new List<string>();
    private ChatMember _activeMember;
    private List<string> _parserBranchNames = new List<string>();
    private bool _isParsing;
    private bool _isReady = false;
    private bool _isChatActive = false;
    private ConversationBranch _currentBranch;
    private List<ChatChoice> _choices = new List<ChatChoice>();
    private ConversationState _variables;
    
    public string Name => _name;
    
    
    private ChatConversation(string name)
    {
        _name = name;
        _variables = new ConversationState(this);
    }

    #region Variable Support

    public bool IsVariableDefined(string name)
        => _variables.IsDefined(name);

    public int GetVariableValue(string name)
        => _variables.GetValue(name);

    public void SetVariableValue(string name, int value)
        => _variables.SetValue(name, value);

    #endregion
    
    #region Chat Runner

    public void AddChoice(string message, ConversationInstruction action)
    {
        var choice = new ChatChoice(this, message, action);
        _choices.Add(choice);
    }

    public void ClearChoices() => _choices.Clear();

    public IEnumerator WaitForChoice()
    {
        if (!_choices.Any())
            yield break;

        Console.WriteLine();
        
        var top = Console.CursorTop;
        Console.CursorLeft = 0;

        var hasChosen = false;

        var choice = 0;
        
        while (!hasChosen)
        {
            Console.CursorTop = top;

            for (var i = 0; i < _choices.Count; i++)
            {
                var choiceData = _choices[i];
                var text = choiceData.Text;

                Console.Write(" - ");

                if (choice == i)
                {
                    Console.BackgroundColor = ConsoleColor.White;
                    Console.ForegroundColor = ConsoleColor.Black;
                }
                else
                {
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.ForegroundColor = ConsoleColor.Gray;
                }

                Console.Write(text);
                
                Console.BackgroundColor = ConsoleColor.Black;
                Console.ForegroundColor = ConsoleColor.Gray;

                Console.WriteLine();
            }

            var keyData = Console.ReadKey();

            switch (keyData.Key)
            {
                case ConsoleKey.Enter:
                    hasChosen = true;
                    yield return this.StartCoroutine(_choices[choice].RunChoice());
                    break;
                case ConsoleKey.UpArrow:
                    if (choice == 0)
                        choice = _choices.Count - 1;
                    else choice--;
                    break;
                case ConsoleKey.DownArrow:
                    if (choice == _choices.Count - 1)
                        choice = 0;
                    else choice++;
                    break;
            }
        }
    }

    public IEnumerator RunConversation()
    {
        if (_isChatActive || !_isReady)
            yield break;

        _isChatActive = true;

        Console.Clear();
        Console.WriteLine("Starting chat: {0}", Name);

        _currentBranch = _branches["main"];

        yield return this.StartCoroutine(_currentBranch.RunBranch());
    }

    public IEnumerator RunBranch(string name)
    {
        _currentBranch = _branches[name];

        yield return this.StartCoroutine(_currentBranch.RunBranch());
    }
    
    #endregion

    public void MarkMemberOnline(ChatMember member)
    {
        if (!_onlineIds.Contains(member.Id))
            _onlineIds.Add(member.Id);
    }
    
    public void MarkMemberOffline(ChatMember member)
    {
        if (_onlineIds.Contains(member.Id))
            _onlineIds.Remove(member.Id);
    }

    public bool BranchExists(string id)
    {
        return _branches.ContainsKey(id) || (_isParsing && _parserBranchNames.Contains(id));
    }

    public bool GetMemberById(string id, out ChatMember member) 
        => _members.TryGetValue(id, out member);

    public ChatMember GetPossessedMember()
    {
        return _activeMember;
    }

    public void PossessMember(ChatMember member)
    {
        if (member == null)
            throw new InvalidOperationException("Cannot possess a null member!");

        if (_onlineIds.Contains(member.Id) || _isParsing)
            _activeMember = member;
        else throw new InvalidOperationException("Cannot possess a member that's offline.");
    }
    
    public static ChatConversation FromFile(string filePath)
    {
        var conversation = new ChatConversation(Path.GetFileName(filePath));

        conversation.LoadFromFile(filePath);
        conversation._isReady = true;
        
        return conversation;
    }

    private void LoadFromFile(string filePath)
    {
        _isParsing = true;
        
        using var stream = File.OpenRead(filePath);
        using var reader = new StreamReader(stream);

        // Start by reading each line in the file.
        var lines = new List<string>();
        while (!reader.EndOfStream)
        {
            // Read the line.
            var line = reader.ReadLine();

            // Skip it if it's blank.
            if (string.IsNullOrWhiteSpace(line))
                continue;

            // Strip out any comments.
            if (line.Contains("//"))
            {
                var commentIndex = line.IndexOf("//", StringComparison.Ordinal);
                line = line.Substring(0, commentIndex);
            }

            // Another whitespace check.
            if (string.IsNullOrWhiteSpace(line))
                continue;

            // Store the line!
            lines.Add(line.Trim());
        }

        Console.WriteLine("{0} lines read.", lines.Count);

        // Process sections.
        var sections = ProcessSections(lines.ToArray());
        Console.WriteLine("{0} sections read.", sections.Length);
        
        // Find the member definition.
        var memberSection = sections.FirstOrDefault(x => x.Name == "members");
        if (memberSection == null)
            throw new InvalidOperationException("Chat conversation is missing the members list!");

        ProcessMembers(memberSection);
        Console.WriteLine("{0} members in the chat:", _members.Count);
        foreach (var member in _members)
            Console.WriteLine(" - {0}: {1}", member.Key, _onlineIds.Contains(member.Key) ? "invited" : "uninvited");

        var branches = sections.Where(x => x.Name != "members").ToArray();
        if (branches.All(x => x.Name != "main"))
            throw new InvalidOperationException("Chat conversation doesn't have a main branch.");

        var stateList = branches.FirstOrDefault(x => x.Name == "state");
        if (stateList != null)
        {
            branches = branches.Where(x => x.Name != "state").ToArray();
            ProcessConversationState(stateList);
        }
        
        _parserBranchNames.AddRange(branches.Select(x=>x.Name));

        foreach (var branch in branches)
            ProcessBranch(branch);

        _isParsing = false;
    }

    private ConversationSection[] ProcessSections(string[] lines)
    {
        var isInSection = false;
        var sectionStartChar = '[';
        var sectionEndChat = ']';
        var usedSections = new List<string>();
        var sections = new List<ConversationSection>();
        var section = null as ConversationSection;

        foreach (var line in lines)
        {
            if (isInSection)
            {
                var sectionTest = line.IndexOf(sectionStartChar, StringComparison.Ordinal);
                if (sectionTest != 0)
                {
                    section.AddAction(line);
                    continue;
                }

                if (!section.Actions.Any())
                {
                    throw new ParserException(line, 0, "Empty section definitions are not allowed.");
                }

                isInSection = false;
                section = null;
            }

            var sectionStartIndex = line.IndexOf(sectionStartChar, StringComparison.Ordinal);

            if (sectionStartIndex != 0)
            {
                throw new ParserException(line, 0, "Expected section definition but got an unexpected string.");
            }

            var sectionEndIndex = line.IndexOf(sectionEndChat, StringComparison.Ordinal);

            if (sectionEndIndex + 1 != line.Length)
            {
                if (sectionEndIndex > 0)
                    throw new ParserException(line, sectionEndIndex + 1,
                        "Unexpected text after section definition.");
                else if (sectionEndIndex < 0)
                    throw new ParserException(line, line.Length, "Expected end of section definition.");
            }

            var begin = sectionStartIndex + 1;
            var end = sectionEndIndex - begin;
            var sectionName = line.Substring(begin, end);

            if (string.IsNullOrWhiteSpace(sectionName))
                throw new ParserException(line, begin, "Expected name of section!");

            if (usedSections.Contains(sectionName))
                throw new ParserException(line, begin, "Duplicate section definition.");

            usedSections.Add(sectionName);

            section = new ConversationSection(sectionName);
            sections.Add(section);
            isInSection = true;
        }

        return sections.ToArray();
    }

    private void ProcessMembers(ConversationSection memberSection)
    {
        // Add the player member.
        _members.Clear();
        _members.Add("player", ChatMember.Fallback("player"));
        _onlineIds.Add("player");

        foreach (var line in memberSection.Actions)
        {
            var equalSign = line.IndexOf("=", StringComparison.Ordinal);

            if (equalSign >= 0)
            {
                var name = line.Substring(0, equalSign);
                var online = line.Substring(equalSign + 1);

                if (string.IsNullOrWhiteSpace(name))
                    throw new ParserException(line, 0, "Member ID must not be blank.");

                if (_members.ContainsKey(name))
                    throw new ParserException(line, 0,
                        "Duplicate member ID! Note: 'player' is implicitly defined and is always online.");
                
                if (online == "yes" || online == "no")
                {
                    if (online == "yes")
                        _onlineIds.Add(name);
                }
                else
                {
                    throw new ParserException(line, equalSign + 1, "Expected either 'yes' or 'no' for online state.");
                }

                _members.Add(name, ChatMember.Fallback(name));
            }
            else
            {
                throw new ParserException(line, 0, "Expected member definition: <id>=<yes/no>");
            }
        }
    }

    private void ProcessConversationState(ConversationSection section)
    {
        // Clear any variables from previous conversations.
        _variables.Clear();

        foreach (var line in section.Actions)
        {
            // Find the assignment operator.
            var equalSign = line.IndexOf("=", StringComparison.Ordinal);

            if (equalSign > 0)
            {
                var name = line.Substring(0, equalSign).Trim();
                var valueString = line.Substring(equalSign + 1).Trim();

                if (string.IsNullOrWhiteSpace(name))
                    throw new ParserException(line, 0, "Expected a variable name.");

                if (int.TryParse(valueString, out var value))
                {
                    _variables.SetValue(name, value);
                }
                else
                {
                    throw new ParserException(line, equalSign,
                        "Expected numeric integer value for variable " + name + ".");
                }
            }
            else
            {
                throw new ParserException(line, line.Length, "Expected state variable assignment.");
            }
        }
    }
    
    private void ProcessBranch(ConversationSection section)
    {
        // Back up the online status of members.
        var online = _onlineIds.ToArray();
        
        var branch = new ConversationBranch(this, section.Name);
        _branches.Add(branch.Name, branch);

        foreach (var line in section.Actions)
        {
            var tokens = Tokenize(line);

            if (tokens.Length == 0)
                continue;

            var instructionName = tokens.First();
            var arguments = tokens.Skip(1).ToArray();

            var instruction = ConversationInstruction.CreateFromTokens(this, instructionName, arguments);

            branch.AddInstruction(instruction);
        }
        
        // Restore it.
        _onlineIds.Clear();
        _onlineIds.AddRange(online);
    }

    private string[] Tokenize(string line)
    {
        var words = new List<string>();
        var isInQuote = false;
        var isEscaping = false;
        var word = string.Empty;

        for (var i = 0; i <= line.Length; i++)
        {
            if (i == line.Length)
            {
                if (isEscaping)
                    throw new ParserException(line, i, "Unfinished escape sequence.");
                if (isInQuote)
                    throw new ParserException(line, i, "Newline in string.");

                if (!string.IsNullOrEmpty(word))
                {
                    words.Add(word);
                }
            }
            else
            {
                var ch = line[i];

                if (isEscaping)
                {
                    isEscaping = false;
                    word += ch;
                    continue;
                }

                if (ch == '\\')
                {
                    isEscaping = true;
                    continue;
                }

                if (ch == '"')
                {
                    if (isInQuote)
                    {
                        if (!string.IsNullOrEmpty(word))
                        {
                            words.Add(word);
                            word = string.Empty;
                        }
                        
                        isInQuote = false;
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(word))
                        {
                            throw new ParserException(line, i, "Unexpected start of string.");
                        }

                        isInQuote = true;
                    }

                    continue;
                }

                if (isInQuote || !char.IsWhiteSpace(ch))
                {
                    word += ch;
                    continue;
                }

                if (!string.IsNullOrEmpty(word))
                {
                    words.Add(word);
                    word = string.Empty;
                }
                
                
            }
        }

        return words.ToArray();
    }
}