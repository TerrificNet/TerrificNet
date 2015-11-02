using System;
using System.Collections;
using System.Collections.Generic;

namespace TerrificNet.Thtml.Emit
{
	internal interface IEmitterFactory<TEmit, in TRunnable, TEmitterRunnable>
	{
		TEmit Iterator(Func<object, IEnumerable> list, TEmit blockEmitter);
		TEmit Many(IEnumerable<TEmit> emitters);
		TEmit Condition(Func<object, bool> predicate, TEmit blockEmitter);
		TEmit AsList(TEmitterRunnable emitter);
		TEmit AsList(IEnumerable<TEmitterRunnable> emitter);
		//IEmitterRunnable<TEmit> Lambda(Func<object, IRenderingContext, TEmit> func);
		TEmitterRunnable Lambda(TRunnable func);
	}
}