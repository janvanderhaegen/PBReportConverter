using ReportMigration.Models;
using System.Text;

namespace ReportMigration.Helpers;

internal static class DataSourceXmlGenerator
{
    public static string GenerateDataSourceXML(TableModel table)
    {
        var _writer = new StringWriter();
        var attributes = table._attributes;
        _writer.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
        _writer.WriteLine("<SqlDataSource Name=\"sqlDataSource1\">");
        _writer.WriteLine("<Connection Name=\"TestDBConnection\" FromAppConfig=\"true\" />");

        if (attributes.TryGetValue("procedure", out var query))
        {
            _writer.WriteLine($"<Query Type=\"StoredProcQuery\" Name=\"Query\">");
            var procedureName = GetProcedureName(query);
            _writer.WriteLine($"<ProcName>{procedureName}</ProcName>");
        }
        else if (attributes.TryGetValue("retrieve", out query))
        {
            _writer.WriteLine($"<Query Type=\"CustomSqlQuery\" Name=\"Query\">");
            if (!attributes.TryGetValue("sort", out var sortString))
            {
                sortString = "";
            }
            _writer.WriteLine($"<Sql>{query.Replace(":", "@").Replace("~\"", "'")} {ParseSorting(sortString)}</Sql>");
        }
        else
        {
            throw new Exception("SQL statement for data source is undefined");
        }

        if (attributes.TryGetValue("arguments", out var parameters))
        {
            foreach (var paramName in PBFormattingHelper.GetParameters(parameters))
            {
                _writer.WriteLine($"<Parameter Name=\"@{paramName}\" Type=\"DevExpress.DataAccess.Expression\">(System.String)(?{paramName})</Parameter>");
            }
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

    private static string ParseSorting(string sortString)
    {
        if(sortString == "")
        {
            return sortString;
        }

        List<string> sortList = [];
        var sortElements = sortString.Trim().Split(' ');
        foreach(var sortElement in sortElements.Chunk(2))
        {
            var sortMethod = sortElement[1] == "A" ? "ASC" : "DESC";
            sortList.Add($"{sortElement[0]} {sortMethod}");
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
