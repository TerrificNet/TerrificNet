using System;
using System.Linq.Expressions;

namespace TerrificNet.Thtml.Emit
{
	public interface IDataBinder
	{
		IDataBinder Property(string propertyName);
		IDataBinder Item();

		Expression BindString(Expression dataContext);
		Expression BindBoolean(Expression dataContext);
		Expression BindEnumerable(Expression dataContext);

		Type DataContextType { get; }
	}
}