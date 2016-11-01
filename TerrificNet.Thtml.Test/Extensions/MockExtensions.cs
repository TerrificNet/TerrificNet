using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Moq;
using Moq.Language;

namespace TerrificNet.Thtml.Test.Extensions
{
	internal static class MockExtensions
	{
		public static Mock<T> InSequence<T>(this Mock<T> mock, params Expression<Action<T>>[] expressions) where T : class
		{
			return InSequence(mock, (IEnumerable<Expression<Action<T>>>) expressions);
		}

		public static Mock<T> InSequence<T>(this Mock<T> mock, IEnumerable<Expression<Action<T>>> expressions) where T : class
		{
			var sequence = new MockSequence();
			foreach (var expression in expressions)
			{
				mock.InSequence(sequence).Setup(expression);
			}

			return mock;
		}

		public static Mock<TMock> InSequence<TMock>(this Mock<TMock> mock, params Action<ISetupConditionResult<TMock>>[] expressions)
			where TMock : class
		{
			var sequence = new MockSequence();
			foreach (var expression in expressions)
			{
				expression(mock.InSequence(sequence));
			}

			return mock;
		}
	}
}