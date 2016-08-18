using System;
using System.Collections;
using System.Linq.Expressions;
using TerrificNet.Thtml.Emit.Schema;

namespace TerrificNet.Thtml.Emit
{
	public interface IBinding<T>
	{
		BindingPathTemplate Path { get; }

		Expression CreateExpression(Expression dataContext);
	}

	public static class BindingExtension
	{
		public static void Train(this IBinding<string> binding,
			Func<BindingResultDescriptionBuilder<string>, BindingResultDescription<string>> before,
			Func<BindingResultDescriptionBuilder<string>, BindingResultDescription<string>> after, ChangeOperation operation)
		{
			var tBinding = binding as DataScopeContract.Binding<string>;
			if (tBinding == null)
				return;

			var descriptionBuilder = new BindingResultDescriptionBuilder<string>();
			var beforeResult = before(descriptionBuilder);
			var afterResult = after(descriptionBuilder);

			tBinding._collection.Add(tBinding.Path, beforeResult, afterResult, operation);
		}

		public static void TrainMove(this IBinding<IEnumerable> binding, ChangeOperation operation)
		{

		}

		public static void TrainAdd(this IBinding<IEnumerable> binding, ChangeOperation operation)
		{
			var tBinding = binding as DataScopeContract.Binding<IEnumerable>;
			tBinding?._collection.AddNode(tBinding.Path, operation);
		}

		public static void TrainRemove(this IBinding<IEnumerable> binding, ChangeOperation operation)
		{
			var tBinding = binding as DataScopeContract.Binding<IEnumerable>;
			tBinding?._collection.RemoveNode(tBinding.Path, operation);
		}
	}
}