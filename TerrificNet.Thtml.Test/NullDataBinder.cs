using System;
using System.Linq.Expressions;
using TerrificNet.Thtml.Emit;

namespace TerrificNet.Thtml.Test
{
	public class NullDataBinder : IDataBinder
	{
		public IDataBinder Property(string propertyName)
		{
			return null;
		}

		public Expression BindString(Expression dataContext)
		{
			return Expression.Empty();
		}

		public Expression BindBoolean(Expression dataContext)
		{
			return Expression.Empty();
		}

		public IDataBinder Item()
		{
			IDataBinder childScope = new NullDataBinder();
			return childScope;
		}

		public Expression BindEnumerable(Expression dataContext)
		{
			return Expression.Empty();
		}

		public Type DataContextType => typeof (object);
	}
}