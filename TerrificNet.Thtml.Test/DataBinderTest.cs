using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TerrificNet.Thtml.Emit;
using Xunit;

namespace TerrificNet.Thtml.Test
{
    public class DataBinderTest
    {
        // ReSharper disable once UnusedMember.Global
        public static IEnumerable<object[]> BinderFactoriesParameter
        {
            get { return BinderFactories.Select(s => new object[] {s}); }
        }

        private static IEnumerable<Func<Type, IDataBinder>> BinderFactories
        {
            get
            {
                yield return TypeDataBinder.BinderFromType;
                yield return t => new DynamicDataBinder();
            }
        }

        [Theory]
        [MemberData("BinderFactoriesParameter")]
        public void DataBinder_SimpleProperty(Func<Type, IDataBinder> dataBinderFactory)
        {
            const string expectedResult = "property";
            var obj = new { Property = expectedResult };
            
            var underTest = dataBinderFactory(obj.GetType());
            var result = underTest.Property("property");

            Assert.NotNull(result);
            var evaluator = result.BindString();
            var propertyResult = evaluator.Evaluate(new ObjectDataContext(obj));

            Assert.Equal(expectedResult, propertyResult);
        }

        [Theory]
        [MemberData("BinderFactoriesParameter")]
        public void DataBinder_IterationProperty(Func<Type, IDataBinder> dataBinderFactory)
        {
            const string expectedResult = "property";
            var obj = new { Property = new [] { new { Property2 = expectedResult } } };

            var underTest = dataBinderFactory(obj.GetType());
            var result = underTest.Property("property");

            Assert.NotNull(result);

            var evaluator = result.BindEnumerable();
            var propertyResult = evaluator.Evaluate(new ObjectDataContext(obj));

            Assert.NotNull(propertyResult);

            var itemResult = result.Item();
            var innerPropertyResult = itemResult.Property("property2");

            var innerPropertyEvaluator = innerPropertyResult.BindString();

            foreach (var item in propertyResult)
            {
                var innerResult = innerPropertyEvaluator.Evaluate(new ObjectDataContext(item));
                Assert.Equal(expectedResult, innerResult);
            }
        }

        [Theory]
        [MemberData("BinderFactoriesParameter")]
        public void DataBinder_NestedProperty(Func<Type, IDataBinder> dataBinderFactory)
        {
            const string expectedResult = "property";
            var obj = new { Property1 = new { Property2 = expectedResult } };

            var underTest = dataBinderFactory(obj.GetType());
            var result = underTest.Property("property1").Property("property2");

            Assert.NotNull(result);
            var evaluator = result.BindString();
            var propertyResult = evaluator.Evaluate(new ObjectDataContext(obj));

            Assert.Equal(expectedResult, propertyResult);
        }

        [Theory]
        [MemberData("BinderFactoriesParameter")]
        public void DataBinder_ConditionalProperty(Func<Type, IDataBinder> dataBinderFactory)
        {
            const bool expectedResult = true;
            var obj = new { Property1 = true };

            var underTest = dataBinderFactory(obj.GetType());
            var result = underTest.Property("property1");

            Assert.NotNull(result);
            var evaluator = result.BindBoolean();
            var propertyResult = evaluator.Evaluate(new ObjectDataContext(obj));

            Assert.Equal(expectedResult, propertyResult);
        }

        [Theory]
        [InlineData(typeof(List<string>), typeof(string))]
        [InlineData(typeof(IEnumerable<string>), typeof(string))]
        [InlineData(typeof(IDictionary<object, string>), typeof(KeyValuePair<object, string>))]
        public void TypeDataBinder_ItemFromGeneric(Type interfaceType, Type expectedItemType)
        {
            var underTest = TypeDataBinder.BinderFromType(interfaceType);
            var result = underTest.Item();

            Assert.NotNull(result);
            var binder = Assert.IsType<TypeDataBinder>(result);

            Assert.Equal(expectedItemType, binder.ResultType);
        }
    }
}
