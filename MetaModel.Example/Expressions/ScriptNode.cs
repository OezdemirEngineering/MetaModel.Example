using MetaModel.Example.Contracts;

namespace MetaModel.Example.Expressions;

public class ScriptNode : BaseNode
{
    public IReadOnlyList<INode> Statements { get; }

    public ScriptNode(IEnumerable<INode> statements)
    {
        Name = "Script";
        var list = statements.ToList();
        Statements = list;
        foreach (var s in list) AddChild(s);
    }
}
