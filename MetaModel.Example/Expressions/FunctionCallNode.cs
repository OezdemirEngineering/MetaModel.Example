using MetaModel.Example.Contracts;

namespace MetaModel.Example.Expressions;

public class FunctionCallNode : BaseNode
{
    public string FunctionName { get; }
    public IReadOnlyList<INode> Arguments { get; }

    public FunctionCallNode(string functionName, IEnumerable<INode> args)
    {
        Name = "FunctionCall";
        FunctionName = functionName;
        var argList = args.ToList();
        Arguments = argList;
        foreach (var a in argList) AddChild(a);
    }
}
