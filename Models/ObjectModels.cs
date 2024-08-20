namespace ReportMigration.Models;

internal abstract class ObjectModel(string objType)
{
    public string _objectType = objType;
}

internal class ContainerModel : ObjectModel
{
    public ContainerModel(string objType) : base(objType)
    {
    }

    public List<SubModel> Elements { get; } = new List<SubModel>();

    public Dictionary<string, object> Attributes { get; init; }
}

internal class SubModel : ObjectModel
{
    public SubModel(string objType) : base(objType)
    {
    }
    public Dictionary<string, object> Attributes { get; init; }
}
