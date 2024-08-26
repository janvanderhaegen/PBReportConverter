using DevExpress.DataAccess.Sql;
using DevExpress.Xpo.DB.Helpers;
using DevExpress.XtraRichEdit.Model;
using ReportMigration.Models;
using System.Text;
using System;
using DevExpress.Data.Linq;

namespace ReportMigration.Helpers;

internal static class DataSourceXmlGenerator
{
    public static string GenerateDataSourceXML(ContainerModel table)
    {
        var _writer = new StringWriter();
        var attributes = table._attributes;
        _writer.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
        _writer.WriteLine("<SqlDataSource Name=\"sqlDataSource1\">");
        _writer.WriteLine("<Connection Name=\"TestDBConnection\" FromAppConfig=\"true\" />");
        string queryName;

        if(attributes.TryGetValue("procedure", out var query))
        {
            queryName = "StoredProcedure";
            _writer.WriteLine($"<Query Type=\"StoredProcQuery\" Name=\"{queryName}\">");
            var procedureName = GetProcedureName((string)query);
            _writer.WriteLine($"<ProcName>{procedureName}</ProcName>");
        }
        else if(attributes.TryGetValue("retrieve", out query))
        {
            queryName = "Query";
            _writer.WriteLine($"<Query Type=\"CustomSqlQuery\" Name=\"{queryName}\">");
            _writer.WriteLine($"<Sql>{((string)query).Replace(":", "@")}</Sql>");
        }
        else
        {
            throw new Exception("SQL statement for data source is undefined");
        }

        if (attributes.TryGetValue("arguments", out var parameters))
        {
            foreach (var element in (List<object>)parameters)
            {
                var paramName = ((List<object>)element)[0];
                _writer.WriteLine($"<Parameter Name=\"@{paramName}\" Type=\"DevExpress.DataAccess.Expression\">(System.String)(?{paramName})</Parameter>");
            }
        }
        _writer.WriteLine("</Query>");

        _writer.WriteLine("<ResultSchema>");
        _writer.WriteLine("<DataSet Name =\"sqlDataSource1\">");
        _writer.WriteLine($"<View Name=\"{queryName}\">");
        if(!attributes.TryGetValue("columns", out var columns))
        {
            throw new Exception("Columns of data source are undefined");
        }



        return "";
    }

    private static object GetProcedureName(string query)
    {
        var startIndex = query.EndIndexOf("execute ");
        var endIndex = query.IndexOf(";");
        return query.Substring(startIndex, endIndex-startIndex);
    }

    public static int EndIndexOf(this string source, string value)
    {
        int index = source.IndexOf(value);
        if (index >= 0)
        {
            index += value.Length;
        }

        return index;
    }
}
