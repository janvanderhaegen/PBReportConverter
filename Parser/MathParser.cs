using System.Text;

namespace ReportMigration.Parser;

internal class MathParser
{
    private StringReader _reader;

    private Token _lookahead = new EOFToken();

    public enum Operation
    {
        PLUS,
        MINUS,
        MULTI,
        DIV
    }

    public MathParser(string expression)
    {
        _reader = new StringReader(expression);
    }

    public abstract class Token { }

    public class OpenBracketToken : Token { }
    public class CloseBracketToken : Token { }

    public class EOFToken : Token { }
    public class OperatorToken : Token
    {
        public Operation _op;
        public OperatorToken(Operation op)
        {
            _op = op;
        }
    }

    public class NumValueToken : Token
    {
        public readonly double _value;
        public NumValueToken(double value)
        {
            _value = value;
        }
    }

    private void LookAhead()
    {
        var c = this._reader.Read();
        if (c == -1)
        {
            _lookahead = new EOFToken();
            return;
        }
        switch ((char) c)
        {
            
            case '(':
                {
                    _lookahead = new OpenBracketToken();
                    return;
                }
            case ')':
                {
                    _lookahead = new CloseBracketToken();
                    return;
                }
            case '+':
                {
                    _lookahead = new OperatorToken(Operation.PLUS);
                    return;
                }
            case '-':
                {
                    _lookahead = new OperatorToken(Operation.MINUS);
                    return;
                }
            case '*':
                {
                    _lookahead = new OperatorToken(Operation.MULTI);
                    return;
                }
            case '/':
                {
                    _lookahead = new OperatorToken(Operation.DIV);
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
            if (double.TryParse(builder.ToString(), out var numval))
            {
                _lookahead = new NumValueToken(numval);
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
        if ((_lookahead is OperatorToken && ((OperatorToken)_lookahead)._op == Operation.MINUS) || _lookahead is OpenBracketToken || _lookahead is NumValueToken) 
        {
            return MultiplyExpression() + RepeatAddExpression();
        }
        throw new Exception("Expected ( or num");
    }

    private double RepeatAddExpression()
    {
        if(_lookahead is OperatorToken)
        {
            if (((OperatorToken)_lookahead)._op==Operation.PLUS)
            {
                LookAhead();
                return MultiplyExpression() + RepeatAddExpression();
            }
            else if (((OperatorToken)_lookahead)._op == Operation.MINUS)
            {
                LookAhead();
                return -(MultiplyExpression() + RepeatAddExpression());
            }
        }
        else if (_lookahead is CloseBracketToken || _lookahead is EOFToken)
        {
            return 0;
        }
        throw new Exception("Expected + or - or end of expression");
    }

    private double MultiplyExpression()
    {
        if ((_lookahead is OperatorToken && ((OperatorToken)_lookahead)._op == Operation.MINUS) || _lookahead is OpenBracketToken || _lookahead is NumValueToken)
        {
            return NegativeExpression() * RepeatMultiplyExpression();
        }
        throw new Exception("Expected ( or num");
    }

    private double RepeatMultiplyExpression()
    {
        if (_lookahead is OperatorToken)
        {
            if (((OperatorToken)_lookahead)._op == Operation.MULTI)
            {
                LookAhead();
                return NegativeExpression() * RepeatMultiplyExpression();
            }
            else if (((OperatorToken)_lookahead)._op == Operation.DIV)
            {
                LookAhead();
                return 1 / (NegativeExpression() * RepeatMultiplyExpression());
            }
            else { return 1; }
        }
        else if (_lookahead is CloseBracketToken || _lookahead is EOFToken)
        {
            return 1;
        }
        throw new Exception("Expected * or / or end of expression");
    }

    private double NegativeExpression()
    {
        if (_lookahead is OperatorToken && ((OperatorToken)_lookahead)._op == Operation.MINUS)
        {
            LookAhead();
            return -BracketOrNumExpression();
        }
        else if (_lookahead is OpenBracketToken || _lookahead is NumValueToken)
        {
            return BracketOrNumExpression();
        }
        throw new Exception("Expected - or ( or num");
    }

    private double BracketOrNumExpression()
    {
        if (_lookahead is OpenBracketToken)
        {
            LookAhead();
            var tmp = AddExpression();
            if (_lookahead is CloseBracketToken)
            {
                LookAhead();
            }
            else throw new Exception("Expected )");
            return tmp;
        }
        if (_lookahead is NumValueToken)
        {
            var result = ((NumValueToken)_lookahead)._value;
            LookAhead();
            return result;
        }
        throw new Exception("Expected ( or num");
    }
}
