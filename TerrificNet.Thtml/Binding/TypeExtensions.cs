using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace TerrificNet.Thtml.Binding
{
	public static class TypeExtensions
	{
		private static bool IsDictionaryType(Type t)
		{
			return t.GetTypeInfo().IsGenericType && t.GetGenericTypeDefinition() == typeof(IDictionary<,>);
		}

		public static Type GetDictionaryTypeWithKey(this Type t)
		{
			Type dictionaryType;
			if (IsDictionaryType(t)) dictionaryType = t;
			else dictionaryType = t.GetTypeInfo().GetInterfaces().FirstOrDefault(IsDictionaryType);

			if (dictionaryType == null) return null;
			if (dictionaryType.GetTypeInfo().GetGenericArguments()[0] != typeof(string)) return null;
			return dictionaryType;
		}
	}
}