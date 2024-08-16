using System.Text;

namespace ReportMigration.Parser;

public enum Token
{
    OPBRACKET,
    CLBRACKET,
    EQUALS,
    SPACE,
    NEWLINE,
    IDENTIFIER,
    EOF
}

internal class PBReportParser
{
    private static readonly char[] invalidIdentifierChars = { '(', ')', '=', ' ' };
    private StringReader _reader;

    private string _stringval;
    private Token _lookahead;

    private List<Tuple<string, Dictionary<string, string>>> _structure = new List<Tuple<string, Dictionary<string, string>>>();

    public PBReportParser(string expression)
    {
        _reader = new StringReader(expression);
        _stringval = "";
    }
    private void LookAhead()
    {
        StringBuilder builder = new StringBuilder();
        var c = this._reader.Read();
        if (c == -1)
        {
            _lookahead = Token.EOF;
            return;
        }
        switch ((char) c)
        {
            case '"':
                {
                    while ((c = (char)this._reader.Read()) != '"')
                    {
                        builder.Append(c);
                    }
                    _stringval = builder.ToString();
                    _lookahead = Token.IDENTIFIER;
                    return;
                }
            case '(':
                {
                    _lookahead = Token.OPBRACKET;
                    return;
                }
            case ')': 
                {
                    _lookahead = Token.CLBRACKET;
                    return;
                }
            case '=':
                {
                    _lookahead = Token.EQUALS;
                    return;
                }
            case ' ':
                {
                    while((char) this._reader.Peek() == ' ')
                    {
                        this._reader.Read();
                    }
                    _lookahead = Token.SPACE;
                    return;
                }
            case '\n':
                {
                    _lookahead = Token.NEWLINE;
                    return;
                }

        }
        builder.Append((char) c);
        while (!invalidIdentifierChars.Contains((char)this._reader.Peek()))
        {
            builder.Append((char) this._reader.Read());
        }
        _stringval = builder.ToString();
        _lookahead = Token.IDENTIFIER;
        return;
    }

    public List<Tuple<string, Dictionary<string, string>>> Parse()
    {
        LookAhead();
        Line();
        LP();

        return _structure;
    }

    private void Line()
    {
        if(_lookahead != Token.IDENTIFIER)
        {
            throw new FormatException("Wrong format of input file, expected string");
        }
        var key = _stringval;
        LookAhead();
        if (_lookahead != Token.OPBRACKET)
        {
            throw new FormatException("Wrong format of input file, expected (");
        }
        var args = Args();
        _structure.Add(new(key, args));
    }

    private Dictionary<string, string> Args()
    {
        Dictionary<string, string> result = new Dictionary<string, string>();
        LookAhead();
        var arg = Arg();
        result.Add(arg.Key, arg.Value);
        AP(result);
        LookAhead();
        return result;
    }

    private (string Key, string Value) Arg()
    {
        if (_lookahead != Token.IDENTIFIER)
        {
            throw new FormatException("Wrong format of input file, expected string");
        }
        var key = _stringval;

        LookAhead();
        if (_lookahead != Token.EQUALS)
        {
            throw new FormatException("Wrong format of input file, expected =");
        }

        LookAhead();
        if (_lookahead != Token.IDENTIFIER)
        {
            throw new FormatException("Wrong format of input file, expected string");
        }

        LookAhead();

        return (key, _stringval);
    }

    private void AP(Dictionary<string, string> result)
    {
        if (_lookahead == Token.SPACE)
        {
            LookAhead();
            var arg = Arg();
            result.Add(arg.Key, arg.Value);
            AP(result);
        }
        if (_lookahead == Token.CLBRACKET) 
        {
            return;
        }
        throw new FormatException("Wrong format of input file, expected string or )");
    }

    private void LP()
    {
        if (_lookahead == Token.NEWLINE)
        {
            LookAhead();
            Line();
            LP();
        }
        if(_lookahead == Token.EOF)
        {
            return;
        }
        throw new FormatException("Wrong format of input file, expected \\n or EOF");
    }
}
