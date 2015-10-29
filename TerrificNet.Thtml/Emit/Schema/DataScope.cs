using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace TerrificNet.Thtml.Emit.Schema
{
	public class DataScope : IDataScope
	{
		private DataScopeBuildStrategy _strategy;

		private DataScopeBuildStrategy GetOrCreate(Func<DataScopeBuildStrategy> creation)
		{
			if (_strategy == null)
				_strategy = creation();

			return _strategy;
		}

		public IDataScope Property(string propertyName)
		{
			return GetOrCreate(() => new ComplexDataScope()).Property(propertyName);
		}

		public IEvaluator<string> BindString()
		{
			return GetOrCreate(() => new StringDataScope()).BindString();
		}

		public IEvaluator<bool> BindBoolean()
		{
			throw new NotImplementedException();
		}

		public IEvaluator<IEnumerable> BindEnumerable(out IDataScope childScope)
		{
			throw new NotImplementedException();
		}

		public DataSchema GetSchema()
		{
			return _strategy == null ? new DataSchema() : _strategy.GetSchema();
		}

		private abstract class DataScopeBuildStrategy : IDataScope
		{
			public virtual IDataScope Property(string propertyName)
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

			public override IDataScope Property(string propertyName)
			{
				DataScope scope;
				if (!_childScopes.TryGetValue(propertyName, out scope))
				{
					scope = new DataScope();
					_childScopes.Add(propertyName, scope);
				}

				return scope;
			}

			public override DataSchema GetSchema()
			{
				return new ComplexDataSchema(_childScopes.Select(kv => new DataSchemaProperty(kv.Key, kv.Value.GetSchema())));
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
	}
}