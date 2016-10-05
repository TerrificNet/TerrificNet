using System;
using System.Linq.Expressions;
using TerrificNet.Thtml.Rendering;

namespace TerrificNet.Thtml.Emit.Compiler
{
	public class IncrementalDomEmitter : IEmitter<IIncrementalDomTemplate>
	{
		private readonly ParameterExpression _parameterExpression;

		public IncrementalDomEmitter()
		{
			_parameterExpression = Expression.Parameter(typeof(IIncrementalDomRenderer));
			this.OutputExpressionEmitter = new IncrementalDomOutputEmitter(_parameterExpression);
		}

		public IOutputExpressionEmitter OutputExpressionEmitter { get; }

		public IIncrementalDomTemplate WrapResult(CompilerResult result)
		{
			var action = Expression.Lambda<Action<IIncrementalDomRenderer, object>>(result.BodyExpression, _parameterExpression, result.InputExpression).Compile();
			return new IncrementDomTemplate(action);
		}

		private class IncrementDomTemplate : IIncrementalDomTemplate
		{
			private readonly Action<IIncrementalDomRenderer, object> _action;

			public IncrementDomTemplate(Action<IIncrementalDomRenderer, object> action)
			{
				_action = action;
			}

			public void Render(IIncrementalDomRenderer renderer, object data)
			{
				_action(renderer, data);
			}
		}
	}
}