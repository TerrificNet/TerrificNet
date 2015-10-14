using System;
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
            IEvaluater<string> evalutor;
            Assert.True(result.TryCreateEvaluation(out evalutor));
            var propertyResult = evalutor.Evaluate(new ObjectDataContext(obj));

            Assert.Equal(expectedResult, propertyResult);
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
            IEvaluater<string> evalutor;
            Assert.True(result.TryCreateEvaluation(out evalutor));
            var propertyResult = evalutor.Evaluate(new ObjectDataContext(obj));

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
