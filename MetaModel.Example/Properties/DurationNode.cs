namespace MetaModel.Example.Properties;

internal sealed class DurationNode : MeasuredVariableNode
{
    public DurationNode(double value, string unit = "s") : base("Duration", value, unit) { }
}
