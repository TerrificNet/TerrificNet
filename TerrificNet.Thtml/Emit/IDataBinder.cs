using System;
using System.Collections;

namespace TerrificNet.Thtml.Emit
{
	public interface IDataBinder
	{
		IDataBinder Property(string propertyName);
		IDataBinder Item();
		bool TryCreateEvaluation<T>(out IEvaluator<T> evaluationFunc);
	}

	public static class DataBinderExtension
	{
		public static IEvaluator<string> BindString(this IDataBinder dataBinder)
		{
			IEvaluator<string> evaluator;
			if (dataBinder.TryBindString(out evaluator))
				return evaluator;

			throw new Exception($"Can not bind a string to the current binder ${dataBinder}.");
		}

		private static bool TryBindString(this IDataBinder dataBinder, out IEvaluator<string> evaluationFunc)
		{
			return dataBinder.TryCreateEvaluation(out evaluationFunc);
		}

		public static IEvaluator<IEnumerable> BindEnumerable(this IDataBinder dataBinder, out IDataBinder childScope)
		{
			childScope = dataBinder.Item();

			IEvaluator<IEnumerable> evaluator;
			if (dataBinder.TryBindEnumerable(out evaluator))
				return evaluator;

			throw new Exception($"Can not bind a enumerable to the current binder ${dataBinder}.");
		}

		private static bool TryBindEnumerable(this IDataBinder dataBinder, out IEvaluator<IEnumerable> evaluationFunc)
		{
			return dataBinder.TryCreateEvaluation(out evaluationFunc);
		}

		public static IEvaluator<bool> BindBoolean(this IDataBinder dataBinder)
		{
			IEvaluator<bool> evaluator;
			if (dataBinder.TryBindBoolean(out evaluator))
				return evaluator;

			throw new Exception($"Can not bind a boolean to the current binder ${dataBinder}.");
		}

		private static bool TryBindBoolean(this IDataBinder dataBinder, out IEvaluator<bool> evaluationFunc)
		{
			return dataBinder.TryCreateEvaluation(out evaluationFunc);
		}
	}
}