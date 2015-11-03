using System;

namespace TerrificNet.Thtml.Emit
{
	public interface IEvaluator<out T>
	{
		T Evaluate(object context);
	}

	public interface IBinding<T> : IEvaluator<T>
	{
		void Train(Func<ResultGenerator<T>, Result<T>> before, Func<ResultGenerator<T>, Result<T>> after, string operation);
	}

	public class ResultGenerator<T>
	{
		public Result<T> Any()
		{
			return new Result<T>();
		}

		public Result<T> Exact(T value)
		{
			return new Result<T>();
		}
	}

	public class Result<T>
	{
		internal Result()
		{
		}

		public static Result<T> Any = new Result<T>();

		public static Result<T> Exact(T value)
		{
			return new Result<T>();
		}
	}
}