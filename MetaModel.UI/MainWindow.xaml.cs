using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MahApps.Metro.Controls; // MahApps base window
using MetaModel.Example.Contracts;
using MetaModel.Example.Evaluation;
using MetaModel.Example.Expressions;
using MetaModel.Example.Properties;

namespace MetaModel.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow // inherit MetroWindow
    {
        private readonly List<INode> _steps = new();
        private readonly EvaluationContext _ctx = new();
        private readonly List<(string op, string operand)> _chainSegments = new();

        public MainWindow()
        {
            InitializeComponent();
            _ctx.RegisterFunction("sum", (c, args) => new NumberNode(args.OfType<NumberNode>().Sum(a => a.Value)));
            // Set default modes explicitly
            ThenMode.SelectedIndex = 0;
            ElseMode.SelectedIndex = 0;
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

            INode BuildBranch(string mode, string valueText, string opLeftText, string opSymbol, string opRightText, string assignVar)
            {
                INode expr = NodeFromText(valueText);
                if (mode == "Operation" || mode == "Operation+Assign")
                {
                    if (!string.IsNullOrWhiteSpace(opLeftText) && !string.IsNullOrWhiteSpace(opSymbol) && !string.IsNullOrWhiteSpace(opRightText))
                    {
                        var l = NodeFromText(opLeftText);
                        var r = NodeFromText(opRightText);
                        expr = opSymbol switch
                        {
                            "+" => new AddNode(l, r),
                            "-" => new SubtractNode(l, r),
                            "*" => new MultiplyNode(l, r),
                            "/" => new DivideNode(l, r),
                            _ => expr
                        };
                    }
                }
                if (mode == "Operation+Assign" && !string.IsNullOrWhiteSpace(assignVar))
                {
                    expr = new AssignmentNode(assignVar.Trim(), expr);
                }
                return expr;
            }

            var thenMode = (ThenMode.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Value";
            var elseMode = (ElseMode.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Value";

            var thenExpr = BuildBranch(thenMode, ThenValue.Text.Trim(), ThenLeft.Text.Trim(), (ThenOp.SelectedItem as ComboBoxItem)?.Content?.ToString(), ThenRight.Text.Trim(), ThenAssignVar.Text.Trim());
            var elseExpr = BuildBranch(elseMode, ElseValue.Text.Trim(), ElseLeft.Text.Trim(), (ElseOp.SelectedItem as ComboBoxItem)?.Content?.ToString(), ElseRight.Text.Trim(), ElseAssignVar.Text.Trim());

            AddStep(new ConditionalNode(cond, thenExpr, elseExpr), $"if {CondLeft.Text.Trim()} {condOp} {CondRight.Text.Trim()} then [{thenMode}] else [{elseMode}]");
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

        private void OnChainAddSegment(object sender, RoutedEventArgs e)
        {
            var op = (ChainOp.SelectedItem as ComboBoxItem)?.Content?.ToString();
            var operandText = ChainNextOperand.Text.Trim();
            if (string.IsNullOrWhiteSpace(op) || string.IsNullOrWhiteSpace(operandText)) return;
            _chainSegments.Add((op, operandText));
            ChainList.Items.Add($"{op} {operandText}");
            UpdateChainPreview();
            ChainNextOperand.Clear();
        }

        private void OnChainClear(object sender, RoutedEventArgs e)
        {
            _chainSegments.Clear();
            ChainList.Items.Clear();
            ChainPreview.Text = "(empty)";
            ChainStart.Clear();
            ChainTargetVar.Clear();
        }

        private void OnChainFinalize(object sender, RoutedEventArgs e)
        {
            var start = ChainStart.Text.Trim();
            if (string.IsNullOrWhiteSpace(start)) return;
            // Build nested INode expression
            INode current = NodeFromText(start);
            foreach (var (op, operand) in _chainSegments)
            {
                var right = NodeFromText(operand);
                current = op switch
                {
                    "+" => new AddNode(current, right),
                    "-" => new SubtractNode(current, right),
                    "*" => new MultiplyNode(current, right),
                    "/" => new DivideNode(current, right),
                    _ => current
                };
            }

            // Optional constant simplification: if all nodes are numbers after building, collapse.
            if (ChainSimplify.IsChecked == true && TryEvaluateConstants(current) is NumberNode collapsed)
            {
                current = collapsed;
            }

            var targetVar = ChainTargetVar.Text.Trim();
            if (!string.IsNullOrWhiteSpace(targetVar))
            {
                AddStep(new AssignmentNode(targetVar, current), $"chain {targetVar} = {ChainPreview.Text}");
            }
            else
            {
                AddStep(current, $"chain expr {ChainPreview.Text}");
            }

            // Keep chain for reuse? Here we clear.
            OnChainClear(sender, e);
        }

        private void UpdateChainPreview()
        {
            var start = ChainStart.Text.Trim();
            if (string.IsNullOrWhiteSpace(start)) { ChainPreview.Text = "(empty)"; return; }
            var expr = start;
            foreach (var (op, operand) in _chainSegments)
            {
                expr += $" {op} {operand}";
            }
            ChainPreview.Text = expr;
        }

        private INode? TryEvaluateConstants(INode node)
        {
            // Recursively attempt to evaluate pure numeric subtrees.
            switch (node)
            {
                case AddNode add:
                    var lnAdd = TryEvaluateConstants(add.Left) ?? add.Left;
                    var rnAdd = TryEvaluateConstants(add.Right) ?? add.Right;
                    if (lnAdd is NumberNode l1 && rnAdd is NumberNode r1) return new NumberNode(l1.Value + r1.Value);
                    return null;
                case SubtractNode sub:
                    var lnSub = TryEvaluateConstants(sub.Left) ?? sub.Left;
                    var rnSub = TryEvaluateConstants(sub.Right) ?? sub.Right;
                    if (lnSub is NumberNode l2 && rnSub is NumberNode r2) return new NumberNode(l2.Value - r2.Value);
                    return null;
                case MultiplyNode mul:
                    var lnMul = TryEvaluateConstants(mul.Left) ?? mul.Left;
                    var rnMul = TryEvaluateConstants(mul.Right) ?? mul.Right;
                    if (lnMul is NumberNode l3 && rnMul is NumberNode r3) return new NumberNode(l3.Value * r3.Value);
                    return null;
                case DivideNode div:
                    var lnDiv = TryEvaluateConstants(div.Left) ?? div.Left;
                    var rnDiv = TryEvaluateConstants(div.Right) ?? div.Right;
                    if (lnDiv is NumberNode l4 && rnDiv is NumberNode r4 && r4.Value != 0) return new NumberNode(l4.Value / r4.Value);
                    return null;
                default:
                    return node is NumberNode ? node : null;
            }
        }

        // Branch mode change handlers
        private void OnThenModeChanged(object sender, SelectionChangedEventArgs e)
        {
            var mode = (ThenMode.SelectedItem as ComboBoxItem)?.Content?.ToString();
            ThenValuePanel.Visibility = mode == "Value" ? Visibility.Visible : Visibility.Collapsed;
            ThenOpPanel.Visibility = (mode == "Operation" || mode == "Operation+Assign") ? Visibility.Visible : Visibility.Collapsed;
            ThenAssignPanel.Visibility = mode == "Operation+Assign" ? Visibility.Visible : Visibility.Collapsed;
        }

        private void OnElseModeChanged(object sender, SelectionChangedEventArgs e)
        {
            var mode = (ElseMode.SelectedItem as ComboBoxItem)?.Content?.ToString();
            ElseValuePanel.Visibility = mode == "Value" ? Visibility.Visible : Visibility.Collapsed;
            ElseOpPanel.Visibility = (mode == "Operation" || mode == "Operation+Assign") ? Visibility.Visible : Visibility.Collapsed;
            ElseAssignPanel.Visibility = mode == "Operation+Assign" ? Visibility.Visible : Visibility.Collapsed;
        }

        private void OnRemoveSelectedStep(object sender, RoutedEventArgs e)
        {
            var index = StepsList.SelectedIndex;
            if (index < 0 || index >= _steps.Count) return;
            _steps.RemoveAt(index);
            StepsList.Items.RemoveAt(index);
        }

        private void OnClearAllSteps(object sender, RoutedEventArgs e)
        {
            _steps.Clear();
            StepsList.Items.Clear();
            ResultText.Text = string.Empty;
        }

        private void OnRemoveStepDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var index = StepsList.SelectedIndex;
            if (index < 0 || index >= _steps.Count) return;
            _steps.RemoveAt(index);
            StepsList.Items.RemoveAt(index);
        }

        private void OnParseRunScript(object sender, RoutedEventArgs e)
        {
            ParserErrors.Text = string.Empty;
            ParsedStepsList.Items.Clear();
            ParsedResultText.Text = string.Empty;
            var text = ParserInput.Text;
            if (string.IsNullOrWhiteSpace(text)) return;
            try
            {
                var scriptNode = MetaModel.Example.Parsing.ExpressionParser.ParseScript(text);
                // Show each statement description (simple ToString fallback)
                foreach (var stmt in scriptNode.Children)
                {
                    ParsedStepsList.Items.Add(stmt.Name);
                }
                var result = Evaluator.Evaluate(scriptNode, _ctx);
                ParsedResultText.Text = result switch
                {
                    NumberNode num => $"Result: {num.Value}",
                    VectorNode vec => "Result: [" + string.Join(",", vec.Components) + "]",
                    _ => $"Result: {result.GetType().Name}"
                };
            }
            catch (System.Exception ex)
            {
                ParserErrors.Text = ex.Message;
            }
        }

        private void OnClearParsed(object sender, RoutedEventArgs e)
        {
            ParserInput.Clear();
            ParsedStepsList.Items.Clear();
            ParsedResultText.Text = string.Empty;
            ParserErrors.Text = string.Empty;
        }
    }
}