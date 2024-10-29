using System.Text;
using System.Text.RegularExpressions;

namespace PBReportConverter.Parser;

internal class PBExpressionParser
{
    private StringReader? _reader;
    private int _lastChar;
    private char _lastCharAsChar;

    // Main StringBuilder used to store the converted expression
    private readonly StringBuilder _expressionSb = new();

    // Buffer StringBuilder used in case an expression needs to be stored for later use in code
    private readonly StringBuilder _expressionBufferSb = new();

    // Buffer used to check the last string literal read by parser
    private readonly StringBuilder _lastStrBufferSb = new();

    // Switch between writing into the main StringBuilder or the Expression Buffer
    private bool _writeToBuffer = false;

    // Keywords used to guide the parser's behavior
    private static readonly List<string> _knownKeywords = ["when", "then", "and", "or", "else", "not"];

    // Expression delimiters
    private static readonly List<char> _exprDelims = ['(', ')', ',', '\''];

    // DevExpress event when the expression is to be evaluated
    private string _event = "BeforePrint";

    private static readonly string[] _substringFuncs = ["left", "right"];

    private static string FormatChar(int c) => c < 32 ? $"\\x{c:X2}" : $"'{(char)c}'";

    private void ReadChar()
    {
        _lastChar = _reader!.Read();
        _lastCharAsChar = (char) _lastChar;
    }

    private void Append(string value)
    {
        if (_writeToBuffer)
        {
            _expressionBufferSb.Append(value);
        }
        else
        {
            _expressionSb.Append(value);
        }
    }

    private void Append(char value)
    {
        Append(value.ToString());
    }

    private void AddChar(bool skipWhitespace = true)
    {
        Append(_lastCharAsChar);
        if(_lastChar == '=')
        {
            Append('=');
        }
        if (skipWhitespace)
        {
            ReadCharSkipWhitespace();
        }
        else
        {
            ReadChar();
        }
    }

    private void ReadNonAlphanumericChar()
    {
        while (_lastChar >= 0 && !(char.IsAsciiLetterOrDigit(_lastCharAsChar) || _exprDelims.Contains(_lastCharAsChar)))
        {
            AddChar();
        }
    }

    private void ReadCharSkipWhitespace()
    {
        do ReadChar();
        while (_lastChar >= 0 && char.IsWhiteSpace(_lastCharAsChar));
    }

    // Parses and returns the next string of letters and numbers in the expression text
    private string ParseString()
    {
        Span<char> buf = stackalloc char[10000];
        int pos = 0;
        if (Char.IsWhiteSpace(_lastCharAsChar))
        {
            ReadCharSkipWhitespace();
        }
        while (Char.IsAsciiLetterOrDigit(_lastCharAsChar) || _lastChar == '_')
        {
            buf[pos++] = _lastCharAsChar;
            ReadChar();
        }
        if (Char.IsWhiteSpace(_lastCharAsChar))
        {
            ReadCharSkipWhitespace();
        }

        if (pos == 0) 
            throw new Exception($"Unexpected character {FormatChar(_lastChar)}.");

        return buf[..pos].ToString();
    }

    public (string printEvent, string expr) Parse(string expression)
    {
        _expressionSb.Clear();

        // Removes all occurrences of for loops in expression, as they are unneccesary in DevExpress
        // For example, "sum(value for all)" or "sum(value for group 3)" would become "sum(value)"
        expression = Regex.Replace(expression, @"\sfor [a-zA-Z0-9]+(\s[0-9]+)?", "");

        _reader = new StringReader(expression);
        _event = "BeforePrint";
        ReadChar();
        ParseExpression();

        _reader.Dispose();

        return (_event, _expressionSb.ToString().Replace("<>", "!=").Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;"));
    }

    // Parses expression inside brackets.
    private void ParseBracketExpression()
    {
        if(_lastChar < 0)
        {
            return;
        }
        if (_lastChar == '(')
        {
            AddChar();
            ParseExpression();
            if (_lastChar == ')')
            {
                AddChar();
                ReadNonAlphanumericChar();
            }
        }
    }

    public void ParseExpression()
    {
        for (; ; )
        {
            // ')' and ',' mark the end of the expression, so the loop exits
            if (!(_lastChar >= 0 && _lastChar != ')' && _lastChar != ','))
            {
                break;
            }
            // PowerBuilder comments are formatted as /*comment text*/
            else if (_lastChar == '/' && _reader!.Peek() == '*')
            {
                ReadChar();
                ParseComment();
                continue;
            }
            else
            {
                ReadNonAlphanumericChar();
            }

            if (_lastChar == '(')
            {
                ParseBracketExpression();
                continue;
            }
            else if (_lastChar == '\'' || Char.IsAsciiDigit(_lastCharAsChar))
            {
                ParseLiteral();
            }
            else
            {
                var str = ParseString();

                if (_knownKeywords.Contains(str.ToLower()))
                {
                    // If the keyword is a logical operation, append the mapped operator and continue to the next expression
                    var mapOp = MapOperation(str.ToLower());
                    if (mapOp != string.Empty)
                    {
                        Append(mapOp);
                        continue;

                    }
                    // Otherwise, it's "when" or "then" in the case function. Write it to the string buffer for later parsing and break the loop
                    _lastStrBufferSb.Clear();
                    _lastStrBufferSb.Append(str.ToLower());
                    break;
                }

                if (_lastChar == '(')
                {
                    // Bracket after a string literal means the string is a function identifier
                    Append(FunctionHelper(str));
                    AddChar();

                    // Handle each function based on its specification.
                    switch (str.ToLower()){
                        case "case": { ParseCaseParameter(); break; }
                        case "string":
                        case "pos": { FlipParameters(str); break; }
                        case "page":
                        case "pagecount":
                        {
                            // Page functions can only be handled during the PrintOnPage event
                            _event = "PrintOnPage";
                            SkipParameters();
                            break;
                        }
                        case "last":
                        case "cumulativesum":
                        {
                            SkipParameters();
                            break;
                        }
                        default:
                        {
                            ParseParameters(str);
                            if (str.Equals("right", StringComparison.CurrentCultureIgnoreCase))
                            {
                                Append(')');
                            }
                            else if (str.Equals("getrow", StringComparison.CurrentCultureIgnoreCase))
                            {
                                // Rows are zero-indexed in PowerBuilder, but one-indexed in DevExpress
                                Append("+1");
                            }
                            break;
                        }
                    }
                }
                else
                {
                    // If it's not a function identifier or a known keyword, the parsed string is a variable name and should be appended as-is
                    Append(str);
                }
            }
        }
    }

    // Maps logical operation words to equivalent operators.
    private static string MapOperation(string op)
    {
        return op switch
        {
            "and" => " && ",
            "or" => " || ",
            "not" => "!",
            _ => ""
        };
    }

    // Skips parameters of a given expression in case when the DevExpress equivalent takes none.
    private void SkipParameters()
    {
        _expressionSb.Length -= 1;
        var bracketPairCheck = 0;
        while(_lastChar >= 0)
        {
            if (_lastChar == ')')
            {
                if (bracketPairCheck == 0)
                {
                    ReadCharSkipWhitespace();
                    break;
                }
                bracketPairCheck--;
            }
            else if (_lastChar == '(')
            {
                bracketPairCheck++;
            }
            ReadChar();
        }
    }

    // Reads past PowerBuilder comments.
    private void ParseComment()
    {
        ReadChar();
        while(_lastChar >= 0 && _lastChar != '*')
        {
            ReadCharSkipWhitespace();
            if (_lastChar == '*' && _reader!.Peek() == '/')
            {
                ReadChar();
                break;
            }
        }
        if( _lastChar != '/')
        {
            throw new Exception($"Unexpected character while parsing comment: {FormatChar(_lastChar)}.");
        }
        ReadChar();
    }

    // Parses string literal within an expression in quotation marks.
    private void ParseLiteral()
    {
        if(_lastChar == '\'')
        {
            AddChar(false);
            while(_lastChar >=0 && _lastChar != '\'')
            {
                AddChar(false);
            }
            if (_lastChar == '\'')
            {
                AddChar();
            }
            else
            {
                throw new Exception($"Unexpected character {FormatChar(_lastChar)}.");
            }
        }
        else
        {
            while (Char.IsAsciiDigit(_lastCharAsChar)){
                AddChar();
            }
        }
    }

    // Parses parameters passed to an expression, which may also be expressions themselves.
    private void ParseParameters(string function)
    {
        // right and left need the first parameter parsed, followed by appending ',0' in order to metch the specification of the DevExpress "Substring" syntax
        if(function.Equals("right", StringComparison.CurrentCultureIgnoreCase))
        {
            // right(string, length) has no equivalent in DevExpress, and needs to be converted into Reverse(Substring(Reverse(string), 0, length))
            Append("Reverse(");
            ParseExpression();
            Append(')');
        }
        else if(function.Equals("left", StringComparison.CurrentCultureIgnoreCase))
        {
            ParseExpression();
        }

        if (_substringFuncs.Contains(function.ToLower()))
        {
            Append(",0");
        }

        for(; ; )
        {
            if (_lastChar == ',')
            {
                AddChar();
            }
            else if (_lastChar == ')' || _lastChar < 0)
            {
                AddChar();
                break;
            }
            ParseExpression();
        }
    }

    /// <summary>
    /// Parameters for the case expression need to be parsed in a separate method.
    /// The general syntax is case(expression when testValue1 then value1 when testValue2 then value2 .... else valueN)
    /// The "else" is optional, in which case a default value must be added manually.
    /// At the moment the default value is an empty string, however rare cases use number values instead of strings and would need a different default value.
    /// Determening the type of the default value is still pending, so double check the case expressions in the resulting .repx files and modify them manually if needed.
    /// </summary>
    private void ParseCaseParameter()
    {
        // Check for whether an "else" default value is present, so the method knows if it needs to add its own default value at the end of the expression.
        var elseCheck = false;

        // Writes the expression to test the value of in the buffer, to use repeatedly in the loop.
        _writeToBuffer = true;
        ParseExpression();
        var expressionToCheck = _expressionBufferSb.ToString();
        _expressionBufferSb.Clear();
        _writeToBuffer = false;

        for (; ; )
        {
            if (_lastChar == ')' || _lastChar < 0)
            {
                if (!elseCheck)
                {
                    Append("''");
                }
                AddChar();
                break;
            }
            if (_lastStrBufferSb.ToString().Equals("else", StringComparison.CurrentCultureIgnoreCase))
            {
                ParseExpression();
                elseCheck = true;
                continue;
            }
            Append(expressionToCheck + "==");
            ParseExpression();
            Append(',');
            ParseExpression();
            Append(',');
        }
    }

    // Flips the two parameters of a function, in cases where the DevExpress and PowerBuilder order doesn't line up
    private void FlipParameters(string function)
    {
        _writeToBuffer = true;
        ParseExpression();
        var firstParam = _expressionBufferSb.ToString();
        _expressionBufferSb.Clear();
        ReadCharSkipWhitespace();
        ParseExpression();
        var secondParam = _expressionBufferSb.ToString();
        _expressionBufferSb.Clear();
        _writeToBuffer = false;
        if (_lastChar == ')' || _lastChar < 0)
        {
            Append($"{secondParam}{(function.Equals("string", StringComparison.CurrentCultureIgnoreCase) ? ",0" : "")},{firstParam}");
            AddChar();
        }
        else
        {
            throw new Exception($"Unexpected character {FormatChar(_lastChar)}.");
        }
    }

    // Maps the PowerBuilder functions into their DevExpress equivalents
    private static string FunctionHelper(string function)
    {
        return function.ToLower() switch
        {
            "if" => "Iif",
            "sum" => "[].Sum",
            "cumulativesum" => "1",
            "date" => "GetDate",
            "left" or "mid" or "string" => "Substring",
            "round" => "Round",
            "right" => "Reverse(Substring",
            "isnull" => "IsNull",
            "today" => "Today",
            "case" => "Iif",
            "pos" => "CharIndex",
            "getrow" => "CurrentRowIndexInGroup",
            "avg" => "[].Avg",
            "len" => "Len",
            "abs" => "Abs",
            "max" => "[].Max",
            "min" => "[].Min",
            "last" => "[DataSource.RowCount]",
            "page" => "[Arguments.PageIndex]",
            "pagecount" => "[Arguments.PageCount]",
            "trim" or "lefttrim" or "righttrim" => "Trim",
            _ => throw new Exception($"Unrecognized function: {function}")
        };
    }
}
