using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using TerrificNet.Thtml.Emit.Compiler;
using Xunit;
using Xunit.Sdk;

namespace TerrificNet.Thtml.Test
{
	public class AsyncExpressionBuilderTest
	{
		[Theory]
		[MethodData(nameof(OnlySyncCall))]
		[MethodData(nameof(OnlyAsyncCall))]
		[MethodData(nameof(SyncThanAsync))]
		[MethodData(nameof(AsyncThanSync))]
		[MethodData(nameof(AsyncThanAsync))]
		public async Task AsyncExpressionBuilder_TestBehavior(Action<Mock> training)
		{
			var underTest = new AsyncExpressionBuilder();
			var mock = new Mock(underTest);
			training(mock);

			var result = underTest.Compile();
			var resultTask = result();

			mock.NotifyAll();

			await resultTask;

			mock.Verify();
		}

		[Fact]
		public async Task AsyncExpressionBuilder_LabelBeforeAwait()
		{
			var underTest = new AsyncExpressionBuilder();
			var mock = new Mock(underTest, /*"async", */"async", "sync", "async", "sync");

			var variable = underTest.DefineVariable(typeof(int));
			var labelTarget = Expression.Label("gugus");
			underTest.Add(Expression.Assign(variable, Expression.Constant(0)));
			underTest.Add(Expression.Label(labelTarget));
			underTest.Add(Expression.Assign(variable, Expression.Increment(variable)));

			mock.AddAsync();
			mock.AddSync();

			underTest.Add(Expression.Condition(Expression.Equal(variable, Expression.Constant(1)), Expression.Goto(labelTarget), Expression.Empty()));

			var result = underTest.Compile();
			var awaiter = result();

			mock.NotifyAll();

			await awaiter;

			mock.Verify();
		}

		[Fact]
		public async Task AsyncExpressionBuilder_ForeachIterator()
		{
			var underTest = new AsyncExpressionBuilder();
			var mock = new Mock(underTest, "async", "sync", "async", "sync", "async", "sync");

			var list = new List<string> { "1", "2", "3" };

			var collection = Expression.Constant(list);
			var item = Expression.Parameter(typeof(string));

			Action<Expression> loop = ex =>
			{
				mock.AddAsync();
				mock.AddSync();
			};

			underTest.Foreach(collection, loop);

			var result = underTest.Compile();
			var awaiter = result();

			mock.NotifyAll();

			await awaiter;

			mock.Verify();
		}


		private static void OnlySyncCall(Mock obj)
		{
			obj.TrainSync();
		}

		private static void OnlyAsyncCall(Mock obj)
		{
			obj.TrainAsync();
		}

		private static void SyncThanAsync(Mock obj)
		{
			obj.TrainSync();
			obj.TrainAsync();
		}

		private static void AsyncThanSync(Mock obj)
		{
			obj.TrainAsync();
			obj.TrainSync();
		}

		private static void AsyncThanAsync(Mock obj)
		{
			obj.TrainAsync();
			obj.TrainAsync();
		}

		private class MethodDataAttribute : DataAttribute
		{
			private readonly string _action;

			public MethodDataAttribute(string actionName)
			{
				_action = actionName;
			}

			public override IEnumerable<object[]> GetData(MethodInfo testMethod)
			{
				yield return new object[] { testMethod.DeclaringType.GetTypeInfo().GetMethod(_action, BindingFlags.NonPublic | BindingFlags.Static).CreateDelegate(typeof(Action<Mock>)) };
			}
		}

		public class Mock
		{
			private readonly AsyncExpressionBuilder _builder;
			private readonly Queue<string> _expected = new Queue<string>();

			private readonly MethodCallExpression _syncCall;
			private readonly MethodCallExpression _asyncCall;

			private readonly Queue<TaskCompletionSource<object>> _asyncSources = new Queue<TaskCompletionSource<object>>();

			public Mock(AsyncExpressionBuilder builder)
			{
				_builder = builder;
				_syncCall = Expression.Call(Expression.Constant(this), typeof(Mock).GetTypeInfo().GetMethod(nameof(Do)));
				_asyncCall = Expression.Call(Expression.Constant(this), typeof(Mock).GetTypeInfo().GetMethod(nameof(DoAsync)));
			}

			public Mock(AsyncExpressionBuilder builder, params string[] expected) : this(builder)
			{
				_expected = new Queue<string>(expected);
			}

			public void NotifyAll()
			{
				while (_asyncSources.Count > 0)
				{
					var result = _expected.Dequeue();
					Assert.Equal("async", result);
					_asyncSources.Dequeue().SetResult(null);
				}
			}

			public void TrainSync()
			{
				AddSync();
				_expected.Enqueue("sync");
			}

			public void AddSync()
			{
				_builder.Add(_syncCall);
			}

			public void TrainAsync()
			{
				AddAsync();
				_expected.Enqueue("async");
				_asyncSources.Enqueue(new TaskCompletionSource<object>());
			}

			public void AddAsync()
			{
				_builder.Add(_asyncCall);
			}

			public void Do()
			{
				Assert.Equal("sync", _expected.Dequeue());
			}

			public Task DoAsync()
			{
				if (_asyncSources.Count == 0)
				{
					var completionSource = new TaskCompletionSource<object>();
					_asyncSources.Enqueue(completionSource);

					return completionSource.Task;
				}

				return _asyncSources.Peek().Task;
			}

			public void Verify()
			{
				Assert.Equal(0, _expected.Count);
			}
		}
	}
}
