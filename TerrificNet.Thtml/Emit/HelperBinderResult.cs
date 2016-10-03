using System;
using System.Linq.Expressions;

namespace TerrificNet.Thtml.Emit
{
	public abstract class HelperBinderResult
	{
		public abstract Expression CreateExpression(HelperParameters helperParameters);

		public static HelperBinderResult Create(Func<HelperParameters, Expression> action)
		{
			return new HelperBinderResultAction(action);
		}

		private class HelperBinderResultAction : HelperBinderResult
		{
			private readonly Func<HelperParameters, Expression> _action;

			public HelperBinderResultAction(Func<HelperParameters, Expression> action)
			{
				_action = action;
			}

			public override Expression CreateExpression(HelperParameters helperParameters)
			{
				return _action(helperParameters);
			}
		}
	}
}