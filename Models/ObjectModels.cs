using DevExpress.Pdf.Native.BouncyCastle.Asn1.Cms;

namespace ReportMigration.Models;

internal class ObjectModel(string objType, Dictionary<string, object> attributes)
{
    public string _objectType = objType;
    public Dictionary<string, object> _attributes = attributes;
}

internal class ContainerModel : ObjectModel
{
    public ContainerModel(string objType, Dictionary<string, object> attributes) : base(objType, attributes)
    {
    }

    public List<ObjectModel> _elements { get; } = new List<ObjectModel>();
}

internal class TableModel : ObjectModel
{
    public TableModel(string objType, Dictionary<string, object> attributes, List<ObjectModel> columns) : base(objType, attributes)
    {
        _columns = columns;
    }
    public List<ObjectModel> _columns;
}

//internal class SubModel : ObjectModel
//{
//    public SubModel(string objType, Dictionary<string, object> attributes) : base(objType)
//    {
//        _attributes = attributes;
//    }
//}
