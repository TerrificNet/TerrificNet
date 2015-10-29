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
			return GetOrCreate(() => new ComplexDataScopeContract()).Property(propertyName, node);
		}

		public IEvaluator<string> RequiresString()
		{
			return GetOrCreate(() => new StringDataScopeContract()).RequiresString();
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

		private class LateEvalutor : IEvaluator<IEnumerable>
		{
			private readonly DataScopeContract _dataScopeContract;

			public LateEvalutor(DataScopeContract dataScopeContract)
			{
				_dataScopeContract = dataScopeContract;
			}

			public IEnumerable Evaluate(IDataContext context)
			{
				//var schema = _dataScopeContract.CompleteSchema();
				throw new NotImplementedException();
			}
		}

		private class IterableDataScopeContract : DataScopeContractBuildStrategy
		{
			private readonly DataScopeContract _childScopeContract = new DataScopeContract("item");
			private readonly bool _nullable;
			private readonly DataScopeContract _dataScopeContract;

			public IterableDataScopeContract(DataScopeContract dataScopeContract, bool nullable = false)
			{
				_dataScopeContract = dataScopeContract;
				_nullable = nullable;
			}

			public override IEvaluator<IEnumerable> RequiresEnumerable(out IDataScopeContract childScopeContract)
			{
				childScopeContract = _childScopeContract;
				return new LateEvalutor(_dataScopeContract);
			}

			public override IEvaluator<bool> RequiresBoolean()
			{
				throw new DataContextException("The iterable member was already called without boolean check.", _dataScopeContract.DependentNodes.ToArray());
			}

			public override DataSchema GetSchema()
			{
				return new IterableDataSchema(_childScopeContract.CompleteSchema(), _nullable);
			}
		}

		private abstract class DataScopeContractBuildStrategy : IDataScopeContract
		{
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

		private class ComplexDataScopeContract : DataScopeContractBuildStrategy
		{
			private readonly Dictionary<string, DataScopeContract> _childScopes = new Dictionary<string, DataScopeContract>();
			private bool _nullable = false;

			public override IDataScopeContract Property(string propertyName, SyntaxNode node)
			{
				DataScopeContract scopeContract;
				if (!_childScopes.TryGetValue(propertyName, out scopeContract))
				{
					scopeContract = new DataScopeContract(propertyName);
					_childScopes.Add(propertyName, scopeContract);
				}

				scopeContract.DependentNodes.Add(node);
				return scopeContract;
			}

			public override DataSchema GetSchema()
			{
				return new ComplexDataSchema(_childScopes.Select(kv => new DataSchemaProperty(kv.Key, kv.Value.CompleteSchema(), kv.Value.DependentNodes)), _nullable);
			}
		}

		private class StringDataScopeContract : DataScopeContractBuildStrategy
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

		private class BooleanDataScopeContract : DataScopeContractBuildStrategy
		{
			private readonly DataScopeContract _dataScopeContract;
			private DataScopeContractBuildStrategy _strategy;

			public BooleanDataScopeContract(DataScopeContract dataScopeContract)
			{
				_dataScopeContract = dataScopeContract;
			}

			public override IEvaluator<bool> RequiresBoolean()
			{
				return null;
			}

			public override IEvaluator<IEnumerable> RequiresEnumerable(out IDataScopeContract childScopeContract)
			{
				_strategy = new IterableDataScopeContract(_dataScopeContract, true);
				return _strategy.RequiresEnumerable(out childScopeContract);
			}

			public override DataSchema GetSchema()
			{
				return _strategy != null ? _strategy.GetSchema() : DataSchema.Boolean;
			}
		}

	}
}