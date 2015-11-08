using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using TerrificNet.Thtml.Emit;

namespace TerrificNet.Thtml.Test
{
	public class NullDataScope : IDataBinder
	{
		public IDataBinder Property(string propertyName)
		{
			return null;
		}

		public IEvaluator<string> BindString()
		{
			return null;
		}

		public Expression BindStringToExpression(Expression dataContext)
		{
			return Expression.Empty();
		}

		public Expression BindBooleanToExpression(Expression dataContext)
		{
			return Expression.Empty();
		}

		public IEvaluator<bool> BindBoolean()
		{
			return null;
		}

		public IEvaluator<IEnumerable> BindEnumerable(out IDataBinder childScope)
		{
			childScope = new NullDataScope();
			return null;
		}

		public Expression BindEnumerableToExpression(Expression dataContext)
		{
			return Expression.Empty();
		}

		public Type DataContextType => typeof (object);
	}
}