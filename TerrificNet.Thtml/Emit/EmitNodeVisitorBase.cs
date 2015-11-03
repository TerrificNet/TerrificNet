namespace TerrificNet.Thtml.Emit
{
	internal abstract class EmitNodeVisitorBase<TEmit, TConfig> : NodeVisitorBase<TEmit>
	{
		protected IHelperBinder<TEmit, TConfig> HelperBinder { get; }
		protected IDataScopeContract DataScopeContract { get; }

		protected EmitNodeVisitorBase(IDataScopeContract dataScopeContract, IHelperBinder<TEmit, TConfig> helperBinder)
		{
			DataScopeContract = dataScopeContract;
			HelperBinder = helperBinder;
		}
	}
}