using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

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
			return GetOrCreate(() => new BooleanDataScope()).BindBoolean();
		}

		public IEvaluator<IEnumerable> BindEnumerable(out IDataScope childScope)
		{
			return GetOrCreate(() => new IterableDataScope()).BindEnumerable(out childScope);
		}

		private class IterableDataScope : DataScopeBuildStrategy
		{
			private readonly DataScope _childScope = new DataScope();
			private readonly bool _nullable;

			public IterableDataScope() : this(false)
			{
			}

			public IterableDataScope(bool nullable)
			{
				_nullable = nullable;
			}

			public override IEvaluator<IEnumerable> BindEnumerable(out IDataScope childScope)
			{
				childScope = _childScope;
				return null;
			}

			public override IEvaluator<bool> BindBoolean()
			{
				throw new ContextException("The iterable member was already called without boolean check.");
			}

			public override DataSchema GetSchema()
			{
				return new IterableDataSchema(_childScope.GetSchema(), _nullable);
			}
		}

		public DataSchema GetSchema()
		{
			return _strategy != null ? _strategy.GetSchema() : DataSchema.Empty;
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
			private bool _nullable = false;

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
				return new ComplexDataSchema(_childScopes.Select(kv => new DataSchemaProperty(kv.Key, kv.Value.GetSchema())), _nullable);
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
			private DataScopeBuildStrategy _strategy;

			public override IEvaluator<bool> BindBoolean()
			{
				return null;
			}

			public override IEvaluator<IEnumerable> BindEnumerable(out IDataScope childScope)
			{
				_strategy = new IterableDataScope(true);
				return _strategy.BindEnumerable(out childScope);
			}

			public override DataSchema GetSchema()
			{
				return _strategy != null ? _strategy.GetSchema() : DataSchema.Boolean;
			}
		}

	}

	[Serializable]
	internal class ContextException : Exception
	{
		public ContextException()
		{
		}

		public ContextException(string message) : base(message)
		{
		}

		public ContextException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected ContextException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}