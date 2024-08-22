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
    private readonly StreamReader _reader = new StreamReader(path);
    private int _col = 1;
    private int _row = 0;
    private int _lastChar;
    private char _testChar;

    private List<ContainerModel> _structure = new List<ContainerModel>();

    private static string FormatChar(int c) => c < 32 ? $"\\x{c:X2}" : $"'{(char)c}'";

    private void ReadChar()
    {
        _lastChar = _reader.Read();
        _testChar = (char)_lastChar;
        if (_lastChar == '\n')
        {
            _row++;
            _col = 0;
        }
        else if (_lastChar >= 0)
        {
            _col++;
        }
    }

    private void ReadCharSkipWhitespace()
    {
        do ReadChar();
        while (_lastChar >= 0 && char.IsWhiteSpace((char)_lastChar));
    }

    public List<ContainerModel> GetStructure() { return _structure; }

    public void Parse()
    {
        ReadCharSkipWhitespace();
        ParseObject();
    }

    private void ParseObject()
    {
        var key = ParseIdentifier();

        var attributes = ParseAttributes(key);

        if (attributes.TryGetValue("band", out var band))
        {
            var container = FindContainerByName(band);
            if (container != null)
            {
                container._elements.Add(new(key, attributes));
            }
            else
            {
                throw new Exception($"Unexpected character {FormatChar(_lastChar)} at ({_row}, {_col}).");
            }
        }
        else
        {
            _structure.Add(new(key, attributes));
        }

        for (; ; )
        {
            ReadCharSkipWhitespace();
            if (_lastChar >= 0)
            {
                ParseObject();
            }
            else
            {
                return;
            }
        }
    }

    private string ParseString()
    {
        Span<char> buf = stackalloc char[8192];
        int pos = 0;

        if((char)_lastChar == '"')
        {
            ReadChar();
            while ((char)_lastChar != '"')
            {
                buf[pos++] = (char)_lastChar;
                ReadChar();
            }
            ReadChar();
            if (pos == 0)
            {
                return "";
            }
        }
        else
        {
            while(Char.IsAsciiLetterOrDigit((char) _lastChar) || _lastChar == '.' || _lastChar == '_')
            {
                buf[pos++] = (char)_lastChar;
                ReadChar();
            }

            if(_lastChar == '(')
            {
                ReadChar();
                var str = ParseString();

                str.AsSpan().CopyTo(buf);
                pos += str.Length;

                if (_lastChar != ')') 
                {
                    throw new Exception($"Unexpected character {FormatChar(_lastChar)} at ({_row}, {_col}).");
                }
                buf[pos++] = (char)_lastChar;
                ReadChar();
            }

            if (pos == 0) throw new Exception($"Unexpected character {FormatChar(_lastChar)} at ({_row}, {_col}).");
        }

        return buf[..pos].ToString();
    }

    private string ParseIdentifier()
    {
        Span<char> buf = stackalloc char[256];
        int pos = 0;

        while (Char.IsAsciiLetterOrDigit((char)_lastChar) || _lastChar == '.' || _lastChar == '_')
        {
            buf[pos++] = (char)_lastChar;
            ReadChar();
        }
        if (pos == 0) throw new Exception($"Unexpected character {FormatChar(_lastChar)} at ({_row}, {_col}).");

        return buf[..pos].ToString();
    }

    private Dictionary<string, object> ParseAttributes(string key)
    {
        Dictionary<string, object> attributes = new Dictionary<string, object>();
        if(key == "table")
        {
            attributes.Add("columns", new List<ObjectModel>());
        }
        if ((char)_lastChar != '(')
        {
            throw new Exception($"Unexpected character {FormatChar(_lastChar)} at ({_row}, {_col}).");
        }

        for(;;)
        {
            if(_lastChar == ')')
            {
                ReadChar();
                break;
            }
            var attr = ParseAttribute();
            if(attr.key == "column")
            {
                (attributes["columns"] as List<ObjectModel>)!.Add(new(attr.key, (attr.value as Dictionary<string, object>)!));
            }else
            {
                attributes.Add(attr.key, attr.value);
            }
        }

        return attributes;
    }

    private (string key, object value) ParseAttribute()
    {
        ReadCharSkipWhitespace();
        var key = ParseIdentifier();

        if (Char.IsWhiteSpace((char)_lastChar))
        {
            ReadCharSkipWhitespace();
        }

        if (_lastChar != '=')
        {
            throw new Exception($"Unexpected character {FormatChar(_lastChar)} at ({_row}, {_col})."); 
        } 
        ReadCharSkipWhitespace();

        if (_lastChar == '(')
        {
            if (key == "column")
            {
                return (key, ParseAttributes(key));
            }
            ReadCharSkipWhitespace();
            return (key, ParseList());
        }
        else
        {
            return (key, ParseString());
        }

    }

    private List<object> ParseList()
    {
        var result = new List<object>();
        for (;;)
        {
            if (_lastChar == ')'){
                ReadChar();
                break;
            }
            if (Char.IsWhiteSpace((char)_lastChar))
            {
                ReadCharSkipWhitespace();
            }
            if(_lastChar == '(')
            {
                ReadChar();
                result.Add(ParseList());
            }
            else if (_lastChar == ',')
            {
                ReadCharSkipWhitespace();
            }else
            {
                result.Add(ParseString());
            }
        }
        return result;
    }

    private ContainerModel? FindContainerByName(object band)
    {
        if (band.GetType() != typeof(string))
        {
            throw new Exception($"Unexpected character {FormatChar(_lastChar)} at ({_row}, {_col}).");
        }
        var name = (string)band;
        foreach (var container in _structure)
        {
            if (name == container._objectType)
            {
                return container;
            }
            if (container._objectType == "group" && name.Split('.')[1] == (string)container._attributes["level"])
            {
                return container;
            }
        }
        return null;
    }
}
