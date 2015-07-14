using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Schema;
using Roslyn.Compilers.CSharp;
using TerrificNet.Test.Common;
using TerrificNet.ViewEngine;
using Xunit;

namespace TerrificNet.Generator.Test
{
	
	public class JsonSchemaCodeGeneratorTest
	{
		[Fact]
		public void TestClassNameNormalization()
		{
			var inputs = new List<Tuple<string, string>>
			{
				new Tuple<string, string>(null, null),
				new Tuple<string, string>("class", "Class"),
				new Tuple<string, string>("person info", "PersonInfo"),
				new Tuple<string, string>("person", "Person"),
				new Tuple<string, string>("Person", "Person"),
                new Tuple<string, string>("person_info", "PersonInfo"),
				new Tuple<string, string>("SimpleClass", "SimpleClass"),
			};

			foreach (var input in inputs)
			{
				var result = new NamingRule().GetPropertyName(input.Item1);
				Assert.Equal(input.Item2, result);
			}
		}

		[Fact]
		public void TestNoTitleSet()
		{
		    Assert.Throws<Exception>(() => GenerateCode("Mocks/Schemas/simpleObjectNoTitle.json"));
		}

		[Fact]
		public void TestSimpleObject()
		{
			const string reference = "namespace SimpleClass { public class SimpleClass{public string Name{get;set;}}}";
			Assert.True(CompareCode(reference, GenerateCode("Mocks/Schemas/simpleObject.json")));
		}

		[Fact]
		public void TestSimpleObjectAllType()
		{
            const string reference = "namespace SimpleClass { public class SimpleClass{public string Name{get;set;} public int Age{get;set;} public bool Male{get;set;}}}";
			Assert.True(CompareCode(reference, GenerateCode("Mocks/Schemas/simpleObjectAllType.json")));
		}

		[Fact]
		public void TestSimpleObjectComplex()
		{
            const string reference = "namespace SimpleClass {public class Person{ public string Name{get;set;}}public class SimpleClass{public Person Person{get;set;}}} ";
			Assert.True(CompareCode(reference, GenerateCode("Mocks/Schemas/simpleObjectComplex.json")));
		}

		[Fact]
		public void TestListSimple()
		{
            const string reference = "namespace Person {public class Person{ public System.Collections.Generic.IList<string> Names{get;set;}}}";
			Assert.True(CompareCode(reference, GenerateCode("Mocks/Schemas/listSimple.json")));
		}

        [Fact]
        public void TestListSimpleWithComplexTitle()
        {
			const string reference = "namespace TestOuter {public class Person{ public System.Collections.Generic.IList<string> Names{get;set;}}}";

            var schema = GetSchema(PathUtility.GetFullFilename("Mocks/Schemas/listSimple.json"));
			schema.Title = "TestOuter/Person";
            Assert.True(CompareCode(reference, GenerateCode(schema)));
        }

		[Fact]
		public void TestListSimpleWithComplexTitleCasesinsitive()
		{
			const string reference = "namespace TestOuter {public class Person{ public System.Collections.Generic.IList<string> Names{get;set;}}}";

            var schema = GetSchema(PathUtility.GetFullFilename("Mocks/Schemas/listSimple.json"));
			schema.Title = "testOuter/person";
			Assert.True(CompareCode(reference, GenerateCode(schema)));
		}

		[Fact]
		public void TestListComplex()
		{
            const string reference = "namespace Person {public class Name{public string Firstname{get;set;}} public class Person{ public System.Collections.Generic.IList<Name> Names{get;set;}}}";
			Assert.True(CompareCode(reference, GenerateCode("Mocks/Schemas/listComplex.json")));
		}

        [Fact]
	    public void TestCompileComplexType()
        {
            var path = "Mocks/Schemas/listComplex.json";
            var generator = new JsonSchemaCodeGenerator();
            var schema = GetSchema(path);

            var type = generator.Compile(schema);
            Assert.NotNull(type);
        }

		private static bool CompareCode(string original, string generated)
		{
			var originalTree = SyntaxTree.ParseText(original);
			var originalSyntax = originalTree.GetRoot().NormalizeWhitespace();
			var generatedTree = SyntaxTree.ParseText(generated);
			var generatedSyntax = generatedTree.GetRoot().NormalizeWhitespace();

			Console.WriteLine("Original");
			Console.WriteLine("--------");
			Console.WriteLine(originalSyntax.ToFullString());
			Console.WriteLine();

			Console.WriteLine("Generated");
			Console.WriteLine("---------");
			Console.WriteLine(generatedSyntax.ToFullString());

			return string.Equals(originalSyntax.ToFullString(), generatedSyntax.ToFullString());
		}

		private string GenerateCode(string path)
		{
            var schema = GetSchema(PathUtility.GetFullFilename(path));
		    return GenerateCode(schema);
		}

        private static string GenerateCode(JSchema schema)
	    {
	        var generator = new JsonSchemaCodeGenerator();
	        return generator.Generate(schema);
	    }

        private static JSchema GetSchema(string path)
		{
			string content;
			using (var reader = new StreamReader(path))
			{
				content = reader.ReadToEnd();
			}

            return JsonConvert.DeserializeObject<JSchema>(content);
		}
	}
}
