﻿using System;
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

		public override MemberInfo FindMember(Type modelType, string name, MemberTypes types)
		{
			name = _namingRule.GetPropertyName(name);
			return base.FindMember(modelType, name, types);
		}

	}
}