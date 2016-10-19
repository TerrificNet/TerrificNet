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

			var result = underTest.Complete();

			Assert.NotNull(result);
			Assert.Equal(typeof(Task), result.Type);

			await ExecuteResult(result);

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

		private static Task ExecuteResult(Expression result)
		{
			var exec = Expression.Lambda<Func<Task>>(result).Compile();
			return exec();
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

			public Mock(AsyncExpressionBuilder builder)
			{
				_builder = builder;
				_syncCall = Expression.Call(Expression.Constant(this), typeof(Mock).GetTypeInfo().GetMethod("Do"));
				_asyncCall = Expression.Call(Expression.Constant(this), typeof(Mock).GetTypeInfo().GetMethod("DoAsync"));
			}

			public void TrainSync()
			{
				_builder.Add(_syncCall);
				_expected.Enqueue("sync");
			}

			public void TrainAsync()
			{
				_builder.Add(_asyncCall);
				_expected.Enqueue("async");
			}

			public void Do()
			{
				Assert.Equal("sync", _expected.Dequeue());
			}

			public Task DoAsync()
			{
				Assert.Equal("async", _expected.Dequeue());
				return Task.CompletedTask;
			}

			public void Verify()
			{
				Assert.Equal(0, _expected.Count);
			}
		}
	}
}
