namespace MetaModel.Example.Properties;

internal sealed class LengthNode : MeasuredVariableNode
{
    public LengthNode(double value, string unit = "m") : base("Length", value, unit) { }
}
