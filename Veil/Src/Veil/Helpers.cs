﻿using System;
using System.Collections.Concurrent;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Veil.Compiler;
using Veil.Parser;
using Veil.Parser.Nodes;

namespace Veil
{
	public static class Helpers
	{
		public static Task HtmlEncodeAsync(Task before, TextWriter writer, object value)
		{
			if (value != null)
				return HtmlEncodeAsync(before, writer, value.ToString());

			return HtmlEncodeAsync(before, writer, string.Empty);
		}

		public static void HtmlEncode(TextWriter writer, object value)
		{
			if (value != null)
			{
				HtmlEncode(writer, value.ToString());
				return;
			}

			HtmlEncode(writer, string.Empty);
		}

		public static void Write(TextWriter writer, string value)
		{
			writer.Write(value);
			//await before.ConfigureAwait(false);
			//await writer.WriteAsync(value).ConfigureAwait(false);
			//return before.ContinueWith(t => writer.WriteAsync(value), TaskContinuationOptions.ExecuteSynchronously | TaskContinuationOptions.OnlyOnRanToCompletion).Unwrap();
		}

		public static Task WriteAsync(Task before, TextWriter writer, string value)
		{
			return Then(before, () => writer.Write(value));
			//await before.ConfigureAwait(false);
			//await writer.WriteAsync(value).ConfigureAwait(false);
			//return before.ContinueWith(t => writer.WriteAsync(value), TaskContinuationOptions.ExecuteSynchronously | TaskContinuationOptions.OnlyOnRanToCompletion).Unwrap();
		}

		public static void HtmlEncode(TextWriter writer, string value)
		{
			Escape(writer, value);
		}

		public static Task HtmlEncodeAsync(Task before, TextWriter writer, string value)
		{
			return Then(before, () => Escape(writer, value));
		}

		private static void Escape(TextWriter writer, string value)
		{
			if (string.IsNullOrEmpty(value))
				return;

			var startIndex = 0;
			var currentIndex = 0;
			var valueLength = value.Length;
			char currentChar;

			if (valueLength < 50)
			{
				// For short string, we can just pump each char directly to the writer
				for (; currentIndex < valueLength; ++currentIndex)
				{
					currentChar = value[currentIndex];
					switch (currentChar)
					{
						case '&':
							writer.Write("&amp;");
							break;
						case '<':
							writer.Write("&lt;");
							break;
						case '>':
							writer.Write("&gt;");
							break;
						case '"':
							writer.Write("&quot;");
							break;
						case '\'':
							writer.Write("&#39;");
							break;
						default:
							writer.Write(currentChar);
							break;
					}
				}
			}
			else
			{
				// For longer strings, the number of Write calls becomes prohibitive, so sacrifice a call to ToCharArray to allos us to buffer the Write calls
				char[] chars = null;
				for (; currentIndex < valueLength; ++currentIndex)
				{
					currentChar = value[currentIndex];
					switch (currentChar)
					{
						case '&':
						case '<':
						case '>':
						case '"':
						case '\'':
							if (chars == null) chars = value.ToCharArray();
							if (currentIndex != startIndex) writer.Write(chars, startIndex, currentIndex - startIndex);
							startIndex = currentIndex + 1;

							switch (currentChar)
							{
								case '&':
									writer.Write("&amp;");
									break;
								case '<':
									writer.Write("&lt;");
									break;
								case '>':
									writer.Write("&gt;");
									break;
								case '"':
									writer.Write("&quot;");
									break;
								case '\'':
									writer.Write("&#39;");
									break;
							}
							break;
					}
				}

				if (startIndex == 0)
				{
					writer.Write(value);
					return;
				}

				if (currentIndex != startIndex)
					writer.Write(chars, startIndex, currentIndex - startIndex);
			}
		}

		public static Task HtmlEncodeLateBoundAsync(Task before, TextWriter writer, object value)
		{
			if (value is string)
			{
				return HtmlEncodeAsync(before, writer, (string)value);
			}

			if (value != null)
				return Unwrap(before, writer, value);

			return before;
		}

		private static Task Unwrap(Task before, TextWriter writer, object value)
		{
			return Then(before, () => writer.Write(value.ToString()));
			//await before.ConfigureAwait(false);
			//await writer.WriteAsync(value.ToString());
		}

		public static bool Boolify(object o)
		{
			if (o is bool) return (bool)o;
			return o != null;
		}

		public static void CheckNotNull(string message, object obj, SyntaxTreeNode node)
		{
			if (obj == null)
				throw new VeilCompilerException(message, node);
		}

		private static ConcurrentDictionary<Tuple<Type, string>, Func<object, object>> lateBoundCache = new ConcurrentDictionary<Tuple<Type, string>, Func<object, object>>();

		public static object RuntimeBind(object model, LateBoundExpressionNode node)
		{
			var itemName = node.ItemName;
			var memberLocator = node.MemberLocator;

			CheckNotNull($"Could not bind expression with name '{node.ItemName}'. The value is null.",
				model, node);

			var runtimeModel = model as IRuntimeModel;
			if (runtimeModel?.Data != null)
				model = runtimeModel.Data;

			var binder = GetBinder(model, itemName, memberLocator);

			if (binder == null)
				throw new VeilCompilerException("Unable to late-bind '{0}' against model {1}".FormatInvariant(itemName, model.GetType().Name), node);

			var result = binder(model);
			return result;
		}

	    public static Func<object, object> GetBinder(object model, string itemName)
	    {
	        return GetBinder(model, itemName, MemberLocator.Default);
	    }

	    private static Func<object, object> GetBinder(object model, string itemName, IMemberLocator memberLocator)
	    {
	        var binder = lateBoundCache.GetOrAdd(Tuple.Create(model.GetType(), itemName), pair =>
	        {
	            var type = pair.Item1;
	            var name = pair.Item2;

	            if (name.EndsWith("()"))
	            {
	                var function =
	                    memberLocator.FindMember(type, name.Substring(0, name.Length - 2), MemberTypes.Method) as MethodInfo;
	                if (function != null) return DelegateBuilder.FunctionCall(type, function);
	            }

	            var property = memberLocator.FindMember(type, name, MemberTypes.Property) as PropertyInfo;
	            if (property != null) return DelegateBuilder.Property(type, property);

	            var field = memberLocator.FindMember(type, name, MemberTypes.Field) as FieldInfo;
	            if (field != null) return DelegateBuilder.Field(type, field);

	            var dictionaryType = type.GetDictionaryTypeWithKey();
	            if (dictionaryType != null) return DelegateBuilder.Dictionary(dictionaryType, name);

	            return null;
	        });
	        return binder;
	    }

	    private static BindingFlags GetBindingFlags(bool isCaseSensitive)
		{
			var flags = BindingFlags.Public | BindingFlags.Instance;
			if (!isCaseSensitive)
			{
				flags = flags | BindingFlags.IgnoreCase;
			}
			return flags;
		}

		public static Task Then(Task first, Action next)
		{
			return first.ContinueWith(t => next(), TaskContinuationOptions.ExecuteSynchronously);
		}

		public static Task Then(Task first, Func<Task> next)
		{
			//return first.ContinueWith(t => next()).Unwrap();
			var tcs = new TaskCompletionSource<bool>();
			first.ContinueWith(t1 =>
			{
				if (t1.IsFaulted)
					tcs.TrySetException(t1.Exception.InnerExceptions);

				else if (t1.IsCanceled)
					tcs.TrySetCanceled();

				else
				{
					try
					{
						var t = next();
						if (t == null)
							tcs.TrySetCanceled();

						else t.ContinueWith(t2 =>
						{
							if (t2.IsFaulted)
								tcs.TrySetException(t2.Exception.InnerExceptions);

							else if (t2.IsCanceled) tcs.TrySetCanceled();
							else tcs.TrySetResult(true);
						}, TaskContinuationOptions.ExecuteSynchronously);
					}
					catch (Exception exc) { tcs.TrySetException(exc); }
				}
			}, TaskContinuationOptions.ExecuteSynchronously);

			return tcs.Task;
		}
	}

	public interface IRuntimeModel
	{
		object Data { get; }
	}
}