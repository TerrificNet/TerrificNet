using System;
using TerrificNet.Thtml.Emit.Schema;
using Xunit;

namespace TerrificNet.Thtml.Test
{
	public class BindingPathTemplateTest
	{
		[Fact]
		public void BindingPathTemplate_CreatePathFromGlobal_ReturnsEmptyPath()
		{
			var template = BindingPathTemplate.Global;
			var path = template.GetPath();

			Assert.NotNull(path);
			Assert.Empty(path.Segments);
		}

		[Fact]
		public void BindingPathTemplate_CreatePathFromProperty_ReturnsProperty()
		{
			const string propertyName = "$scope";
			var template = BindingPathTemplate.Global.Property(propertyName);
			var path = template.GetPath();

			Assert.NotNull(path);
			Assert.Equal(new [] { propertyName }, path.Segments);
		}

		[Fact]
		public void BindingPathTemplate_CreatePathFromPropertyThenProperty_ReturnsPropertyProperty()
		{
			const string propertyName1 = "$scope";
			const string propertyName2 = "prop";
			var template = BindingPathTemplate.Global
				.Property(propertyName1)
				.Property(propertyName2);

			var path = template.GetPath();

			Assert.NotNull(path);
			Assert.Equal(new [] { propertyName1, propertyName2 }, path.Segments);
		}

		[Fact]
		public void BindingPathTemplate_CreatePathFromItem_ThrowsNotSupportedException()
		{
			const string propertyName1 = "$scope";
			var template = BindingPathTemplate.Global
				.Property(propertyName1)
				.Item();

			Assert.Throws<NotSupportedException>(() => template.GetPath());
		}

	}
}
