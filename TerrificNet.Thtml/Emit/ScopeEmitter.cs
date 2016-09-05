using System;
using TerrificNet.Thtml.Parsing;
using TerrificNet.Thtml.Parsing.Handlebars;

namespace TerrificNet.Thtml.Emit
{
	internal class ScopeEmitter : NodeVisitorBase<IDataScopeContract>
	{
		private IDataScopeContract _dataScopeContract;

		private ScopeEmitter(IDataScopeContract dataScopeContract)
		{
			_dataScopeContract = dataScopeContract;
		}

		public override IDataScopeContract Visit(MemberExpression memberExpression)
		{
			_dataScopeContract = _dataScopeContract.Property(memberExpression.Name, memberExpression);
			if (memberExpression.SubExpression != null)
				return memberExpression.SubExpression.Accept(this);

			return _dataScopeContract;
		}

		public override IDataScopeContract Visit(ParentExpression parentExpression)
		{
			if (_dataScopeContract.Parent == null)
				throw new Exception("No parent defined.");
			
			_dataScopeContract = _dataScopeContract.Parent;
			if (parentExpression.SubExpression != null)
				return parentExpression.SubExpression.Accept(this);

			return _dataScopeContract;
		}

		public override IDataScopeContract Visit(SelfExpression selfExpression)
		{
			return _dataScopeContract;
		}

		public static IDataScopeContract Bind(IDataScopeContract scopeContract, MustacheExpression expression)
		{
			var visitor = new ScopeEmitter(scopeContract);
			return expression.Accept(visitor);
		}
	}
}