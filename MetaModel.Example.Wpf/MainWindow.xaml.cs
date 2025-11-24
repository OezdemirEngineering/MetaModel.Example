using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MetaModel.Example.Contracts;
using MetaModel.Example.Evaluation;
using MetaModel.Example.Expressions;
using MetaModel.Example.Properties;

namespace MetaModel.Example.Wpf;

public partial class MainWindow : Window
{
    private readonly List<INode> _steps = new();
    private readonly EvaluationContext _ctx = new();

    public MainWindow()
    {
        InitializeComponent();
        _ctx.RegisterFunction("sum", (c, args) => new NumberNode(args.OfType<NumberNode>().Sum(a => a.Value)));
    }

    private INode NodeFromText(string text)
    {
        if (double.TryParse(text, out var v)) return new NumberNode(v);
        return new VariableNode(text, new NumberNode(0));
    }

    private void AddStep(INode node, string description)
    {
        _steps.Add(node);
        StepsList.Items.Add(description);
    }

    private void OnAddAssign(object sender, RoutedEventArgs e)
    {
        var name = AssignVarName.Text.Trim();
        if (string.IsNullOrWhiteSpace(name)) return;
        var valueNode = NodeFromText(AssignValue.Text.Trim());
        AddStep(new AssignmentNode(name, valueNode), $"assign {name} = {AssignValue.Text.Trim()}");
    }

    private void OnAddOperation(object sender, RoutedEventArgs e)
    {
        var leftText = OpLeft.Text.Trim();
        var rightText = OpRight.Text.Trim();
        if (string.IsNullOrWhiteSpace(leftText) || string.IsNullOrWhiteSpace(rightText)) return;
        var left = NodeFromText(leftText);
        var right = NodeFromText(rightText);
        var opSymbol = (OpType.SelectedItem as ComboBoxItem)?.Content?.ToString();
        switch (opSymbol)
        {
            case "+": AddStep(new AddNode(left, right), $"op {leftText} + {rightText}"); break;
            case "-": AddStep(new SubtractNode(left, right), $"op {leftText} - {rightText}"); break;
            case "*": AddStep(new MultiplyNode(left, right), $"op {leftText} * {rightText}"); break;
            case "/": AddStep(new DivideNode(left, right), $"op {leftText} / {rightText}"); break;
        }
    }

    private void OnAddConditional(object sender, RoutedEventArgs e)
    {
        var left = NodeFromText(CondLeft.Text.Trim());
        var right = NodeFromText(CondRight.Text.Trim());
        var condOp = (CondOp.SelectedItem as ComboBoxItem)?.Content?.ToString();
        BinaryExpressionNode cond = condOp switch
        {
            "==" => new EqualNode(left, right),
            ">" => new GreaterThanNode(left, right),
            _ => new EqualNode(left, right)
        };
        var thenNode = NodeFromText(ThenValue.Text.Trim());
        var elseNode = NodeFromText(ElseValue.Text.Trim());
        AddStep(new ConditionalNode(cond, thenNode, elseNode), $"if {CondLeft.Text.Trim()} {condOp} {CondRight.Text.Trim()} then {ThenValue.Text.Trim()} else {ElseValue.Text.Trim()}");
    }

    private void OnAddFunctionCall(object sender, RoutedEventArgs e)
    {
        var name = FuncName.Text.Trim();
        if (string.IsNullOrWhiteSpace(name)) return;
        var argsCsv = FuncArgs.Text.Trim();
        var args = argsCsv
            .Split(',', System.StringSplitOptions.RemoveEmptyEntries | System.StringSplitOptions.TrimEntries)
            .Select(NodeFromText)
            .ToList();
        AddStep(new FunctionCallNode(name, args), $"call {name}({argsCsv})");
    }

    private void OnRunScript(object sender, RoutedEventArgs e)
    {
        var script = new ScriptNode(_steps);
        var result = Evaluator.Evaluate(script, _ctx);
        ResultText.Text = result switch
        {
            NumberNode num => $"Result: {num.Value}",
            VectorNode vec => "Result: [" + string.Join(",", vec.Components) + "]",
            _ => $"Result: {result.GetType().Name}"
        };
    }
}