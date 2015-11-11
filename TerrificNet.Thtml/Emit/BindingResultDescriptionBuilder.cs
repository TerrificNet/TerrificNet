namespace TerrificNet.Thtml.Emit
{
	public class BindingResultDescriptionBuilder<T>
	{
		public BindingResultDescription<T> Any()
		{
			return new BindingResultDescription<T>();
		}

		public BindingResultDescription<T> Exact(T value)
		{
			return new BindingResultDescription<T>();
		}
	}
}