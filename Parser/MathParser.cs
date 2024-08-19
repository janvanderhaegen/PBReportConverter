using System.Text;

namespace ReportMigration.Parser;

internal class MathParser
{
    private readonly StringReader _reader;
    private int _readerPosition = -1;
    private char _lastChar;

    private double _numval;
    private TokenType _lookahead;
    public enum TokenType
    {
        PLUS,
        MINUS,
        MULTI,
        DIV,
        EXP,
        NUM,
        OBRACKET,
        CBRACKET,
        EOF
    }

    public MathParser(string expression)
    {
        _reader = new StringReader(expression);
    }

    private char ReadChar()
    {
        _readerPosition++;
        return _lastChar = (char)_reader.Read();
    }

    private void LookAhead()
    {
        ReadChar();

        while (Char.IsWhiteSpace(_lastChar))
        {
            ReadChar();
        }

        switch (_lastChar)
        {
            case '(':
                {
                    _lookahead = TokenType.OBRACKET;
                    return;
                }
            case ')':
                {
                    _lookahead = TokenType.CBRACKET;
                    return;
                }
            case '+':
                {
                    _lookahead = TokenType.PLUS;
                    return;
                }
            case '-':
                {
                    _lookahead = TokenType.MINUS;
                    return;
                }
            case '*':
                {
                    _lookahead = TokenType.MULTI;
                    return;
                }
            case '/':
                {
                    _lookahead = TokenType.DIV;
                    return;
                }
            case '^':
                {
                    _lookahead = TokenType.EXP;
                    return;
                }
            case '\uffff':
                {
                    _lookahead = TokenType.EOF;
                    return;
                }
        }

        StringBuilder builder = new StringBuilder();
        if ('0' <= _lastChar && _lastChar <= '9')
        {
            builder.Append(_lastChar);
            while ('0' <= (char)_reader.Peek() && (char)_reader.Peek() <= '9')
            {
                builder.Append(ReadChar());
            }

            if ((char) _reader.Peek() == '.')
            {
                builder.Append(ReadChar());
                while ('0' <= (char)_reader.Peek() && (char)_reader.Peek() <= '9')
                {
                    builder.Append(ReadChar());
                }
            }

            if (double.TryParse(builder.ToString(), out _numval))
            {
                _lookahead = TokenType.NUM;
                return;
            }
        }

        throw new Exception($"Unexpected character: {_lastChar} at position: {_readerPosition}");
    }

    public double Parse()
    {
        LookAhead();

        return AddExpression();
    }

    private double AddExpression()
    {
        if (_lookahead == TokenType.MINUS || _lookahead == TokenType.OBRACKET || _lookahead == TokenType.NUM) 
        {
            return MultiplyExpression() + RepeatAddExpression();
        }
        throw new Exception($"Expected - or ( or num at position: {_readerPosition}, found character: {_lastChar}");
    }

    private double RepeatAddExpression()
    {
        if (_lookahead == TokenType.PLUS)
        {
            LookAhead();
            return MultiplyExpression() + RepeatAddExpression(); 
        }
        else if (_lookahead == TokenType.MINUS)
        {
            LookAhead();
            return -(MultiplyExpression() + RepeatAddExpression());
        }
        else if (_lookahead == TokenType.CBRACKET || _lookahead == TokenType.EOF)
        {
            return 0;
        }
        throw new Exception($"Expected + or - or end of expression at position: {_readerPosition}, found character: {_lastChar}");
    }

    private double MultiplyExpression()
    {
        if (_lookahead == TokenType.MINUS || _lookahead == TokenType.OBRACKET || _lookahead == TokenType.NUM)
        {
            return NegativeExpression() * RepeatMultiplyExpression();
        }
        throw new Exception($"Expected - or ( or num at position: {_readerPosition}, found character: {_lastChar}");
    }

    private double RepeatMultiplyExpression()
    {
       if (_lookahead == TokenType.MULTI)
        {
            LookAhead();
            return NegativeExpression() * RepeatMultiplyExpression();
        }
        else if (_lookahead == TokenType.DIV)
        {
            LookAhead();
            return 1 / (NegativeExpression() * RepeatMultiplyExpression());
        }
        else if (_lookahead == TokenType.PLUS || _lookahead == TokenType.MINUS || _lookahead == TokenType.CBRACKET || _lookahead == TokenType.EOF)
        {
            return 1;
        }
        throw new Exception($"Expected * or / or end of expression at position: {_readerPosition}, found character: {_lastChar}");
    }

    private double NegativeExpression()
    {
        if (_lookahead == TokenType.MINUS)
        {
            LookAhead();
            return -NegativeExpression();
        }
        else if (_lookahead == TokenType.OBRACKET || _lookahead == TokenType.NUM)
        {
            return ExponentExpression();
        }
        throw new Exception($"Expected - or ( or num at position: {_readerPosition}, found character: {_lastChar}");
    }

    private double ExponentExpression()
    {
        if (_lookahead == TokenType.OBRACKET || _lookahead == TokenType.NUM)
        {
            return Math.Pow(BracketOrNumExpression(), ExponentRepeatExpression());
        }
        throw new Exception($"Expected ( or num at position: {_readerPosition}, found character: {_lastChar}");
    }

    private double ExponentRepeatExpression()
    {
        if (_lookahead == TokenType.EXP)
        {
            LookAhead();
            return Math.Pow(NegativeExpression(), ExponentRepeatExpression());
        }
        else if(_lookahead == TokenType.MULTI || _lookahead == TokenType.DIV || _lookahead == TokenType.PLUS || _lookahead == TokenType.MINUS || _lookahead == TokenType.CBRACKET || _lookahead == TokenType.EOF)
        {
            return 1;
        }
        throw new Exception($"Expected ^ at position: {_readerPosition}, found character: {_lastChar}");
    }

    private double BracketOrNumExpression()
    {
        if (_lookahead == TokenType.OBRACKET)
        {
            LookAhead();
            var tmp = AddExpression();
            if (_lookahead == TokenType.CBRACKET)
            {
                LookAhead();
            }
            else throw new Exception($"Expected ) at position: {_readerPosition}, found character: {_lastChar}");
            return tmp;
        }
        if (_lookahead == TokenType.NUM)
        {
            var result = _numval;
            LookAhead();
            return result;
        }
        throw new Exception($"Expected ( or num at position: {_readerPosition}, found character: {_lastChar}");
    }
}
