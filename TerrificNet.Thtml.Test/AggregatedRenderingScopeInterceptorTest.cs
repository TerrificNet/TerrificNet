using System;
using Moq;
using TerrificNet.Thtml.Emit.Compiler;
using Xunit;

namespace TerrificNet.Thtml.Test
{
	public class AggregatedRenderingScopeInterceptorTest
	{
		[Fact]
		public void AggregatedRenderingScope_SingleInterceptor_CallsSingleInterceptor()
		{
			bool isCompleted = false;
			Action completeAction = () => isCompleted = true; 

			var interceptorMock = new Mock<IRenderingScopeInterceptor>();
			interceptorMock.Setup(s => s.Intercept(null, null, It.IsAny<Action>()))
				.Callback<IRenderingScope, IExpressionBuilder, Action>((s, e, a) => a());

			var underTest = new AggregatedRenderingScopeInterceptor(new[] {interceptorMock.Object});
			underTest.Intercept(null, null, completeAction);

			interceptorMock.Verify();
			Assert.True(isCompleted);
		}

		[Fact]
		public void AggregatedRenderingScope_ServeralInterceptor_CallsInterceptorInRightOrder()
		{
			bool isCompleted = false;
			Action completeAction = () => isCompleted = true;

			var sequence = new MockSequence();
			var interceptorMock1 = new Mock<IRenderingScopeInterceptor>();
			interceptorMock1.InSequence(sequence).Setup(s => s.Intercept(null, null, It.IsAny<Action>()))
				.Callback<IRenderingScope, IExpressionBuilder, Action>((s, e, a) => a());

			var interceptorMock2 = new Mock<IRenderingScopeInterceptor>();
			interceptorMock2.InSequence(sequence).Setup(s => s.Intercept(null, null, It.IsAny<Action>()))
				.Callback<IRenderingScope, IExpressionBuilder, Action>((s, e, a) => a());

			var underTest = new AggregatedRenderingScopeInterceptor(new[] { interceptorMock1.Object, interceptorMock2.Object });
			underTest.Intercept(null, null, completeAction);

			interceptorMock1.VerifyAll();
			interceptorMock2.VerifyAll();

			Assert.True(isCompleted);
		}
	}
}
