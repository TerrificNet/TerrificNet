using System;
using System.Linq.Expressions;
using TerrificNet.Thtml.Emit.Schema;

namespace TerrificNet.Thtml.Emit
{
	public interface IBinding<T>
	{
		BindingPathTemplate Path { get; }

		void Train(Func<BindingResultDescriptionBuilder<T>, BindingResultDescription<T>> before, Func<BindingResultDescriptionBuilder<T>, BindingResultDescription<T>> after, ChangeOperation operation);

		Expression CreateExpression(Expression dataContext);
	}
}