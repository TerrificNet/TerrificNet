using System.Collections.Generic;

namespace TerrificNet.Thtml.VDom.Builder
{
	internal class ElementListBuilderScope : VDomBuilderScope
	{
		private readonly Stack<string> _elementNameStack = new Stack<string>();

		private readonly Stack<List<VTree>> _childElementStack = new Stack<List<VTree>>();

		private readonly Stack<List<VProperty>> _propertyStack = new Stack<List<VProperty>>();

		public ElementListBuilderScope(VDomBuilder builder) : base(builder)
		{
			_childElementStack.Push(new List<VTree>());
		}

		protected override string ScopeName => "ElementList";

		public override void ElementOpen(string tagName)
		{
			_elementNameStack.Push(tagName);
			_childElementStack.Push(new List<VTree>());
			_propertyStack.Push(null);
		}

		public override void ElementClose()
		{
			var children = _childElementStack.Pop();
			var properties = _propertyStack.Pop();
			_childElementStack.Peek().Add(new VElement(_elementNameStack.Pop(), properties, children));
		}

		public override void Value(object value)
		{
			var text = value as string;
			if (text != null)
			{
				_childElementStack.Peek().Add(new VText(text));
			}
		}

		public override void ElementOpenStart(string tagName)
		{
			_elementNameStack.Push(tagName);
			_childElementStack.Push(new List<VTree>());

			Builder.ChangeScope(Builder.PropertyBuilderScope).ElementOpenStart(tagName);
		}

		public override void ElementOpenEnd()
		{
			_propertyStack.Push(Builder.PropertyBuilderScope.CurrentValue);
		}

		public IEnumerable<VTree> GetChildren()
		{
			return _childElementStack.Peek();
		}
	}
}