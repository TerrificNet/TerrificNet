using TerrificNet.Thtml.VDom.Builder;

namespace TerrificNet.Thtml.VDom
{
	public class VDomBuilder : IVDomBuilder
	{
		internal readonly ElementListBuilderScope ListBuilderScope;
		internal readonly PropertyBuilderScope PropertyBuilderScope;

		private IVDomBuilder _scope;

		public VDomBuilder()
		{
			ListBuilderScope = new ElementListBuilderScope(this);
			PropertyBuilderScope = new PropertyBuilderScope(this);

			_scope = ListBuilderScope;
		}

		internal IVDomBuilder ChangeScope(IVDomBuilder scope)
		{
			_scope = scope;
			return _scope;
		}

		public VTree ToDom()
		{
			return new VNode(ListBuilderScope.GetChildren());
		}

		public void ElementOpen(string tagName)
		{
			_scope.ElementOpen(tagName);
		}

		public void ElementClose()
		{
			_scope.ElementClose();
		}

		public void PropertyStart(string propertyName)
		{
			_scope.PropertyStart(propertyName);
		}

		public void PropertyEnd()
		{
			_scope.PropertyEnd();
		}

		public void Value(object value)
		{
			_scope.Value(value);
		}

		public void ElementOpenStart(string tagName)
		{
			_scope.ElementOpenStart(tagName);
		}

		public void ElementOpenEnd()
		{
			_scope.ElementOpenEnd();
		}
	}
}
