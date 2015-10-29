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

		public string Name { get; }

		public DataScope(string name)
		{
			Name = name;
		}

		private DataScopeBuildStrategy GetOrCreate(Func<DataScopeBuildStrategy> creation)
		{
			return _strategy ?? (_strategy = creation());
		}

		public IDataScope Property(string propertyName, SyntaxNode node)
		{
			return GetOrCreate(() => new ComplexDataScope()).Property(propertyName, node);
		}

		public IEvaluator<string> RequiresString()
		{
			return GetOrCreate(() => new StringDataScope()).RequiresString();
		}

		public IEvaluator<bool> RequiresBoolean()
		{
			return GetOrCreate(() => new BooleanDataScope(this)).RequiresBoolean();
		}

		public IEvaluator<IEnumerable> RequiresEnumerable(out IDataScope childScope)
		{
			return GetOrCreate(() => new IterableDataScope(this)).RequiresEnumerable(out childScope);
		}

		public DataSchema CompleteSchema()
		{
			return _strategy != null ? _strategy.GetSchema() : DataSchema.Any;
		}

		private class LateEvalutor : IEvaluator<IEnumerable>
		{
			private readonly DataScope _dataScope;

			public LateEvalutor(DataScope dataScope)
			{
				_dataScope = dataScope;
			}

			public IEnumerable Evaluate(IDataContext context)
			{
				//var schema = _dataScope.CompleteSchema();
				throw new NotImplementedException();
			}
		}

		private class IterableDataScope : DataScopeBuildStrategy
		{
			private readonly DataScope _childScope = new DataScope("item");
			private readonly bool _nullable;
			private readonly DataScope _dataScope;

			public IterableDataScope(DataScope dataScope, bool nullable = false)
			{
				_dataScope = dataScope;
				_nullable = nullable;
			}

			public override IEvaluator<IEnumerable> RequiresEnumerable(out IDataScope childScope)
			{
				childScope = _childScope;
				return new LateEvalutor(_dataScope);
			}

			public override IEvaluator<bool> RequiresBoolean()
			{
				throw new DataContextException("The iterable member was already called without boolean check.", _dataScope.DependentNodes.ToArray());
			}

			public override DataSchema GetSchema()
			{
				return new IterableDataSchema(_childScope.CompleteSchema(), _nullable);
			}
		}

		private abstract class DataScopeBuildStrategy : IDataScope
		{
			public virtual IDataScope Property(string propertyName, SyntaxNode node)
			{
				throw new NotSupportedException();
			}

			public virtual IEvaluator<string> RequiresString()
			{
				throw new NotSupportedException();
			}

			public virtual IEvaluator<bool> RequiresBoolean()
			{
				throw new NotSupportedException();
			}

			public virtual IEvaluator<IEnumerable> RequiresEnumerable(out IDataScope childScope)
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
					scope = new DataScope(propertyName);
					_childScopes.Add(propertyName, scope);
				}

				scope.DependentNodes.Add(node);
				return scope;
			}

			public override DataSchema GetSchema()
			{
				return new ComplexDataSchema(_childScopes.Select(kv => new DataSchemaProperty(kv.Key, kv.Value.CompleteSchema(), kv.Value.DependentNodes)), _nullable);
			}
		}

		private class StringDataScope : DataScopeBuildStrategy
		{
			public override IEvaluator<string> RequiresString()
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

			public override IEvaluator<bool> RequiresBoolean()
			{
				return null;
			}

			public override IEvaluator<IEnumerable> RequiresEnumerable(out IDataScope childScope)
			{
				_strategy = new IterableDataScope(_dataScope, true);
				return _strategy.RequiresEnumerable(out childScope);
			}

			public override DataSchema GetSchema()
			{
				return _strategy != null ? _strategy.GetSchema() : DataSchema.Boolean;
			}
		}

	}
}