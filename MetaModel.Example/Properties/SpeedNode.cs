namespace MetaModel.Example.Properties;

internal sealed class SpeedNode : MeasuredVariableNode
{
    public SpeedNode(double value, string unit = "m/s") : base("Speed", value, unit) { }
}
