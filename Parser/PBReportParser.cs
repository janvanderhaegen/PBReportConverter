using ReportMigration.Models;

namespace ReportMigration.Parser;

internal class PBReportParser(string path)
{
    private readonly StreamReader _reader = new(path);
    private readonly string _filePath = path;
    private int _col = 1;
    private int _row = 0;
    private int _lastChar;
    public int reportHeight = 0;
    public int reportWidth = 0;
    public int groupCount = 0;

    private readonly List<ContainerModel> _structure = [];

    private static string FormatChar(int c) => c < 32 ? $"\\x{c:X2}" : $"'{(char)c}'";

    private void ReadChar()
    {
        _lastChar = _reader.Read();

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

    //public List<ContainerModel> GetStructure() { return _structure; }

    public List<ContainerModel> Parse()
    {
        for (;;)
        {
            ReadCharSkipWhitespace();
            if (_lastChar >= 0)
            {
                ParseObject();
            }
            else
            {
                return _structure;
            }
        }
    }

    private void ParseObject()
    {
        var key = ParseIdentifier();

        if (key == "table")
        {
            ParseTable(key);
            return;
        }

        if (key == "group")
        {
            groupCount++;
        }

        var attributes = ParseAttributes();

        if (attributes.TryGetValue("band", out var band))
        {
            var container = FindContainerByName(band);
            if (container != null)
            {
                container._elements.Add(new(key, attributes));
                //if(attributes.TryGetValue("x", out var x))
                //{
                    //var xint = Int32.Parse((string)x);
                    //var width = Int32.Parse((string)attributes["width"]);
                    ////var width = attributes["width"];
                    //if (xint + width > reportWidth)
                    //{
                    //    reportWidth = xint + width;
                    //}
                //}
            }
            else
            {
                throw new Exception($"Unexpected character {FormatChar(_lastChar)} at ({_row}, {_col}) in file {_filePath}.");
            }
        }
        else
        {
            _structure.Add(new(key, attributes));
            if(attributes.TryGetValue("height", out var height))
            {
                reportHeight += Int32.Parse(height);
            }
            else if(attributes.TryGetValue("header.height", out height))
            {
                reportHeight += Int32.Parse(height);
                reportHeight += Int32.Parse(attributes["trailer.height"]);
            }
        }
    }

    public void ParseTable(string key)
    {
        if ((char)_lastChar != '(')
        {
            throw new Exception($"Unexpected character {FormatChar(_lastChar)} at ({_row}, {_col}) in file {_filePath}.");
        }

        var columns = new List<ObjectModel>();
        var attributes = new Dictionary<string, string>();
        for (;;)
        {
            if (_lastChar == ')')
            {
                ReadChar();
                break;
            }
            ReadCharSkipWhitespace();

            var identifier = ParseIdentifier();

            if (Char.IsWhiteSpace((char)_lastChar))
            {
                ReadCharSkipWhitespace();
            }

            if (_lastChar != '=')
            {
                throw new Exception($"Unexpected character {FormatChar(_lastChar)} at ({_row}, {_col}) in file {_filePath}.");
            }
            ReadCharSkipWhitespace();
            if (identifier == "column")
            {
                columns.Add(new(identifier, ParseAttributes()));
            }
            else if (identifier == "retrieve")
            {
                attributes.Add(identifier, ParseSqlQuery());
            }
            else
            {
                attributes.Add(identifier, ParseString());
            }
        }

        _structure.Add(new TableModel(key, attributes, columns));
    }

    private string ParseString()
    {
        Span<char> buf = stackalloc char[8192];
        int pos = 0;

        if(_lastChar == '"')
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

            while (Char.IsAsciiLetterOrDigit((char)_lastChar) || _lastChar == '.' || _lastChar == '_')
            {
                buf[pos++] = (char)_lastChar;
                ReadChar();
            }
            if (_lastChar == '(')
            {
                buf[pos++] = (char)_lastChar;
                ReadChar();
                var str = ParseString();
                str.AsSpan().CopyTo(buf.Slice(pos, str.Length));
                pos += str.Length;

                if (_lastChar != ')') 
                {
                    throw new Exception($"Unexpected character {FormatChar(_lastChar)} at ({_row}, {_col}) in file {_filePath}.");
                }
                buf[pos++] = (char)_lastChar;
                ReadChar();
            }

            if (pos == 0) throw new Exception($"Unexpected character {FormatChar(_lastChar)} at ({_row}, {_col}) in file {_filePath}.");
        }

        if (_lastChar == ',')
        {
            buf[pos++] = (char)_lastChar;
            ReadCharSkipWhitespace();
            var str = ParseString();
            str.AsSpan().CopyTo(buf.Slice(pos, str.Length));
            pos += str.Length;
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
        if (pos == 0) throw new Exception($"Unexpected character {FormatChar(_lastChar)} at ({_row}, {_col}) in file {_filePath}.");

        return buf[..pos].ToString();
    }

    private string ParseSqlQuery()
    {
        Span<char> buf = stackalloc char[50000];
        int pos = 0;

        if (_lastChar == '"')
        {
            ReadChar();
            for (; ; )
            {
                if (_lastChar == '"' && Char.IsWhiteSpace((char)_reader.Peek()))
                {
                    ReadChar();
                    if(_reader.Peek() == 'a') 
                    { 
                        break;
                    }
                    else
                    {
                        buf[pos++] = '"';
                    }
                }
                buf[pos++] = (char)_lastChar;
                ReadChar();
            }
            if (pos == 0)
            {
                throw new Exception($"Unexpected character {FormatChar(_lastChar)} at ({_row}, {_col}) in file {_filePath}.");
            }
        }

        return buf[..pos].ToString().Replace("<", "&lt;").Replace(">", "&gt;");
    }

    private Dictionary<string, string> ParseAttributes()
    {
        Dictionary<string, string> attributes = [];
        
        if ((char)_lastChar != '(')
        {
            throw new Exception($"Unexpected character {FormatChar(_lastChar)} at ({_row}, {_col}) in file {_filePath}.");
        }

        for (;;)
        {
            if(_lastChar == ')')
            {
                ReadChar();
                break;
            }
            var (key, value) = ParseAttribute();
            attributes.Add(key, value);
        }

        return attributes;
    }

    private (string key, string value) ParseAttribute()
    {
        ReadCharSkipWhitespace();
        var key = ParseIdentifier();

        if (Char.IsWhiteSpace((char)_lastChar))
        {
            ReadCharSkipWhitespace();
        }

        if (_lastChar != '=')
        {
            throw new Exception($"Unexpected character {FormatChar(_lastChar)} at ({_row}, {_col}) in file {_filePath}.");
        } 
        ReadCharSkipWhitespace();

        return (key, ParseString());
    }

    private ContainerModel? FindContainerByName(object band)
    {
        if (band.GetType() != typeof(string))
        {
            throw new Exception($"Unexpected character {FormatChar(_lastChar)} at ({_row}, {_col}) in file {_filePath}.");
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
