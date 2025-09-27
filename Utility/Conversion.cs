using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace GraphicsLib.Utility;

public static class Conversion {
	public static IEnumerable<T> ToEnumerable<T>(this IEnumerator<T> enumerator, bool resetOnCompletion = false) {
		ArgumentNullException.ThrowIfNull(enumerator);

		while (enumerator.MoveNext())
			yield return enumerator.Current;

		if (resetOnCompletion)
			enumerator.Reset();
	}

	public static ref readonly TTo UnsafeCast<TFrom, TTo>(scoped ref readonly TFrom from)
		=> ref Unsafe.As<TFrom, TTo>(ref Unsafe.AsRef(in from));

	public static ref TTo UnsafeCastRef<TFrom, TTo>(ref TFrom from)
		=> ref Unsafe.As<TFrom, TTo>(ref from);
}
