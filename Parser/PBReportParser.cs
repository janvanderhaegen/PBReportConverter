using System.Text;
using ReportMigration.Models;

namespace ReportMigration.Parser;

public enum Token
{
    OPBRACKET,
    CLBRACKET,
    EQUALS,
    WHITESPACE,
    NEWLINE,
    IDENTIFIER,
    EOF
}

internal class PBReportParser
{
    private static readonly char[] invalidIdentifierChars = { '(', ')', '=' };
    private readonly StreamReader _reader;
    private int _readerPosition = 0;
    private int _linePosition = 1;
    private char _lastChar;

    private string _stringval;
    private Token _lookahead;

    private List<Tuple<string, Dictionary<string, string>>> _structure = new List<Tuple<string, Dictionary<string, string>>>();

    public PBReportParser(string path)
    {
        _reader = new StreamReader(path);
        _stringval = "";
    }

    private char ReadChar()
    {
        _readerPosition++;
        return _lastChar = (char)_reader.Read();
    }

    public List<Tuple<string, Dictionary<string, string>>> GetStructure() { return _structure; }

    private void LookAhead()
    {
        if (Char.IsWhiteSpace(ReadChar()))
        {
            while (Char.IsWhiteSpace((char)_reader.Peek()))
            {
                if(ReadChar() == '\n')
                {
                    _linePosition++;
                    _readerPosition = 0;
                }
            }
            _lookahead = Token.WHITESPACE;
            return;
        }

        StringBuilder builder = new();

        switch (_lastChar)
        {
            case '"':
                {
                    while (ReadChar() != '"')
                    {
                        builder.Append(_lastChar);
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
            case '\uffff':
                {
                    _lookahead = Token.EOF;
                    return;
                }
        }
        builder.Append(_lastChar);
        while (!invalidIdentifierChars.Contains((char)this._reader.Peek()) && !Char.IsWhiteSpace((char)this._reader.Peek()))
        {
            builder.Append(ReadChar());
        }
        _stringval = builder.ToString();
        _lookahead = Token.IDENTIFIER;
        return;
    }

    public void Parse()
    {
        LookAhead();
        Line();
        LineRepeat();
    }

    private void Line()
    {
        if(_lookahead != Token.IDENTIFIER)
        {
            throw new ($"Expected alphanumeric string in line: {_linePosition} at position: {_readerPosition}, found character: {_lastChar}");
        }
        var key = _stringval;
        LookAhead();
        if (_lookahead != Token.OPBRACKET)
        {
            throw new ($"Expected '(' in line: {_linePosition} at position: {_readerPosition}, found character: {_lastChar}");
        }
        Args(key);
    }
    private void LineRepeat()
    {
        if (_lookahead == Token.WHITESPACE)
        {
            LookAhead();
            Line();
            LineRepeat();
        }
        if (_lookahead == Token.EOF)
        {
            return;
        }
        throw new ($"Expected newline separator or end of file in line: {_linePosition} at position: {_readerPosition}, found character: {_lastChar}");
    }

    private void Args(string key)
    {
        Dictionary<string, string> result = new Dictionary<string, string>();
        LookAhead();
        Arg(result);
        ArgRepeat(result);
        _structure.Add(new(key, result));
    }

    private void Arg(Dictionary<string, string> result)
    {
        if (_lookahead != Token.IDENTIFIER)
        {
            throw new ($"Expected alphanumeric string in line: {_linePosition} at position: {_readerPosition}, found character: {_lastChar}");
        }
        var key = _stringval;

        LookAhead();

        if(_lookahead == Token.WHITESPACE)
        {
            LookAhead();
        }

        if (_lookahead != Token.EQUALS)
        {
            throw new ($"Expected '=' in line: {_linePosition} at position: {_readerPosition}, found character: {_lastChar}");
        }

        LookAhead();

        if (_lookahead == Token.WHITESPACE)
        {
            LookAhead();
        }
        if (_lookahead != Token.IDENTIFIER)
        {
            throw new ($"Expected alphanumeric string in line: {_linePosition} at position: {_readerPosition}, found character: {_lastChar}");
        }

        LookAhead();

        result.Add(key, _stringval);
    }

    private void ArgRepeat(Dictionary<string, string> result)
    {
        if (_lookahead == Token.WHITESPACE)
        {
            LookAhead();
            Arg(result);
            ArgRepeat(result);
            return;
        }
        if (_lookahead == Token.CLBRACKET)
        {
            LookAhead();
            return;
        }
        throw new ($"Expected whitespace separator or ')' in line: {_linePosition} at position: {_readerPosition}, found character: {_lastChar}");
    }
}
