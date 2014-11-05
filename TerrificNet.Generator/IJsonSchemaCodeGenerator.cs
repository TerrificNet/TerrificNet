﻿using Newtonsoft.Json.Schema;

namespace TerrificNet.Generator
{
	public interface IJsonSchemaCodeGenerator
	{
		string Generate(JsonSchema schema);
	}
}