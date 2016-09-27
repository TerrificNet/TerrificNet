using System.Collections.Generic;

namespace TerrificNet.Thtml.Emit.Schema
{
	internal class TrainingCollection
	{
		private readonly Dictionary<BindingPathTemplate, List<ITraining>> _items = new Dictionary<BindingPathTemplate, List<ITraining>>();

		public void Add(BindingPathTemplate path, IBindingResultDescription beforeResult, IBindingResultDescription afterResult, ChangeOperation operation)
		{
			AddTraining(path, new Training(beforeResult, afterResult, operation));
		}

		private void AddTraining(BindingPathTemplate path, ITraining training)
		{
			List<ITraining> trainings;
			if (!_items.TryGetValue(path, out trainings))
			{
				trainings = new List<ITraining>();
				_items.Add(path, trainings);
			}

			trainings.Add(training);
		}

		public void AddNode(BindingPathTemplate path, ChangeOperation operation)
		{
			AddTraining(path, new NodeAddTraining(operation));
		}

		public void RemoveNode(BindingPathTemplate path, ChangeOperation operation)
		{
			AddTraining(path, new NodeRemoveTraining(operation));
		}

		public void MoveNode(BindingPathTemplate path, ChangeOperation operation)
		{
			AddTraining(path, new NodeMoveTraining(operation));
		}

		private class NodeRemoveTraining : ITraining
		{
			private readonly ChangeOperation _operation;

			public NodeRemoveTraining(ChangeOperation operation)
			{
				_operation = operation;
			}

			public IEnumerable<ChangeOperation> IsMatch(object oldValue, object newValue)
			{
				if (oldValue != null && newValue == null)
					yield return _operation;
			}
		}

		public IEnumerable<ChangeOperation> GetChangeOperations(BindingPathTemplate path, object oldValue, object newValue)
		{
			List<ITraining> trainings;
			if (!_items.TryGetValue(path, out trainings))
				yield break;

			foreach (var training in trainings)
			{
				foreach (var changeOperation in training.IsMatch(oldValue, newValue))
					yield return changeOperation;
			}
		}

		private interface ITraining
		{
			IEnumerable<ChangeOperation> IsMatch(object oldValue, object newValue);
		}

		private class NodeMoveTraining : TrainingCollection.ITraining
		{
			private readonly ChangeOperation _operation;

			public NodeMoveTraining(ChangeOperation operation)
			{
				_operation = operation;
			}

			public IEnumerable<ChangeOperation> IsMatch(object oldValue, object newValue)
			{
				if (oldValue != null && newValue != null)
					yield return _operation;
			}
		}

		private class NodeAddTraining : ITraining
		{
			private readonly ChangeOperation _operation;

			public NodeAddTraining(ChangeOperation operation)
			{
				_operation = operation;
			}

			public IEnumerable<ChangeOperation> IsMatch(object oldValue, object newValue)
			{
				if (newValue != null && oldValue == null)
					yield return _operation;
			}
		}

		private class Training : ITraining
		{
			private IBindingResultDescription Before { get; }
			private IBindingResultDescription After { get; }
			private ChangeOperation ChangeOperation { get; }

			public Training(IBindingResultDescription before, IBindingResultDescription after, ChangeOperation changeOperation)
			{
				Before = before;
				After = after;
				ChangeOperation = changeOperation;
			}

			public IEnumerable<ChangeOperation> IsMatch(object oldValue, object newValue)
			{
				if (After.IsMatch(newValue) && Before.IsMatch(oldValue))
					yield return ChangeOperation;
			}
		}
	}

	internal interface IBindingResultDescription
	{
		bool IsMatch(object newValue);
	}
}