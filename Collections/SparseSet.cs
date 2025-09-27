using GraphicsLib.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace GraphicsLib.Collections;

/// <summary>
/// Represents an unordered collection of natural numbers and provides O(1) operations on it
/// </summary>
public class SparseSet : IDisposable, IEnumerable<int> {
	private int[] dense;

	private bool disposed;

	private int[] sparse;

	/// <summary>
	/// Initializes a new instance of the <seealso cref="SparseSet" /> class
	/// </summary>
	/// <param name="max">The initial maximum value for the set</param>
	public SparseSet(int max) {
		if (max <= 0)
			throw new ArgumentException("Value was too small", nameof(max));

		if (max >= int.MaxValue / 128)
			throw new ArgumentException("Value was too large", nameof(max));

		Length = max;

		Count = 0;

		dense = new int[max];
		sparse = new int[max];
	}

	public SparseSet(IEnumerable<int> collection, int max) : this(max) {
		foreach (int i in collection)
			Add(i);
	}

	/// <summary>
	///	    Gets the maximum number of elements that could be contained within the set
	/// </summary>
	public int Length { get; private set; }

	public int this[int index] => index >= 0 && index < Count ? dense[index] : throw new ArgumentOutOfRangeException(nameof(index));

	/// <summary>
	/// Gets the number of elements in the set
	/// </summary>
	public int Count { get; private set; }

	public void Dispose() {
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	/// <summary>
	/// Returns an enumerator that iterates through all elements in the set
	/// </summary>
	/// <returns>An <see cref="IEnumerator{T}" /> object that can be used to iterate through the collection.</returns>
	public IEnumerator<int> GetEnumerator() {
		int i = 0;
		while (i < Count) {
			yield return dense[i];
			i++;
		}
	}

	/// <summary>
	/// Returns an enumerator that iterates through all elements in the set
	/// </summary>
	/// <returns>An <see cref="IEnumerator{T}" /> object that can be used to iterate through the collection.</returns>
	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

	public IEnumerable<int> EnumerateInOrder(bool ascending) {
		IEnumerable<int> enumeration = GetEnumerator().ToEnumerable();

		if (ascending)
			enumeration = enumeration.OrderBy(i => i);
		else
			enumeration = enumeration.OrderByDescending(i => i);

		return enumeration;
	}

	/// <summary>
	/// Attempts to find a sequence of length <paramref name="count"/> of integers within this set.<br/>
	/// If none are found, the set is resized to contain the sequence at the end.
	/// </summary>
	/// <param name="count">The length of the sequence of free integers to find</param>
	/// <param name="findEmpty">Whether the algorithm should check for free integers (<see langword="true"/>) or used integers (<see langword="false"/>).</param>
	/// <returns>The number corresponding to the first entry of the valid sequence</returns>
	public int FindSequence(int count, bool findEmpty) {
		if (count <= 0)
			throw new ArgumentOutOfRangeException(nameof(count), count, "Sequence count must be greater than zero");

		int empty = 0;
		int start = -1;
		int total = Length;

		for (int i = 0; i < total; i++) {
			if (Contains(i) != findEmpty) {
				if (empty == 0)
					start = i;

				empty++;

				if (empty == count)
					break;
			} else {
				start = -1;
				empty = 0;
			}
		}

		if (start == -1) {
			start = Length;
			EnsureCapacity(Length + count);
		}

		if (!findEmpty) {
			// Add entries so that they appear "filled" by the caller
			for (int i = 0; i < count; i++)
				Add(i + start);
		}

		return start;
	}

	private void EnsureCapacity(int maxNum) {
		if (maxNum < Length)
			return;

		Length = maxNum + 1;

		if (Length >= int.MaxValue / 128)
			throw new InvalidOperationException("New capacity was too large");

		Array.Resize(ref dense, Length);
		Array.Resize(ref sparse, Length);
	}

	/// <summary>
	/// Adds the given value.
	/// If the value already exists in the set, it will be ignored.
	/// </summary>
	/// <param name="value">The value</param>
	/// <returns>
	/// <see langword="true"></see> if the set does not contain the given value; otherwise,
	/// <see langword="false"></see>
	/// </returns>
	public bool Add(int value) {
		EnsureCapacity(value);

		if (value < Length && !Contains(value)) {
			dense[Count] = value;  // Insert the new value in the dense array
			sparse[value] = Count; // ... and link it to the sparse array
			Count++;

			return true;
		}

		return false;
	}

	/// <summary>
	///    Fills the set completely
	/// </summary>
	public void Fill() {
		for (int i = 0; i < Length; i++)
			Add(i);
	}

	/// <summary>
	/// Fills the set with values from 0 to <paramref name="maxExclusive"/>
	/// </summary>
	/// <param name="maxExclusive">The maximum value to iterate toward</param>
	public void Fill(int maxExclusive) {
		for (int i = 0; i < maxExclusive; i++)
			Add(i);
	}

	/// <summary>
	/// Fills the set with values from <paramref name="minInclusive"/> to <paramref name="maxExclusive"/>
	/// </summary>
	/// <param name="minInclusive">The minimum value to start the iteration at</param>
	/// <param name="maxExclusive">The maximum value to iterate toward</param>
	public void Fill(int minInclusive, int maxExclusive) {
		for (int i = minInclusive; i < maxExclusive; i++)
			Add(i);
	}

	/// <summary>
	/// Removes the given value if it exists
	/// </summary>
	/// <param name="value">The value</param>
	/// <returns>
	/// <see langword="true"></see> if the set contained the given value; otherwise, <see langword="false"></see>
	/// </returns>
	public bool Remove(int value) {
		if (Contains(value)) {
			dense[sparse[value]] = dense[Count - 1];  // Put the value at the end of the dense array into the slot of the removed value
			sparse[dense[Count - 1]] = sparse[value]; // Put the link to the removed value in the slot of the replaced value
			Count--;

			return true;
		}

		return false;
	}

	/// <summary>
	/// Determines whether the set contains the given value
	/// </summary>
	/// <param name="value">The value</param>
	/// <returns>
	/// <see langword="true"></see> if the set contains the given value; otherwise, <see langword="false"></see>
	/// </returns>
	public bool Contains(int value) {
		// Value must meet two conditions:
		//   1) Link value from sparse array must point to the current used range in the dense array
		//   2) There must be a value two-way link
		return value >= 0 && value < Length && sparse[value] < Count && dense[sparse[value]] == value;
	}

	/// <summary>
	/// Removes all elements from the set, but not from memory
	/// </summary>
	public void Clear() {
		// Simply set the "current size" to 0 to clear the set; no re-initialization is required
		Count = 0;
	}

	public List<int> ToList() => [.. this];

	public int[] ToArray() {
		int[] arr = new int[Count];
		int idx = 0;

		foreach (int item in this)
			arr[idx++] = item;

		return arr;
	}

	private void Dispose(bool disposing) {
		if (!disposed) {
			disposed = true;

			if (disposing) {
				// Not used
			}

			dense = null!;
			sparse = null!;
		}
	}

	~SparseSet() {
		Dispose(false);
	}
}
