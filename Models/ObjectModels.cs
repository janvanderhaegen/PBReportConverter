namespace PBReportConverter.Models;

internal class ObjectModel(string objType, Dictionary<string, string> attributes)
{
    public string _objectType = objType;
    public Dictionary<string, string> _attributes = attributes;
}

internal class ContainerModel(string objType, Dictionary<string, string> attributes) : ObjectModel(objType, attributes)
{
    public List<ObjectModel> _elements = [];
}

internal class TableModel(string objType, Dictionary<string, string> attributes, List<ObjectModel> columns) : ContainerModel(objType, attributes)
{
    public List<ObjectModel> _columns = columns;
}
