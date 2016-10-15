namespace TerrificNet.Thtml.Parsing
{
	public abstract class SyntaxNode
	{
		private bool? _isFixed;

		// ReSharper disable once UnusedMember.Global
		public string TypeName => GetType().Name;

		public bool IsFixed
		{
			get
			{
				if (_isFixed == null)
					_isFixed = CheckIfIsFixed();

				return _isFixed.Value;
			}
		}

		protected abstract bool CheckIfIsFixed();
	}
}