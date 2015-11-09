namespace TerrificNet.Thtml.Emit
{
	internal abstract class EmitNodeVisitorBase<TEmit, TConfig> : NodeVisitorBase<TEmit>
	{
		protected IHelperBinder HelperBinder { get; }
		protected IDataScopeContract DataScopeContract { get; }

		protected EmitNodeVisitorBase(IDataScopeContract dataScopeContract, IHelperBinder helperBinder)
		{
			DataScopeContract = dataScopeContract;
			HelperBinder = helperBinder;
		}
	}
}