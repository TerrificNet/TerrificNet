using System;

namespace TerrificNet.Thtml.Emit
{
	public abstract class HelperBinderResult
	{
		public abstract void Visit(HelperParameters helperParameters);

		public static HelperBinderResult Create(Action<HelperParameters> action)
		{
			return new HelperBinderResultAction(action);
		}

		private class HelperBinderResultAction : HelperBinderResult
		{
			private readonly Action<HelperParameters> _action;

			public HelperBinderResultAction(Action<HelperParameters> action)
			{
				_action = action;
			}

			public override void Visit(HelperParameters helperParameters)
			{
				_action(helperParameters);
			}
		}
	}
}