using MetaModel.Example.Contracts;

namespace MetaModel.Example.Expressions;

public class AssignmentNode(string variableName, INode expression) : BaseNode
{
    public string VariableName { get; } = variableName;
    public INode Expression { get; } = expression;
}
