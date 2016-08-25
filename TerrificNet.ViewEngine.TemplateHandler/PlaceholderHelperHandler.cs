using System;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json.Linq;

namespace TerrificNet.ViewEngine.TemplateHandler
{
	internal class PlaceholderHelperHandler : IHelperHandler
	{
		private readonly ITerrificTemplateHandler _handler;

		public PlaceholderHelperHandler(ITerrificTemplateHandler handler)
		{
			_handler = handler;
		}

		public bool IsSupported(string name)
		{
			return name.StartsWith("placeholder", StringComparison.OrdinalIgnoreCase);
		}

		public void Evaluate(object model, RenderingContext context, IDictionary<string, string> parameters)
		{
			var key = parameters["key"].Trim('"');
			var index = TryGetIndex(parameters, model);
			_handler.RenderPlaceholder(model, key, index, context);
		}

		private static string TryGetIndex(IDictionary<string, string> parameters, object model)
		{
			string indexProperty;
			if (!parameters.TryGetValue("index", out indexProperty))
				return null;

			indexProperty = indexProperty.Trim('"');

			string index = null;
			string indexLocal;
			if (TryGetPropValue(model, indexProperty, out indexLocal))
				index = indexLocal;

			return index;
		}

		private static bool TryGetPropValue<TValue>(object src, string propertyName, out TValue value)
		{
			value = default(TValue);

			//JObject
			var jObject = src as JObject;
			JToken jValue;
			if (jObject != null && jObject.TryGetValue(propertyName, StringComparison.OrdinalIgnoreCase, out jValue))
			{
				value = jValue.Value<TValue>();
				return true;
			}

			//Dictionary
			var dictionaryObject = src as IDictionary<string, object>;
			object dictionaryValue;
			if (dictionaryObject != null && dictionaryObject.TryGetValue(propertyName, out dictionaryValue) && dictionaryValue is TValue)
			{
				value = (TValue)dictionaryValue;
				return true;
			}

			var property = src.GetType().GetTypeInfo().GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
			if (property != null)
			{
				value = (TValue)property.GetValue(src, null);

				return true;
			}

			return false;
		}
	}
}