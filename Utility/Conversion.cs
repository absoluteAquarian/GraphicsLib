using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace GraphicsLib.Utility;

/// <summary>
/// Provides methods for converting between different types and representations.
/// </summary>
public static class Conversion {
	/// <summary>
	/// Converts an <see cref="IEnumerator{T}"/> to an <see cref="IEnumerable{T}"/>, optionally resetting the enumerator upon completion.
	/// </summary>
	/// <typeparam name="T">The type of elements in the enumerator.</typeparam>
	/// <param name="enumerator">The enumerator to convert.</param>
	/// <param name="resetOnCompletion">If <see langword="true"/>, the enumerator will be reset after enumeration completes.</param>
	/// <returns>An <see cref="IEnumerable{T}"/> that iterates through the elements of the enumerator.</returns>
	public static IEnumerable<T> ToEnumerable<T>(this IEnumerator<T> enumerator, bool resetOnCompletion = false) {
		ArgumentNullException.ThrowIfNull(enumerator);

		while (enumerator.MoveNext())
			yield return enumerator.Current;

		if (resetOnCompletion)
			enumerator.Reset();
	}

	/// <summary>
	/// Performs an unsafe cast from one type to another, returning a reference to the target type.
	/// </summary>
	/// <typeparam name="TFrom">The source type.</typeparam>
	/// <typeparam name="TTo">The target type.</typeparam>
	/// <param name="from">A reference to the source value.</param>
	/// <returns>A reference to the target type.</returns>
	public static ref readonly TTo UnsafeCast<TFrom, TTo>(scoped ref readonly TFrom from)
		=> ref Unsafe.As<TFrom, TTo>(ref Unsafe.AsRef(in from));

	/// <inheritdoc cref="UnsafeCast"/>
	public static ref TTo UnsafeCastRef<TFrom, TTo>(ref TFrom from)
		=> ref Unsafe.As<TFrom, TTo>(ref from);
}
