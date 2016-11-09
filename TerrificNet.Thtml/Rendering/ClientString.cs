using System;

namespace TerrificNet.Thtml.Rendering
{
	public class ClientString : IEquatable<ClientString>
	{
		public ClientString(string content)
		{
			Content = content;
		}

		public string Content { get; }

		public bool Equals(ClientString other)
		{
			return other != null && other.Content == Content;
		}

		public override bool Equals(object obj)
		{
			var cs = obj as ClientString;
			return cs != null && Equals(cs);
		}

		public override int GetHashCode()
		{
			return Content?.GetHashCode() ?? 0;
		}
	}
}