using System.Text;

namespace ReportMigration.Parser;

internal class PBExpressionParser(string expression)
{
    private readonly StringReader _reader = new(expression);
    private int _lastChar;
    private char _testChar;
    private StringBuilder _sb = new StringBuilder();

    private static string FormatChar(int c) => c < 32 ? $"\\x{c:X2}" : $"'{(char)c}'";

    private void ReadChar()
    {
        _lastChar = _reader.Read();
        _testChar = (char) _lastChar;
    }

    private void AddChar()
    {
        _sb.Append((char)_lastChar);
        ReadChar();
    }

    private void ReadNonAlphanumericChar()
    {
        do AddChar();
        while (_lastChar >= 0 && !(char.IsAsciiLetterOrDigit((char)_lastChar) || _lastChar == '(' || _lastChar == ')' || _lastChar == ','));
    }

    private void ReadCharSkipWhitespace()
    {
        do ReadChar();
        while (_lastChar >= 0 && char.IsWhiteSpace((char)_lastChar));
    }

    private string ParseString()
    {
        Span<char> buf = stackalloc char[256];
        int pos = 0;
        if (Char.IsWhiteSpace((char)_lastChar))
        {
            ReadCharSkipWhitespace();
        }
        while (Char.IsAsciiLetterOrDigit((char)_lastChar) || _lastChar == '_')
        {
            buf[pos++] = (char)_lastChar;
            ReadChar();
        }
        if (Char.IsWhiteSpace((char)_lastChar))
        {
            ReadCharSkipWhitespace();
        }

        if (pos == 0) throw new Exception($"Unexpected character {FormatChar(_lastChar)}.");

        return buf[..pos].ToString();
    }

    public string Parse()
    {
        ReadChar();

        ParseExpression();

        return _sb.ToString();
    }

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
            }
        }
    }

    public void ParseExpression()
    {
        for (; ; )
        {
            if (_lastChar < 0)
            {
                break;
            }
            if (_lastChar == '(')
            {
                ParseBracketExpression();
            }
            else
            {
                var str = ParseString();
                if (_lastChar == '(')
                {
                    _sb.Append(FunctionHelper(str));
                    AddChar();
                    ParseParameters(str);

                }
                else
                {
                    _sb.Append(str);
                }
                if (_lastChar >= 0 && _lastChar != ')' && _lastChar != ',')
                {
                    ReadNonAlphanumericChar();
                }
                else { break; }
            }
        }
    }

    private void ParseParameters(string func)
    {
        if(func == "right")
        {
            _sb.Append("Reverse(");
            ParseExpression();
            _sb.Append(")");
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

    private string FunctionHelper(string function)
    {
        return function.ToLower() switch
        {
            "if" => "Iif",
            "sum" => "[].Sum",
            "date" => "GetDate",
            "left" => "Substring",
            "round" => "Round",
            "right" => "Reverse(Substring",
            _ => throw new Exception($"Unrecognized function: {function}")
        };
    }
}
