using System;
using System.Collections.Generic;
using TerrificNet.Thtml.Emit;
using Xunit;

namespace TerrificNet.Thtml.Test
{
    public class DataBinderTest
    {
        [Fact]
        public void TypeDataBinder_SimpleProperty()
        {
            const string expectedResult = "property";
            var obj = new { Property = expectedResult };
            
            var underTest = TypeDataBinder.BinderFromType(obj.GetType());
            var result = underTest.Evaluate("property");

            Assert.NotNull(result);
            Assert.Equal(typeof(string), result.ResultType);

            var eval = result.CreateEvaluation<string>();
            var propertyResult = eval(new ObjectDataContext(obj));

            Assert.Equal(expectedResult, propertyResult);
        }

        [Fact]
        public void TypeDataBinder_NestedProperty()
        {
            const string expectedResult = "property";
            var obj = new { Property1 = new { Property2 = expectedResult } };

            var underTest = TypeDataBinder.BinderFromType(obj.GetType());
            var result = underTest.Evaluate("property1").Evaluate("property2");

            Assert.NotNull(result);
            Assert.Equal(typeof (string), result.ResultType);

            var eval = result.CreateEvaluation<string>();
            var propertyResult = eval(new ObjectDataContext(obj));

            Assert.Equal(expectedResult, propertyResult);
        }

        [Theory]
        [InlineData(typeof(List<string>), typeof(string))]
        [InlineData(typeof(IEnumerable<string>), typeof(string))]
        public void TypeDataBinder_ItemFromGeneric(Type interfaceType, Type expectedItemType)
        {
            var underTest = TypeDataBinder.BinderFromType(interfaceType);
            var result = underTest.Item();

            Assert.NotNull(result);
            Assert.Equal(expectedItemType, result.ResultType);
        }
    }
}
