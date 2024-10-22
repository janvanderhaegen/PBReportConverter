using PBReportConverter.Models;
using System.Text;
using System.Text.RegularExpressions;
using static PBReportConverter.Helpers.PBFormattingHelper;

namespace PBReportConverter.Helpers;

internal static class DataSourceXmlGenerator
{
    public static string GenerateDataSourceXML(TableModel table)
    {
        var _writer = new StringWriter();
        var attributes = table._attributes;
        _writer.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
        _writer.WriteLine("<SqlDataSource Name=\"sqlDataSource1\">");
        _writer.WriteLine("<Connection Name=\"TIES Data\" FromAppConfig=\"true\" />");

        var parameters = new List<string>();
        if (attributes.TryGetValue("procedure", out var query))
        {
            _writer.WriteLine($"<Query Type=\"StoredProcQuery\" Name=\"Query\">");
            var procedureName = GetProcedureName(query);
            _writer.WriteLine($"<ProcName>{procedureName}</ProcName>");
            parameters = GetProcedureParams(query);
        }
        else if (attributes.TryGetValue("retrieve", out query))
        {
            _writer.WriteLine($"<Query Type=\"CustomSqlQuery\" Name=\"Query\">");
            if (!attributes.TryGetValue("sort", out var sortString) || query.Contains("ORDER BY", StringComparison.CurrentCultureIgnoreCase))
            {
                sortString = "";
            }
            _writer.WriteLine($"<Sql>{query.Replace(":", "@").Replace("~\"", "'")} {ParseSorting(sortString, table)}</Sql>");
            
            if (attributes.TryGetValue("arguments", out var paramString))
            {
                parameters = GetParameters(paramString);
            }
        }
        else
        {
            throw new Exception("SQL statement for data source is undefined");
        }

        foreach (var paramName in parameters)
        {
            _writer.WriteLine($"<Parameter Name=\"@{paramName}\" Type=\"DevExpress.DataAccess.Expression\">(System.String)(?{paramName})</Parameter>");
        }

        _writer.WriteLine("</Query>");

        _writer.WriteLine("<ResultSchema>");
        _writer.WriteLine("<DataSet Name =\"sqlDataSource1\">");
        _writer.WriteLine($"<View Name=\"Query\">");

        foreach (var column in table._columns)
        {
            var colAttributes = column._attributes;
            _writer.WriteLine($"<Field Name=\"{colAttributes["name"]}\" Type=\"String\" />");
        }
        _writer.WriteLine("</View>");
        _writer.WriteLine("</DataSet>");
        _writer.WriteLine("</ResultSchema>");
        _writer.WriteLine("<ConnectionOptions CloseConnection=\"true\" />");
        _writer.WriteLine("</SqlDataSource>");

        var resultString = _writer.ToString();

        return Convert.ToBase64String(Encoding.UTF8.GetBytes(resultString));
    }

    private static List<string> GetProcedureParams(string query)
    {
        var startIndex = query.IndexOf(';')+1;
        var subString = query[startIndex..];

        var matches = Regex.Matches(subString, @"@([_a-zA-Z]+)");
        return matches.Select(x => x.Groups[1].Value).ToList();
    }

    private static string ParseSorting(string sortString, TableModel table)
    {
        if(sortString == "")
        {
            return sortString;
        }

        List<string> sortList = [];
        var sortElements = sortString.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        foreach(var sortElement in sortElements.Chunk(2))
        {
            var sortField = sortElement[0];
            if(table._columns.Where(col => col._attributes["name"] == sortField).Any())
            {
                var sortMethod = sortElement[1] == "A" ? "ASC" : "DESC";
                sortList.Add($"{sortElement[0]} {sortMethod}");
            }
        }

        return "ORDER BY " + string.Join(',', sortList);
    }

    private static string GetProcedureName(string query)
    {
        var startIndex = query.EndIndexOf("execute ");
        var endIndex = query.IndexOf(';');
        return query[startIndex..endIndex];
    }

    private static int EndIndexOf(this string source, string value)
    {
        int index = source.IndexOf(value);
        if (index >= 0)
        {
            index += value.Length;
        }

        return index;
    }
}
