using MetaModel.Example.Contracts;

namespace MetaModel.Example.Properties;

// Base for variables that carry a numeric value with a unit (e.g., length, duration, speed)
internal abstract class MeasuredVariableNode : BaseNode
{
    public NumberNode ValueNode { get; }
    public double Value => ValueNode.Value;
    public string Unit { get; }

    protected MeasuredVariableNode(string name, double value, string unit)
    {
        Name = name;
        ValueNode = new NumberNode(value);
        Unit = unit;
        AddChild(ValueNode); // keep tree relationships
    }
}
