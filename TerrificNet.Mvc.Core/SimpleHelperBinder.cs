using System.Collections.Generic;
using System.Linq.Expressions;
using TerrificNet.Thtml.Emit;
using TerrificNet.Thtml.Parsing;

namespace TerrificNet.Mvc.Core
{
	public class SimpleHelperBinder : IHelperBinder
	{
		public HelperBinderResult FindByName(string helper, IDictionary<string, string> arguments)
		{
			return new SimpleHelperBinderResult(helper);
		}

		private class SimpleHelperBinderResult : HelperBinderResult
		{
			private readonly string _helper;

			public SimpleHelperBinderResult(string helper)
			{
				_helper = helper;
			}

			public override Expression CreateExpression(HelperParameters helperParameters)
			{
				return helperParameters.Visitor.Visit(new TextNode(_helper));
			}
		}
	}
}