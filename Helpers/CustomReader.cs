namespace PBReportConverter.Helpers;
public class CustomReader(string filePath)
{
    private readonly string[] _lines = File.ReadAllLines(filePath).Select(x => x.Replace("~\"", "'")).ToArray();
    private int _currentLineIndex = 0;
    private int _currentCharIndex = 0;

    public int Peek()
    {
        if (_currentLineIndex >= _lines.Length)
            return -1;

        if (_currentCharIndex >= _lines[_currentLineIndex].Length)
        {
            if (_currentLineIndex == _lines.Length - 1)
                return -1;
            return '\n';
        }

        return _lines[_currentLineIndex][_currentCharIndex];
    }

    public int Read()
    {
        int peekedChar = Peek();
        if (peekedChar != -1)
        {
            if (peekedChar == '\n')
            {
                _currentLineIndex++;
                _currentCharIndex = 0;
            }
            else
            {
                _currentCharIndex++;
            }
        }
        return peekedChar;
    }

    public string? ReadLine()
    {
        if (_currentLineIndex >= _lines.Length)
            return null;

        string remainingLine = _lines[_currentLineIndex][_currentCharIndex..];
        _currentLineIndex++;
        _currentCharIndex = 0;
        return remainingLine;
    }

    public string Peek(int charCount)
    {
        if (_currentLineIndex >= _lines.Length)
            return string.Empty;

        string result = "";
        int lineIndex = _currentLineIndex;
        int charIndex = _currentCharIndex;

        while (result.Length < charCount && lineIndex < _lines.Length)
        {
            if (charIndex < _lines[lineIndex].Length)
            {
                result += _lines[lineIndex][charIndex];
                charIndex++;
            }
            else
            {
                if (lineIndex < _lines.Length - 1)
                {
                    result += "\n";
                    lineIndex++;
                    charIndex = 0;
                }
                else
                {
                    break;
                }
            }
        }

        return result;
    }
}