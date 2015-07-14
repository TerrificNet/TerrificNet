using System;
using System.Collections.Generic;

using TerrificNet.ViewEngine.TemplateHandler.Grid;
using Veil;
using Xunit;

namespace TerrificNet.ViewEngine.Test.Grid
{
	
	public class GridHelperHandlerTest
	{
		[Fact]
		public void TestCalculation1_3()
		{
			var underTest = new GridHelperHandler();

			var renderingContext = new RenderingContext(null);
			var gridStack = GridStack.FromContext(renderingContext);
			gridStack.Push(960);

			underTest.EvaluateAsync(null, renderingContext, new Dictionary<string, string>
			{
			    {"ratio", "1/3"}
			});

			var result = gridStack.Current;
			Assert.Equal(320, result.Width);
		}
	}
}
