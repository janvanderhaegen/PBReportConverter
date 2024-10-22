using PBReportConverter.Helpers;
using PBReportConverter.Models;
using static PBReportConverter.Helpers.PBFormattingHelper;

namespace PBReportConverter.Parser;

internal class PBReportParser(string path)
{
    private readonly CustomReader _reader = new(path);
    private readonly string _filePath = path;
    private int _lastChar;
    private char _lastCharAsChar;
    private readonly List<ContainerModel> _structure = [];
    public double ReportHeight { get; private set; } = 0;
    public double ReportWidth { get; private set; } = 0;
    public int GroupCount { get; private set; } = 0;

    public double _horizontalMarginSum = 0;
    public double _verticalMarginSum = 0;

    private static string FormatChar(char c) => ((int)c) < 32 ? $"\\x{(int)c:X2}" : $"'{c}'";

    private void ReadChar()
    {
        _lastChar = _reader.Read();
        _lastCharAsChar = (char)_lastChar;
    }

    private void ReadCharSkipWhitespace()
    { 
        do ReadChar();
        while (_lastChar >= 0 && char.IsWhiteSpace(_lastCharAsChar));
    }

    public List<ContainerModel> Parse()
    {
        ReadCharSkipWhitespace();

        for (; ; )
        {
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
    private void SkipRestOfLine(string identifier)
    {
        var skippedThisLine = identifier + _reader.ReadLine();
        ReadCharSkipWhitespace();
    }
    private void ParseObject()
    {
        if (!Char.IsAsciiLetterOrDigit(_lastCharAsChar))
        {
            SkipRestOfLine(_lastCharAsChar.ToString());
            return;
        }

        var key = ParseIdentifier();

        if (_lastCharAsChar != '(')
        {
            SkipRestOfLine(key + ' ' + _lastCharAsChar);
            return;
        }

        if (key == "table")
        {
            ParseTable();
            return;
        }

        var attributes = ParseAttributes();

        if (key == "datawindow")
        {
            uom = int.Parse(attributes["units"]);
            _verticalMarginSum = Y(attributes["print.margin.top"]) + Y(attributes["print.margin.bottom"]);
            _horizontalMarginSum = X(attributes["print.margin.left"]) + X(attributes["print.margin.right"]);
        }
        else if (key == "group")
        {
            GroupCount++;
        }

        if (attributes.TryGetValue("band", out var band))
        {
            var container = FindContainerByName(band);
            if (container == null)
            {
                container = new(band, new() { { "height", "0" } });
                container._elements.Add(new(key, attributes));
                _structure.Add(container);
            }
            container._elements.Add(new(key, attributes));
            if (container._objectType == "background") return;

            double height;
            if (band.Contains("header."))
            {
                height = double.Parse(container._attributes["header.height"]);
            }
            else if (band.Contains("trailer."))
            {
                height = double.Parse(container._attributes["trailer.height"]);
            }
            else
            {
                height = double.Parse(container._attributes["height"]);
            }

            if (attributes.TryGetValue("x", out var x) && height > 0)
            {
                var xnum = X(x);
                var width = X(attributes["width"]);
                if (xnum + width + 3 > ReportWidth)
                {
                    ReportWidth = xnum + width + 3;
                }
            }
        }
        else
        {
            _structure.Add(new(key, attributes));
            if (attributes.TryGetValue("height", out var height))
            {
                ReportHeight += Y(height);
            }
            else if (attributes.TryGetValue("header.height", out height))
            {
                ReportHeight += Y(height);
                ReportHeight += Y(attributes["trailer.height"]);
            }
        }
    }

    public void ParseTable()
    {
        if (_lastCharAsChar != '(')
        {
            throw new Exception($"Unexpected character while parsing table start: {FormatChar(_lastCharAsChar)} at ({_reader.CurrentLineIndex()}, {_reader.CurrentCharIndex()}) in file {_filePath}.");
        }

        var columns = new List<ObjectModel>();
        var attributes = new Dictionary<string, string>();
        ReadCharSkipWhitespace();
        for (; ; )
        {
            if (_lastChar == ')')
            {
                ReadChar();
                break;
            }

            var identifier = ParseIdentifier();

            if (_lastChar != '=')
            {
                throw new Exception($"Unexpected character while parsing table: {FormatChar(_lastCharAsChar)} at ({_reader.CurrentLineIndex()}, {_reader.CurrentCharIndex()}) in file {_filePath}.");
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

        ReadCharSkipWhitespace();
        _structure.Add(new TableModel("table", attributes, columns));
    }

    private Dictionary<string, string> ParseAttributes()
    {
        Dictionary<string, string> attributes = [];

        if (_lastCharAsChar != '(')
        {
            throw new Exception($"Unexpected character while parsing attributes: {FormatChar(_lastCharAsChar)} at ({_reader.CurrentLineIndex()}, {_reader.CurrentCharIndex()}) in file {_filePath}.");
        }
        ReadCharSkipWhitespace();
        for (; ; )
        {
            if (_lastChar == ')')
            {
                ReadChar();
                break;
            }
            var (key, value) = ParseAttribute();
            attributes.Add(key, value);
        }

        ReadCharSkipWhitespace();
        return attributes;
    }

    private (string key, string value) ParseAttribute()
    {
        var key = ParseIdentifier();

        if (_lastChar != '=')
        {
            throw new Exception($"Unexpected character while parsing attribute: {FormatChar(_lastCharAsChar)} at ({_reader.CurrentLineIndex()}, {_reader.CurrentCharIndex()}) in file {_filePath}.");
        }
        ReadCharSkipWhitespace();

        var value = ParseString();

        return (key, value);
    }

    private string ParseString()
    {
        Span<char> buf = stackalloc char[8192];
        int pos = 0;


        if (_lastCharAsChar == '"' || _lastCharAsChar == '\'')
        {
            var initialOpenStringChar = _lastCharAsChar;
            ReadChar();
            while (_lastCharAsChar != initialOpenStringChar )
            {
                buf[pos++] = _lastCharAsChar;
                ReadChar();
            }
            ReadChar();
        } 
        else
        { 
            while (Char.IsAsciiLetterOrDigit(_lastCharAsChar) || _lastChar == '.' || _lastChar == '_')
            {
                buf[pos++] = _lastCharAsChar;
                ReadChar();
            }
            if (_lastChar == '(')
            {
                buf[pos++] = _lastCharAsChar;
                ReadChar();
                var str = ParseString();
                str.AsSpan().CopyTo(buf.Slice(pos, str.Length));
                pos += str.Length;

                if (_lastChar != ')')
                {
                    throw new Exception($"Unexpected character while parsing string: {FormatChar(_lastCharAsChar)} at ({_reader.CurrentLineIndex()}, {_reader.CurrentCharIndex()}) in file {_filePath}.");
                }
                buf[pos++] = _lastCharAsChar;
                ReadChar();
            }

            if (pos == 0) throw new Exception($"Unexpected character while parsing string (end): {FormatChar(_lastCharAsChar)} at ({_reader.CurrentLineIndex()}, {_reader.CurrentCharIndex()}) in file {_filePath}.");
        }

        if (Char.IsWhiteSpace(_lastCharAsChar))
        {
            ReadCharSkipWhitespace();
        }

        if (_lastChar == ',')
        {
            buf[pos++] = _lastCharAsChar;
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

        while (Char.IsAsciiLetterOrDigit(_lastCharAsChar) || _lastChar == '.' || _lastChar == '_')
        {
            buf[pos++] = _lastCharAsChar;
            ReadChar();
        }
        if (pos == 0) throw new Exception($"Unexpected character while parsing identifier: {FormatChar(_lastCharAsChar)} at ({_reader.CurrentLineIndex()}, {_reader.CurrentCharIndex()}) in file {_filePath}.");

        if (Char.IsWhiteSpace(_lastCharAsChar))
        {
            ReadCharSkipWhitespace();
        }

        return buf[..pos].ToString();
    }

    private string ParseSqlQuery()
    {
        var start = $"({_reader.CurrentLineIndex()}, {_reader.CurrentCharIndex()})";
        int bufferSize = 50000;
        Span<char> buf = stackalloc char[bufferSize];
        int pos = 0;


        if (_lastChar == '"')
        {
            int bracketCheck = 0;
            ReadChar();
            for (; ; )
            {
                if (pos + 1 == bufferSize)
                {
                    throw new Exception($"Buffer overflow while parsing SQL query that started at {start} to ({_reader.CurrentLineIndex()}, {_reader.CurrentCharIndex()}) in file {_filePath}.");
                }
                if (_lastCharAsChar == '"' && Char.IsWhiteSpace((char)_reader.Peek()))
                {
                    ReadChar();
                    var nextKeyword = _reader.Peek(9);
                    if (nextKeyword == "arguments" || (nextKeyword.StartsWith(')') && bracketCheck==0)) //after the last quote, there should be either "arguments = ... )" or if there are no arguments: ")"
                    {
                        break;
                    }
                    else
                    {
                        buf[pos++] = '\'';
                        continue;
                    }
                }
                if(_lastCharAsChar == '(')
                {
                    bracketCheck++;
                }
                else if(_lastCharAsChar == ')')
                {
                    bracketCheck--;
                }
                else if(_lastCharAsChar == '/' && _reader.Peek() == '/')
                {
                    SkipRestOfLine(_lastCharAsChar.ToString());
                    continue;
                }
                buf[pos++] = _lastCharAsChar == '"' ? '\'' : _lastCharAsChar;
                ReadChar();
            }
            if (pos == 0)
            {
                throw new Exception($"Unexpected character while parsing SQL query that started at {start}: {FormatChar(_lastCharAsChar)} at ({_reader.CurrentLineIndex()}, {_reader.CurrentCharIndex()}) in file {_filePath}.");
            }
        }

        if (Char.IsWhiteSpace(_lastCharAsChar))
        {
            ReadCharSkipWhitespace();
        }

        return buf[..pos].ToString().Replace("<", "&lt;").Replace(">", "&gt;");
    }

    private ContainerModel? FindContainerByName(string name)
    {
        foreach (var container in _structure)
        {
            if (name == container._objectType)
            {
                return container;
            }
            if (container._objectType == "group")
            {
                var nameSplit = name.Split('.');
                if(nameSplit.Length > 1 && nameSplit[1] == container._attributes["level"])
                {
                    return container;
                }
            }
        }
        return null;
    }
}

