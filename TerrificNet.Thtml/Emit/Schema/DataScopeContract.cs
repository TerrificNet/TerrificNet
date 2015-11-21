using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using TerrificNet.Thtml.Parsing;

namespace TerrificNet.Thtml.Emit.Schema
{
	public class DataScopeContract : IDataScopeContract
	{
		private DataScopeContractBuildStrategy _strategy;
		internal readonly List<SyntaxNode> DependentNodes = new List<SyntaxNode>();

		private BindingPathTemplate Path { get; }

		public DataScopeContract(BindingPathTemplate path)
		{
			Path = path;
		}

		private DataScopeContractBuildStrategy GetOrCreate(Func<DataScopeContractBuildStrategy> creation)
		{
			return _strategy ?? (_strategy = creation());
		}

		public IDataScopeContract Property(string propertyName, SyntaxNode node)
		{
			return GetOrCreate(() => new ComplexDataScopeContract(this)).Property(propertyName, node);
		}

		public IBinding<string> RequiresString()
		{
			return GetOrCreate(() => new StringDataScopeContract(this)).RequiresString();
		}

		public IBinding<bool> RequiresBoolean()
		{
			return GetOrCreate(() => new BooleanDataScopeContract(this)).RequiresBoolean();
		}

		public IBinding<IEnumerable> RequiresEnumerable(out IDataScopeContract childScopeContract)
		{
			return GetOrCreate(() => new IterableDataScopeContract(this)).RequiresEnumerable(out childScopeContract);
		}

		public Type ResultType { get; }

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

			protected abstract string Name { get; }

			public virtual IDataScopeContract Property(string propertyName, SyntaxNode node)
			{
				throw new DataContractException($"Can not access property {propertyName} on node {node} because {Name} doesn't support properties.", DataScopeContract.DependentNodes.ToArray());
			}

			public virtual IBinding<string> RequiresString()
			{
				throw new DataContractException($"Can not access {DataScopeContract.Path} as string because {Name} doesn't support this conversion.", DataScopeContract.DependentNodes.ToArray());
			}

			public virtual IBinding<bool> RequiresBoolean()
			{
				throw new DataContractException($"The {DataScopeContract.Path} was already called without boolean check.", DataScopeContract.DependentNodes.ToArray());
			}

			public virtual IBinding<IEnumerable> RequiresEnumerable(out IDataScopeContract childScopeContract)
			{
				throw new DataContractException($"Can not access {DataScopeContract.Path} as iterable because {Name} doesn't support this conversion.", DataScopeContract.DependentNodes.ToArray());
			}

			public Type ResultType => DataScopeContract.ResultType;

			public abstract DataSchema GetSchema();
		}

		private class IterableDataScopeContract : ComplexDataScopeContract
		{
			private readonly DataScopeContract _childScopeContract;
			private readonly bool _nullable;

			public IterableDataScopeContract(DataScopeContract dataScopeContract, IDictionary<string, DataScopeContract> childScopes, bool nullable = false)
				: base(dataScopeContract, childScopes, nullable)
			{
				_childScopeContract = new DataScopeContract(dataScopeContract.Path.Item());
			}

			protected override string Name => "Iterable";

			public IterableDataScopeContract(DataScopeContract dataScopeContract, bool nullable = false) : base(dataScopeContract)
			{
				_nullable = nullable;
				_childScopeContract = new DataScopeContract(dataScopeContract.Path.Item());
			}

			public override IBinding<IEnumerable> RequiresEnumerable(out IDataScopeContract childScopeContract)
			{
				childScopeContract = _childScopeContract;
				return new Binding<IEnumerable>(DataScopeContract.Path);
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

			protected override string Name => "Object";

			public override IDataScopeContract Property(string propertyName, SyntaxNode node)
			{
				if (_strategy != null)
					return _strategy.Property(propertyName, node);

				DataScopeContract scopeContract;
				if (!_childScopes.TryGetValue(propertyName, out scopeContract))
				{
					scopeContract = new DataScopeContract(DataScopeContract.Path.Property(propertyName));
					_childScopes.Add(propertyName, scopeContract);
				}

				scopeContract.DependentNodes.Add(node);
				return scopeContract;
			}

			public override IBinding<IEnumerable> RequiresEnumerable(out IDataScopeContract childScopeContract)
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

			protected override string Name => "String";

			public override IBinding<string> RequiresString()
			{
				return new Binding<string>(DataScopeContract.Path);
			}

			public override IBinding<bool> RequiresBoolean()
			{
				return new Binding<bool>(DataScopeContract.Path);
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

			protected override string Name => "Boolean";

			public override IBinding<bool> RequiresBoolean()
			{
				return new Binding<bool>(DataScopeContract.Path);
			}

			public override IBinding<string> RequiresString()
			{
				_strategy = new StringDataScopeContract(DataScopeContract);
				return _strategy.RequiresString();
			}

			public override IBinding<IEnumerable> RequiresEnumerable(out IDataScopeContract childScopeContract)
			{
				_strategy = new IterableDataScopeContract(DataScopeContract, true);
				return _strategy.RequiresEnumerable(out childScopeContract);
			}

			public override DataSchema GetSchema()
			{
				return _strategy != null ? _strategy.GetSchema() : DataSchema.Boolean;
			}
		}

		public IEnumerable<ChangeOperation> PushChange(DataSchemaProperty property, object value)
		{
			throw new NotImplementedException();
		}

		private class Binding<T> : IBinding<T>
		{
			public BindingPathTemplate Path { get; }

			public Binding(BindingPathTemplate path)
			{
				Path = path;
			}

			public void Train(Func<BindingResultDescriptionBuilder<T>, BindingResultDescription<T>> before, Func<BindingResultDescriptionBuilder<T>, BindingResultDescription<T>> after, ChangeOperation operation)
			{
			}

			public Expression CreateExpression(Expression dataContext)
			{
				throw new NotImplementedException();
			}
		}
	}

	public class BindingPathTemplate
	{
		private readonly BindingPathTemplate _parent;

		private BindingPathTemplate(BindingPathTemplate parent)
		{
			_parent = parent;
			Segment = null;
		}

		public static readonly BindingPathTemplate Global = new BindingPathTemplate(null);

		public BindingPathTemplate Property(string property)
		{
			return new PropertyPathTemplate(property, this);
		}

		public BindingPathTemplate Item()
		{
			return new ItemPathTemplate(this);
		}

		protected virtual string Segment { get; }

		public override string ToString()
		{
			var item = this;
			var segments = new List<string>();
			do
			{
				segments.Insert(0, item.Segment);
			} while((item = item._parent) != null);

			return string.Join("/", segments.Where(s => s != null).ToArray());
		}

		private class PropertyPathTemplate : BindingPathTemplate
		{
			public PropertyPathTemplate(string segment, BindingPathTemplate parent) : base(parent)
			{
				Segment = segment;
			}

			protected override string Segment { get; }
		}

		private class ItemPathTemplate : BindingPathTemplate
		{
			public ItemPathTemplate(BindingPathTemplate parent) : base(parent)
			{
			}

			protected override string Segment => "[n]";
		}
	}

	public abstract class ChangeOperation
	{
		
	}
}