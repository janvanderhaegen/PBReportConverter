using ReportMigration.Models;
using ReportMigration.Helpers;

namespace ReportMigration.Parser;

internal class PBReportParser(string path)
{
    private readonly StreamReader _reader = new(path);
    private readonly string _filePath = path;
    private int _col = 1;
    private int _row = 0;
    private int _lastChar;
    private char _testChar;
    private readonly List<ContainerModel> _structure = [];
    public double ReportHeight { get; private set; } = 0;
    public double ReportWidth { get; private set; } = 0;
    public int GroupCount { get; private set; } = 0;

    public double _horizontalMarginSum = 0;
    public double _verticalMarginSum = 0;

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

    public List<ContainerModel> Parse()
    {
        // Skips release number
        _reader.ReadLine();
        ReadCharSkipWhitespace();

        for (;;)
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

    private void ParseObject()
    {
        var key = ParseIdentifier();

        if (key == "table")
        {
            ParseTable();
            return;
        }

        var attributes = ParseAttributes();

        if (key == "datawindow")
        {
            _verticalMarginSum = PBFormattingHelper.ConvertY(double.Parse(attributes["print.margin.top"])) + PBFormattingHelper.ConvertY(double.Parse(attributes["print.margin.bottom"]));
            _horizontalMarginSum = PBFormattingHelper.ConvertX(double.Parse(attributes["print.margin.left"])) + PBFormattingHelper.ConvertX(double.Parse(attributes["print.margin.right"]));
        }
        else if (key == "group")
        {
            GroupCount++;
        }

        if (attributes.TryGetValue("band", out var band))
        {
            var container = FindContainerByName(band);
            if (container != null)
            {
                container._elements.Add(new(key, attributes));
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
                    var xnum = PBFormattingHelper.ConvertX(double.Parse(x));
                    var width = PBFormattingHelper.ConvertX(double.Parse(attributes["width"]));
                    if (xnum + width > ReportWidth)
                    {
                        ReportWidth = xnum + width;
                    }
                }
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
                ReportHeight += PBFormattingHelper.ConvertY(double.Parse(height));
            }
            else if(attributes.TryGetValue("header.height", out height))
            {
                ReportHeight += PBFormattingHelper.ConvertY(double.Parse(height));
                ReportHeight += PBFormattingHelper.ConvertY(double.Parse(attributes["trailer.height"]));
            }
        }
    }

    public void ParseTable()
    {
        if ((char)_lastChar != '(')
        {
            throw new Exception($"Unexpected character {FormatChar(_lastChar)} at ({_row}, {_col}) in file {_filePath}.");
        }

        var columns = new List<ObjectModel>();
        var attributes = new Dictionary<string, string>();
        ReadCharSkipWhitespace();
        for (;;)
        {
            if (_lastChar == ')')
            {
                ReadChar();
                break;
            }

            var identifier = ParseIdentifier();

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

        ReadCharSkipWhitespace();
        _structure.Add(new TableModel("table", attributes, columns));
    }

    private Dictionary<string, string> ParseAttributes()
    {
        Dictionary<string, string> attributes = [];
        
        if ((char)_lastChar != '(')
        {
            throw new Exception($"Unexpected character {FormatChar(_lastChar)} at ({_row}, {_col}) in file {_filePath}.");
        }
        ReadCharSkipWhitespace();
        for (;;)
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
            throw new Exception($"Unexpected character {FormatChar(_lastChar)} at ({_row}, {_col}) in file {_filePath}.");
        } 
        ReadCharSkipWhitespace();

        var value = ParseString();

        return (key, value);
    }

    private string ParseString()
    {
        Span<char> buf = stackalloc char[8192];
        int pos = 0;

        if (_lastChar == '"')
        {
            ReadChar();
            while ((char)_lastChar != '"')
            {
                buf[pos++] = (char)_lastChar;
                ReadChar();
            }
            ReadChar();
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

        if (Char.IsWhiteSpace((char)_lastChar))
        {
            ReadCharSkipWhitespace();
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

        if (Char.IsWhiteSpace((char)_lastChar))
        {
            ReadCharSkipWhitespace();
        }

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
                    if (_reader.Peek() == 'a')
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

        if (Char.IsWhiteSpace((char)_lastChar))
        {
            ReadCharSkipWhitespace();
        }

        return buf[..pos].ToString().Replace("<", "&lt;").Replace(">", "&gt;");
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
