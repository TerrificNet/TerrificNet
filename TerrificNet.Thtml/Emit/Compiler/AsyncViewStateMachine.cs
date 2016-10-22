using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TerrificNet.Thtml.Emit.Compiler
{
	internal class AsyncViewStateMachine
	{
		private readonly Action<int, AsyncViewStateMachine> _action;
		private TaskCompletionSource<object> _builder;
		private int _state;

		public AsyncViewStateMachine(Action<int, AsyncViewStateMachine> action)
		{
			_action = action;
		}

		public Dictionary<int, int> IntVariables = new Dictionary<int, int>();

		public Task Start(CancellationToken cancellationToken)
		{
			_builder = new TaskCompletionSource<object>();
			_state = -1;
			MoveNext();
			return _builder.Task;
		}

		private void MoveNext()
		{
			try
			{
				_action(_state, this);
			}
			catch (Exception ex)
			{
				_state = -2;
				_builder.SetException(ex);
			}
		}

		public void Complete()
		{
			_state = -2;
			_builder.SetResult(null);
		}

		public void Await(int state, Task task)
		{
			var completion = task.GetAwaiter();

			_state = state;
			if (!completion.IsCompleted)
			{
				completion.OnCompleted(MoveNext);
			}
			else
			{
				MoveNext();
			}
		}
	}
}