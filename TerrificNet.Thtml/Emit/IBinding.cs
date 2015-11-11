using System;
using System.Linq.Expressions;

namespace TerrificNet.Thtml.Emit
{
	public interface IBinding<T>
	{
		void Train(Func<BindingResultDescriptionBuilder<T>, BindingResultDescription<T>> before, Func<BindingResultDescriptionBuilder<T>, BindingResultDescription<T>> after, string operation);

		Expression CreateExpression(Expression dataContext);
	}
}