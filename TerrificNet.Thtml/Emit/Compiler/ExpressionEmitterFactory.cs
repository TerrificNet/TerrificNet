using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace TerrificNet.Thtml.Emit.Compiler
{
	internal class ExpressionEmitterFactory : IEmitterFactory<Expression, Expression, Expression>
	{
		public Expression Iterator(Func<object, IEnumerable> list, Expression blockEmitter)
		{
			throw new NotImplementedException();
		}

		public Expression Many(IEnumerable<Expression> emitters)
		{
			var expressions = emitters as IList<Expression> ?? emitters.ToList();
			if (expressions.Any())
				return Expression.Block(expressions);

			return Expression.Empty();
		}

		public Expression Condition(Func<object, bool> predicate, Expression blockEmitter)
		{
			return Expression.IfThen(Expression.Constant(true), blockEmitter);
		}

		public Expression AsList(Expression emitter)
		{
			return emitter;
		}

		public Expression AsList(IEnumerable<Expression> emitter)
		{
			var expressions = emitter as IList<Expression> ?? emitter.ToList();
			if(expressions.Any())
				return Expression.Block(expressions);

			return Expression.Empty();
		}

		public Expression Lambda(Expression func)
		{
			return func;
		}
	}
}