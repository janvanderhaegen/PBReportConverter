using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReportMigration.Parser;

internal interface IToken { }

internal class StringToken : IToken 
{
    public string _value;

    public StringToken(string value) {  _value = value; }
}

internal enum SpecialChar
{
    OpenBracket,
    ClosedBracket,
    Equals,
    Space,
    NewLine,
    EOF
}

internal class SpecialToken : IToken
{
    public SpecialChar _value;

    public SpecialToken(SpecialChar value) { _value = value; }
}