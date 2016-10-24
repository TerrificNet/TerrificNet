using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace TerrificNet.Thtml.Emit.Compiler
{
	public class AsyncExpressionBuilder : IExpressionBuilder
	{
		private readonly List<AsyncState> _states = new List<AsyncState>();
		private AsyncState _currentState;

		private readonly ParameterExpression _currentStateExpression = Expression.Parameter(typeof(int));
		private readonly ParameterExpression _stateMachine = Expression.Parameter(typeof(AsyncViewStateMachine));

		private readonly List<ParameterExpression> _variables = new List<ParameterExpression>();

		public AsyncExpressionBuilder()
		{
			_currentState = new AsyncState(-1);
			_states.Add(_currentState);
		}

		public void Add(Expression expression)
		{
			if (IsTask(expression))
			{
				_currentState.Expressions.Add(AwaitExpression(_currentState.Id + 1, expression));
				_currentState = new AsyncState(_currentState.Id + 1);
				_states.Add(_currentState);
			}
			else
				_currentState.Expressions.Add(expression);
		}

		public void DefineVariable(ParameterExpression expression)
		{
			_variables.Add(expression);
		}

		private Expression AwaitExpression(int nextState, Expression awaiterExpression)
		{
			return Expression.Call(_stateMachine, typeof(AsyncViewStateMachine).GetTypeInfo().GetMethod(nameof(AsyncViewStateMachine.Await)), Expression.Constant(nextState), awaiterExpression);
		}

		public Func<Task> Compile()
		{
			var stateBuilder = new IncrementalTypeBuilder(typeof(object));
			var variableStates = new Dictionary<ParameterExpression, FieldInfoReference>();
			foreach (var variable in _variables)
			{
				var fieldReference = stateBuilder.AddField(variable.Type);
				variableStates.Add(variable, fieldReference);
			}

			var stateBuilderType = stateBuilder.Complete().Type;
			var stateType = stateBuilderType;
			var stateExpression = Expression.Variable(stateBuilderType);

			var assignStateExpression = Expression.Assign(stateExpression, Expression.Convert(Expression.PropertyOrField(_stateMachine, "State"), stateType));

			var breakTarget = Expression.Label();
			var breakExpression = Expression.Break(breakTarget);

			var tailCall = Expression.Call(_stateMachine, typeof(AsyncViewStateMachine).GetTypeInfo().GetMethod(nameof(AsyncViewStateMachine.Complete)));
			_states[_states.Count - 1].Expressions.Add(tailCall);

			var visitor = new ReplaceVariablePlaceholderVisitor(stateExpression, variableStates);

			var switchExpression = Expression.Switch(_currentStateExpression, _states.Select(e => GetSwitchCase(e, breakExpression, visitor)).ToArray());
			var bodyExpression = Expression.Block(new[] { stateExpression }, assignStateExpression, switchExpression, Expression.Label(breakTarget));

			var lambda = Expression.Lambda<Action<int, AsyncViewStateMachine>>(bodyExpression, _currentStateExpression, _stateMachine);
			var action = lambda.Compile();
			var state = new AsyncViewStateMachine(Activator.CreateInstance(stateType), action);

			return () => state.Start(new CancellationToken());
		}

		private static SwitchCase GetSwitchCase(AsyncState state, GotoExpression breakExpression, ExpressionVisitor visitor)
		{
			var stateExpressions = state.Expressions.Select(e => ReplaceVariablePlaceholder(e, visitor));

			return Expression.SwitchCase(Expression.Block(stateExpressions.Concat(new[] { breakExpression })), Expression.Constant(state.Id));
		}

		private static Expression ReplaceVariablePlaceholder(Expression expression, ExpressionVisitor visitor)
		{
			return visitor.Visit(expression);
		}

		private class ReplaceVariablePlaceholderVisitor : ExpressionVisitor
		{
			private readonly Expression _stateExpression;
			private readonly Dictionary<ParameterExpression, FieldInfoReference> _variableStates;

			public ReplaceVariablePlaceholderVisitor(Expression stateExpression, Dictionary<ParameterExpression, FieldInfoReference> variableStates)
			{
				_stateExpression = stateExpression;
				_variableStates = variableStates;
			}

			protected override Expression VisitParameter(ParameterExpression node)
			{
				FieldInfoReference reference;
				if (_variableStates.TryGetValue(node, out reference))
				{
					return Expression.Field(_stateExpression, reference.FieldInfo);
				}

				return base.VisitParameter(node);
			}
		}

		private static bool IsTask(Expression expression)
		{
			return expression.Type == typeof(Task);
		}

		private class AsyncState
		{
			public AsyncState(int id)
			{
				Id = id;
				Expressions = new List<Expression>();
			}

			public int Id { get; }

			public IList<Expression> Expressions { get; }
		}
	}
}