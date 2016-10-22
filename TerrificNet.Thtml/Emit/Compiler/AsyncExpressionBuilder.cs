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

		public Expression DefineVariable(Type type)
		{
			var call = Expression.Call(_stateMachine,
				typeof(AsyncViewStateMachine).GetTypeInfo()
					.GetMethod(nameof(AsyncViewStateMachine.GetValues))
					.MakeGenericMethod(type));

			var ex = Expression.Property(call, "Item", Expression.Constant(_variableIndex));
			_variableIndex++;

			return ex;
		}

		private Expression AwaitExpression(int nextState, Expression awaiterExpression)
		{
			return Expression.Call(_stateMachine, typeof(AsyncViewStateMachine).GetTypeInfo().GetMethod(nameof(AsyncViewStateMachine.Await)), Expression.Constant(nextState), awaiterExpression);
		}

		public Func<Task> Compile()
		{
			var breakTarget = Expression.Label();
			var breakExpression = Expression.Break(breakTarget);

			var tailCall = Expression.Call(_stateMachine, typeof(AsyncViewStateMachine).GetTypeInfo().GetMethod(nameof(AsyncViewStateMachine.Complete)));
			_states[_states.Count - 1].Expressions.Add(tailCall);

			var switchExpression = Expression.Switch(_currentStateExpression, _states.Select(e => GetSwitchCase(e, breakExpression)).ToArray());
			var bodyExpression = Expression.Block(switchExpression, Expression.Label(breakTarget));

			var lambda = Expression.Lambda<Action<int, AsyncViewStateMachine>>(bodyExpression, _currentStateExpression, _stateMachine);
			var action = lambda.Compile();
			var state = new AsyncViewStateMachine(action);

			return () => state.Start(new CancellationToken());
		}

		private static SwitchCase GetSwitchCase(AsyncState state, GotoExpression breakExpression)
		{
			return Expression.SwitchCase(Expression.Block(state.Expressions.Concat(new [] { breakExpression })), Expression.Constant(state.Id));
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