using System.Text;
using DevExpress.Data.Async.Helpers;
using ReportMigration.Models;

namespace ReportMigration.Parser;

public enum Token
{
    OPBRACKET,
    CLBRACKET,
    EQUALS,
    COMMA,
    WHITESPACE,
    NEWLINE,
    IDENTIFIER,
    EOF
}

internal class PBReportParser(string path)
{
    private static readonly char[] invalidIdentifierChars = { '(', ')', '=' };
    private readonly StreamReader _reader = new StreamReader(path);
    private int _colPosition = 1;
    private int _linePosition = 0;
    private char _lastChar;

    private string _stringval = "";
    private Token _lookahead;

    private List<ContainerModel> _structure = new List<ContainerModel>();

    private char ReadChar()
    {
        _colPosition++;
        return _lastChar = (char)_reader.Read();
    }

    private void ReadCharSkipWhitespace()
    {
        do ReadChar();
        while (_lastChar >= 0 && char.IsWhiteSpace((char)_lastChar));
    }

    public List<ContainerModel> GetStructure() { return _structure; }

    private void LookAhead()
    {
        if (Char.IsWhiteSpace(ReadChar()))
        {
            while (Char.IsWhiteSpace((char)_reader.Peek()))
            {
                if (ReadChar() == '\n')
                {
                    _linePosition++;
                    _colPosition = 0;
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
            case ',':
                {
                    _lookahead = Token.COMMA;
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

    public string ParseString()
    {
        Span<char> buf = stackalloc char[32];
        int pos = 0;
        while (!invalidIdentifierChars.Contains((char)_lastChar) && !Char.IsWhiteSpace((char)_lastChar))
        {
            buf[pos++] = (char)_lastChar;
            ReadChar();
        }
        if (pos == 0) throw new Exception($"Unexpected character {FormatChar(_lastChar)} at ({_linePosition}, {_colPosition}).");

        var result = buf[..pos].ToString();

        return result;
    }

    public void Parse()
    {
        LookAhead();
        Line();
        LineRepeat();
    }

    private void Line()
    {
        if (_lookahead != Token.IDENTIFIER)
        {
            throw new($"Expected alphanumeric string in line: {_linePosition} at position: {_colPosition}, found character: {_lastChar}");
        }
        var key = _stringval;
        LookAhead();
        Attributes(key);
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
        throw new ($"Expected newline separator or end of file in line: {_linePosition} at position: {_colPosition}, found character: {_lastChar}");
    }

    private void Attributes(string key)
    {
        if (_lookahead != Token.OPBRACKET)
        {
            throw new($"Expected '(' in line: {_linePosition} at position: {_colPosition}, found character: {_lastChar}");
        }
        Dictionary<string, object> attributes = new Dictionary<string, object>();
        LookAhead();
        Attr(attributes);
        AttrRepeat(attributes);
        if (attributes.TryGetValue("band", out var band))
        {
            var container = FindContainerByName(band);
            if (container != null)
            {
                container.Elements.Add(new(key) { Attributes = attributes });
            }
            else
            {
                throw new($"No container defined with the name: {band}");
            }
        }
        else 
        {
            _structure.Add(new(key) { Attributes = attributes });   
        }
    }

    private void Attr(Dictionary<string, object> attributes)
    {
        if (_lookahead != Token.IDENTIFIER)
        {
            throw new ($"Expected alphanumeric string in line: {_linePosition} at position: {_colPosition}, found character: {_lastChar}");
        }
        var key = _stringval;

        LookAhead();

        if(_lookahead == Token.WHITESPACE)
        {
            LookAhead();
        }

        if (_lookahead != Token.EQUALS)
        {
            throw new ($"Expected '=' in line: {_linePosition} at position: {_colPosition}, found character: {_lastChar}");
        }
        LookAhead();

        if (_lookahead == Token.WHITESPACE)
        {
            LookAhead();
        }

        if (_lookahead == Token.IDENTIFIER)
        {
            LookAhead();

            attributes.Add(key, _stringval);
            return;
        }
        else if (_lookahead == Token.OPBRACKET) 
        {
            LookAhead();
            attributes.Add(key, StringList());
            return;
        }
        throw new($"Expected alphanumeric string in line: {_linePosition} at position: {_colPosition}, found character: {_lastChar}");
    }

    private List<string> StringList()
    {
        if (_lookahead != Token.IDENTIFIER)
        {
            throw new($"Expected alphanumeric string in line: {_linePosition} at position: {_colPosition}, found character: {_lastChar}");
        }
        var result = new List<string>() { _stringval };
        LookAhead();
        result.AddRange(ListRepeat());
        return result;
    }

    private List<string> ListRepeat()
    {
        if (_lookahead == Token.WHITESPACE)
        {
            LookAhead();
        }

        if (_lookahead == Token.COMMA)
        {
            LookAhead();
            if (_lookahead == Token.WHITESPACE)
            {
                LookAhead();
            }
            if (_lookahead != Token.IDENTIFIER)
            {
                throw new($"Expected alphanumeric string in line: {_linePosition} at position: {_colPosition}, found character: {_lastChar}");
            }
            var result = new List<string>() { _stringval };
            LookAhead();
            result.AddRange(ListRepeat());
            return result;
        }
        if (_lookahead == Token.CLBRACKET)
        {
            LookAhead();
            return new List<string>();
        }
        throw new($"Expected ',' or ')' in line: {_linePosition} at position: {_colPosition}, found character: {_lastChar}");
    }

    private void AttrRepeat(Dictionary<string, object> attributes)
    {
        if (_lookahead == Token.WHITESPACE)
        {
            LookAhead();
            Attr(attributes);
            AttrRepeat(attributes);
            return;
        }
        if (_lookahead == Token.CLBRACKET)
        {
            LookAhead();
            return;
        }
        throw new ($"Expected whitespace separator or ')' in line: {_linePosition} at position: {_colPosition}, found character: {_lastChar}");
    }

    private ContainerModel? FindContainerByName(object band)
    {
        if (band.GetType() != typeof(string))
        {
            throw new("Value of attribute 'band' is not of type 'string'");
        }
        var name = (string)band;
        foreach (var container in _structure)
        {
            if(name == container._objectType)
            {
                return container;
            }
            if (container._objectType == "group" && name.Split('.')[1] == (string)container.Attributes["level"])
            {
                return container;
            }
        }
        return null;
    }
}
