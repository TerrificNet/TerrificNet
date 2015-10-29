using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

static internal class GenericAssert
{
	public static void AssertOneOf(params Func<bool>[] assertions)
	{
		foreach (var assert in assertions)
		{
			if (assert())
				return;
		}
		Assert.False(true, "No Type found");
	}

	public static void AssertCollection<T>(IReadOnlyList<T> expected, IReadOnlyList<T> actual, Action<T, T> assertion)
	{
		Assert.Equal(expected.Count, actual.Count);
		for (int i = 0; i < expected.Count; i++)
		{
			assertion(expected[0], actual[0]);
		}
	}

	public static bool AssertValue<T>(object expected, object result, Action<T, T> assertion)
		where T : class
	{
		var t = expected as T;
		if (t != null)
		{
			Assert.IsType<T>(result);
			assertion(t, result as T);

			return true;
		}
		return false;
	}
}