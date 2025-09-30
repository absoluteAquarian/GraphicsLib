using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace GraphicsLib.Utility;

public static class ExceptionHelper {
	[DoesNotReturn]
	[StackTraceHidden]
	public static void ThrowIndexOutOfRange(int index, int minInclusive, int maxExclusive) {
		throw new IndexOutOfRangeException(index < minInclusive
			? $"Index was out of range ({index} < {minInclusive})"
			: $"Index was out of range ({index} >= {maxExclusive})");
	}

	[DoesNotReturn]
	[StackTraceHidden]
	public static void ThrowSequenceStartNegative(int start) {
		throw new ArgumentOutOfRangeException(nameof(start), $"Sequence start was negative ({start})");
	}

	[DoesNotReturn]
	[StackTraceHidden]
	public static void ThrowSequenceStartOutOfRange(int start, int length) {
		throw new ArgumentOutOfRangeException(nameof(start), start < 0
			? $"Sequence start was negative ({start})"
			: $"Sequence start was out of range ({start} >= {length})");
	}

	[DoesNotReturn]
	[StackTraceHidden]
	public static void ThrowSequenceCountNegative(int count) {
		throw new ArgumentOutOfRangeException(nameof(count), $"Sequence count was negative ({count})");
	}

	[DoesNotReturn]
	[StackTraceHidden]
	public static void ThrowSequenceExceedsLength(int start, int sequenceLength, int totalLength) {
		throw new ArgumentOutOfRangeException(nameof(start), $"Sequence (start: {start}, length: {sequenceLength}) exceeds total length ({totalLength})");
	}
}
