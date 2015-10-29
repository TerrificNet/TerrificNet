using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TerrificNet.Thtml.Parsing;

namespace TerrificNet.Thtml.Emit.Schema
{
	public class DataScope : IDataScope
	{
		private DataScopeBuildStrategy _strategy;
		internal readonly List<SyntaxNode> DependentNodes = new List<SyntaxNode>();

		private DataScopeBuildStrategy GetOrCreate(Func<DataScopeBuildStrategy> creation)
		{
			if (_strategy == null)
				_strategy = creation();

			return _strategy;
		}

		public IDataScope Property(string propertyName, SyntaxNode node)
		{
			return GetOrCreate(() => new ComplexDataScope()).Property(propertyName, node);
		}

		public IEvaluator<string> BindString()
		{
			return GetOrCreate(() => new StringDataScope()).BindString();
		}

		public IEvaluator<bool> BindBoolean()
		{
			return GetOrCreate(() => new BooleanDataScope(this)).BindBoolean();
		}

		public IEvaluator<IEnumerable> BindEnumerable(out IDataScope childScope)
		{
			return GetOrCreate(() => new IterableDataScope(this)).BindEnumerable(out childScope);
		}

		public DataSchema GetSchema()
		{
			return _strategy != null ? _strategy.GetSchema() : DataSchema.Empty;
		}

		private class IterableDataScope : DataScopeBuildStrategy
		{
			private readonly DataScope _childScope = new DataScope();
			private readonly bool _nullable;
			private readonly DataScope _dataScope;

			public IterableDataScope(DataScope dataScope) : this(dataScope, false)
			{
			}

			public IterableDataScope(DataScope dataScope, bool nullable)
			{
				_dataScope = dataScope;
				_nullable = nullable;
			}

			public override IEvaluator<IEnumerable> BindEnumerable(out IDataScope childScope)
			{
				childScope = _childScope;
				return null;
			}

			public override IEvaluator<bool> BindBoolean()
			{
				throw new DataContextException("The iterable member was already called without boolean check.", _dataScope.DependentNodes.ToArray());
			}

			public override DataSchema GetSchema()
			{
				return new IterableDataSchema(_childScope.GetSchema(), _nullable);
			}
		}

		private abstract class DataScopeBuildStrategy : IDataScope
		{
			public virtual IDataScope Property(string propertyName, SyntaxNode node)
			{
				throw new NotSupportedException();
			}

			public virtual IEvaluator<string> BindString()
			{
				throw new NotSupportedException();
			}

			public virtual IEvaluator<bool> BindBoolean()
			{
				throw new NotSupportedException();
			}

			public virtual IEvaluator<IEnumerable> BindEnumerable(out IDataScope childScope)
			{
				throw new NotSupportedException();
			}

			public abstract DataSchema GetSchema();
		}

		private class ComplexDataScope : DataScopeBuildStrategy
		{
			private readonly Dictionary<string, DataScope> _childScopes = new Dictionary<string, DataScope>();
			private bool _nullable = false;

			public override IDataScope Property(string propertyName, SyntaxNode node)
			{
				DataScope scope;
				if (!_childScopes.TryGetValue(propertyName, out scope))
				{
					scope = new DataScope();
					_childScopes.Add(propertyName, scope);
				}

				scope.DependentNodes.Add(node);
				return scope;
			}

			public override DataSchema GetSchema()
			{
				return new ComplexDataSchema(_childScopes.Select(kv => new DataSchemaProperty(kv.Key, kv.Value.GetSchema(), kv.Value.DependentNodes)), _nullable);
			}
		}

		private class StringDataScope : DataScopeBuildStrategy
		{
			public override IEvaluator<string> BindString()
			{
				return null;
			}

			public override DataSchema GetSchema()
			{
				return DataSchema.String;
			}
		}

		private class BooleanDataScope : DataScopeBuildStrategy
		{
			private readonly DataScope _dataScope;
			private DataScopeBuildStrategy _strategy;

			public BooleanDataScope(DataScope dataScope)
			{
				_dataScope = dataScope;
			}

			public override IEvaluator<bool> BindBoolean()
			{
				return null;
			}

			public override IEvaluator<IEnumerable> BindEnumerable(out IDataScope childScope)
			{
				_strategy = new IterableDataScope(_dataScope, true);
				return _strategy.BindEnumerable(out childScope);
			}

			public override DataSchema GetSchema()
			{
				return _strategy != null ? _strategy.GetSchema() : DataSchema.Boolean;
			}
		}

	}
}