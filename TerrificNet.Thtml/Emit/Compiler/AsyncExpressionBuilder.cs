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

		private IncrementalTypeBuilder _stateBuilder = new IncrementalTypeBuilder(typeof(object));

		private int _variableIndex;

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

		private class VariablePlaceholder
		{
			public VariablePlaceholder(FieldInfo field)
			{
				Field = field;
			}

			public FieldInfo Field { get; }
		}

		private class VariablePlaceholder<T> : VariablePlaceholder
		{
			public VariablePlaceholder(FieldInfo field) : base(field)
			{
			}

			public T Value { get; set; }
		}

		public Expression DefineVariable(Type type)
		{
			FieldInfo fieldInfo;
			_stateBuilder = _stateBuilder.AddField(type, out fieldInfo);

			var placeholderType = typeof(VariablePlaceholder<>).MakeGenericType(type);
			var placeHolder = Activator.CreateInstance(placeholderType, fieldInfo);
			var placeHolderExpression = Expression.Constant(placeHolder);

			var propertyInfo = placeholderType.GetTypeInfo().GetProperty(nameof(VariablePlaceholder<object>.Value));
			return Expression.Property(placeHolderExpression, propertyInfo);
		}

		private Expression AwaitExpression(int nextState, Expression awaiterExpression)
		{
			return Expression.Call(_stateMachine, typeof(AsyncViewStateMachine).GetTypeInfo().GetMethod(nameof(AsyncViewStateMachine.Await)), Expression.Constant(nextState), awaiterExpression);
		}

		public Func<Task> Compile()
		{
			var stateType = _stateBuilder.Type;
			var stateExpression = Expression.Variable(_stateBuilder.Type);

			var assignStateExpression = Expression.Assign(stateExpression, Expression.Convert(Expression.PropertyOrField(_stateMachine, "State"), stateType));

			var breakTarget = Expression.Label();
			var breakExpression = Expression.Break(breakTarget);

			var tailCall = Expression.Call(_stateMachine, typeof(AsyncViewStateMachine).GetTypeInfo().GetMethod(nameof(AsyncViewStateMachine.Complete)));
			_states[_states.Count - 1].Expressions.Add(tailCall);

			var visitor = new ReplaceVariablePlaceholderVisitor(stateExpression);

			var switchExpression = Expression.Switch(_currentStateExpression, _states.Select(e => GetSwitchCase(e, breakExpression, visitor)).ToArray());
			var bodyExpression = Expression.Block(new [] { stateExpression }, assignStateExpression, switchExpression, Expression.Label(breakTarget));

			var lambda = Expression.Lambda<Action<int, AsyncViewStateMachine>>(bodyExpression, _currentStateExpression, _stateMachine);
			var action = lambda.Compile();
			var state = new AsyncViewStateMachine(Activator.CreateInstance(stateType), action);

			return () => state.Start(new CancellationToken());
		}

		private static SwitchCase GetSwitchCase(AsyncState state, GotoExpression breakExpression, ExpressionVisitor visitor)
		{
			var stateExpressions = state.Expressions.Select(e => ReplaceVariablePlaceholder(e, visitor));

			return Expression.SwitchCase(Expression.Block(stateExpressions.Concat(new [] { breakExpression })), Expression.Constant(state.Id));
		}

		private static Expression ReplaceVariablePlaceholder(Expression expression, ExpressionVisitor visitor)
		{
			return visitor.Visit(expression);
		}

		private class ReplaceVariablePlaceholderVisitor : ExpressionVisitor
		{
			private readonly Expression _stateExpression;

			public ReplaceVariablePlaceholderVisitor(Expression stateExpression)
			{
				_stateExpression = stateExpression;
			}

			protected override Expression VisitMember(MemberExpression node)
			{
				if (node.Member.DeclaringType.IsConstructedGenericType && node.Member.DeclaringType.GetGenericTypeDefinition() == typeof(VariablePlaceholder<>))
				{
					var constExpression = node.Expression as ConstantExpression;
					var placeholder = constExpression.Value as VariablePlaceholder;

					return Expression.Field(_stateExpression, placeholder.Field);
				}

				return base.VisitMember(node);
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