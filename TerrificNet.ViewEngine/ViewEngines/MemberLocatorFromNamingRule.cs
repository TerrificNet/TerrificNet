using System;
using System.Reflection;
using TerrificNet.Thtml.Binding;

namespace TerrificNet.ViewEngine.ViewEngines
{
	public class MemberLocatorFromNamingRule : MemberLocator
	{
		private readonly INamingRule _namingRule;

		public MemberLocatorFromNamingRule(INamingRule namingRule)
		{
			_namingRule = namingRule;
		}

        public override FieldInfo FindField(Type modelType, string expression)
        {
            return base.FindField(modelType, _namingRule.GetPropertyName(expression));
        }

        public override MethodInfo FindMethod(Type modelType, string name)
        {
            return base.FindMethod(modelType, _namingRule.GetPropertyName(name));
        }

        public override PropertyInfo FindProperty(Type modelType, string expression)
        {
            return base.FindProperty(modelType, _namingRule.GetPropertyName(expression));
        }

    }
}