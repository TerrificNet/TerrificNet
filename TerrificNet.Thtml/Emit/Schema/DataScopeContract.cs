using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TerrificNet.Thtml.Parsing;

namespace TerrificNet.Thtml.Emit.Schema
{
	public class DataScopeContract : IDataScopeContract
	{
		private DataScopeContractBuildStrategy _strategy;
		internal readonly List<SyntaxNode> DependentNodes = new List<SyntaxNode>();

		public string Name { get; }

		public DataScopeContract(string name)
		{
			Name = name;
		}

		private DataScopeContractBuildStrategy GetOrCreate(Func<DataScopeContractBuildStrategy> creation)
		{
			return _strategy ?? (_strategy = creation());
		}

		public IDataScopeContract Property(string propertyName, SyntaxNode node)
		{
			return GetOrCreate(() => new ComplexDataScopeContract(this)).Property(propertyName, node);
		}

		public IEvaluator<string> RequiresString()
		{
			return GetOrCreate(() => new StringDataScopeContract(this)).RequiresString();
		}

		public IEvaluator<bool> RequiresBoolean()
		{
			return GetOrCreate(() => new BooleanDataScopeContract(this)).RequiresBoolean();
		}

		public IEvaluator<IEnumerable> RequiresEnumerable(out IDataScopeContract childScopeContract)
		{
			return GetOrCreate(() => new IterableDataScopeContract(this)).RequiresEnumerable(out childScopeContract);
		}

		public DataSchema CompleteSchema()
		{
			return _strategy != null ? _strategy.GetSchema() : DataSchema.Any;
		}

		private abstract class DataScopeContractBuildStrategy : IDataScopeContract
		{
			protected readonly DataScopeContract DataScopeContract;

			protected DataScopeContractBuildStrategy(DataScopeContract dataScopeContract)
			{
				DataScopeContract = dataScopeContract;
			}

			public virtual IDataScopeContract Property(string propertyName, SyntaxNode node)
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

			public virtual IEvaluator<IEnumerable> RequiresEnumerable(out IDataScopeContract childScopeContract)
			{
				throw new NotSupportedException();
			}

			public abstract DataSchema GetSchema();
		}

		private class IterableDataScopeContract : ComplexDataScopeContract
		{
			private readonly DataScopeContract _childScopeContract = new DataScopeContract("item");
			private readonly bool _nullable;

			public IterableDataScopeContract(DataScopeContract dataScopeContract, IDictionary<string, DataScopeContract> childScopes, bool nullable = false)
				: base(dataScopeContract, childScopes, nullable)
			{
			}

			public IterableDataScopeContract(DataScopeContract dataScopeContract, bool nullable = false) : base(dataScopeContract)
			{
				_nullable = nullable;
			}

			public override IEvaluator<IEnumerable> RequiresEnumerable(out IDataScopeContract childScopeContract)
			{
				childScopeContract = _childScopeContract;
				return null;
			}

			public override DataSchema GetSchema()
			{
				return new IterableDataSchema(_childScopeContract.CompleteSchema(), GetProperties(), _nullable);
			}
		}

		private class ComplexDataScopeContract : DataScopeContractBuildStrategy
		{
			private readonly IDictionary<string, DataScopeContract> _childScopes = new Dictionary<string, DataScopeContract>();
			private readonly bool _nullable;
			private DataScopeContractBuildStrategy _strategy;

			protected ComplexDataScopeContract(DataScopeContract dataScopeContract, IDictionary<string, DataScopeContract> childScopes, bool nullable = false) 
				: this(dataScopeContract)
			{
				_childScopes = childScopes;
				_nullable = nullable;
			}

			public ComplexDataScopeContract(DataScopeContract dataScopeContract) : base(dataScopeContract)
			{
			}

			public override IDataScopeContract Property(string propertyName, SyntaxNode node)
			{
				if (_strategy != null)
					return _strategy.Property(propertyName, node);

				DataScopeContract scopeContract;
				if (!_childScopes.TryGetValue(propertyName, out scopeContract))
				{
					scopeContract = new DataScopeContract(propertyName);
					_childScopes.Add(propertyName, scopeContract);
				}

				scopeContract.DependentNodes.Add(node);
				return scopeContract;
			}

			public override IEvaluator<bool> RequiresBoolean()
			{
				throw new DataContextException("The iterable member was already called without boolean check.", DataScopeContract.DependentNodes.ToArray());
			}

			public override IEvaluator<IEnumerable> RequiresEnumerable(out IDataScopeContract childScopeContract)
			{
				_strategy = new IterableDataScopeContract(DataScopeContract, _childScopes, _nullable);
				return _strategy.RequiresEnumerable(out childScopeContract);
			}

			public override DataSchema GetSchema()
			{
				return _strategy == null ? new ComplexDataSchema(GetProperties(), _nullable) : _strategy.GetSchema();
			}

			protected IEnumerable<DataSchemaProperty> GetProperties()
			{
				return _childScopes.Select(kv => new DataSchemaProperty(kv.Key, kv.Value.CompleteSchema(), kv.Value.DependentNodes));
			}
		}

		private class StringDataScopeContract : DataScopeContractBuildStrategy
		{
			public StringDataScopeContract(DataScopeContract dataScopeContract) : base(dataScopeContract)
			{
			}

			public override IEvaluator<string> RequiresString()
			{
				return null;
			}

			public override IEvaluator<bool> RequiresBoolean()
			{
				return null;
			}

			public override DataSchema GetSchema()
			{
				return DataSchema.String;
			}
		}

		private class BooleanDataScopeContract : DataScopeContractBuildStrategy
		{
			private DataScopeContractBuildStrategy _strategy;

			public BooleanDataScopeContract(DataScopeContract dataScopeContract) : base(dataScopeContract)
			{
			}

			public override IEvaluator<bool> RequiresBoolean()
			{
				return null;
			}

			public override IEvaluator<string> RequiresString()
			{
				_strategy = new StringDataScopeContract(DataScopeContract);
				return _strategy.RequiresString();
			}

			public override IEvaluator<IEnumerable> RequiresEnumerable(out IDataScopeContract childScopeContract)
			{
				_strategy = new IterableDataScopeContract(DataScopeContract, true);
				return _strategy.RequiresEnumerable(out childScopeContract);
			}

			public override DataSchema GetSchema()
			{
				return _strategy != null ? _strategy.GetSchema() : DataSchema.Boolean;
			}
		}

	}
}