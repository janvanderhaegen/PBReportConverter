using System.Text;

namespace ReportMigration.Parser;

internal class MathParser
{
    private StringReader _reader;

    private double _numval;
    private TokenType _lookahead;
    public enum TokenType
    {
        PLUS,
        MINUS,
        MULTI,
        DIV,
        NUM,
        OBRACKET,
        CBRACKET,
        EOF
    }

    public MathParser(string expression)
    {
        _reader = new StringReader(expression);
    }

    private void LookAhead()
    {
        var c = this._reader.Read();
        if (c == -1)
        {
            _lookahead = TokenType.EOF;
            return;
        }
        switch ((char) c)
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
        }
        if (Char.IsDigit((char) c))
        {
            StringBuilder builder = new StringBuilder();
            builder.Append((char) c);
            while (Char.IsDigit((char) this._reader.Peek()))
            {
                builder.Append((char) this._reader.Read());
            }
            if (double.TryParse(builder.ToString(), out _numval))
            {
                _lookahead = TokenType.NUM;
                return;
            }
        }
        throw new Exception("Unexpected character");
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
        throw new Exception("Expected ( or num");
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
        throw new Exception("Expected + or - or end of expression");
    }

    private double MultiplyExpression()
    {
        if (_lookahead == TokenType.MINUS || _lookahead == TokenType.OBRACKET || _lookahead == TokenType.NUM)
        {
            return NegativeExpression() * RepeatMultiplyExpression();
        }
        throw new Exception("Expected ( or num");
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
        throw new Exception("Expected * or / or end of expression");
    }

    private double NegativeExpression()
    {
        if (_lookahead == TokenType.MINUS)
        {
            LookAhead();
            return -BracketOrNumExpression();
        }
        else if (_lookahead == TokenType.OBRACKET || _lookahead == TokenType.NUM)
        {
            return BracketOrNumExpression();
        }
        throw new Exception("Expected - or ( or num");
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
            else throw new Exception("Expected )");
            return tmp;
        }
        if (_lookahead == TokenType.NUM)
        {
            var result = _numval;
            LookAhead();
            return result;
        }
        throw new Exception("Expected ( or num");
    }
}
