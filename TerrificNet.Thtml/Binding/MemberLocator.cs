using System;
using System.Linq;
using System.Reflection;

namespace TerrificNet.Thtml.Binding
{
	public class MemberLocator : IMemberLocator
	{
		public static readonly IMemberLocator Default = new MemberLocator();

		protected MemberLocator()
		{
		}

		public virtual PropertyInfo FindProperty(Type modelType, string expression)
		{
			return
				modelType.GetRuntimeProperties()
					.FirstOrDefault(p => p.Name.Equals(expression, StringComparison.OrdinalIgnoreCase));
		}

		public virtual FieldInfo FindField(Type modelType, string expression)
		{
			return
				modelType.GetRuntimeFields()
					.FirstOrDefault(p => p.Name.Equals(expression, StringComparison.OrdinalIgnoreCase));
		}
	}
}