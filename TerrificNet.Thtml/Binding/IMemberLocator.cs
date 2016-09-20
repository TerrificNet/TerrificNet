using System;
using System.Reflection;

namespace TerrificNet.Thtml.Binding
{
	public interface IMemberLocator
	{
		PropertyInfo FindProperty(Type modelType, string expression);

		FieldInfo FindField(Type modelType, string expression);
	}
}