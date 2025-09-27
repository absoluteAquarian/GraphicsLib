using System.Collections.Generic;
using System;

namespace GraphicsLib.Collections;

// Code adapted from: https://stackoverflow.com/questions/41946007/efficient-and-well-explained-implementation-of-a-quadtree-for-2d-collision-det

public class FreeList<T> {
	private struct FreeElement {
		public T element;
		public int next;
	}

	private FreeElement[] data;

	private int firstFreeIndex;

	public FreeList() {
		data = Array.Empty<FreeElement>();
		firstFreeIndex = -1;
	}

	public int Count { get; private set; }

	public ref T this[int index] => ref data[index].element;

	public ref T this[uint index] => ref data[index].element;

	public int Insert(in T element) {
		// There's a free index.  Use it
		if (firstFreeIndex != -1) {
			int index = firstFreeIndex;

			firstFreeIndex = data[firstFreeIndex].next;
			data[index].element = element;

			return index;
		}

		FreeElement fe = new() {
			element = element
		};

		// Resize the array, since more indices are needed
		Array.Resize(ref data, Math.Max(1, data.Length * 2));

		// Set the entry and "firstFreeIndex"
		data[Count] = fe;
		firstFreeIndex = ++Count < data.Length ? Count : -1;

		if (firstFreeIndex >= 0) {
			// Update the subsequent indices to have the proper "next" values
			for (int i = firstFreeIndex; i < data.Length; i++)
				data[i].next = i + 1;

			// Ensure that the last entry has a -1 set as "next"
			data[^1].next = -1;
		}

		return Count - 1;
	}

	public void Remove(int index) {
		data[index].next = firstFreeIndex;
		firstFreeIndex = index;
	}

	public void Clear() {
		data = Array.Empty<FreeElement>();
		firstFreeIndex = -1;
	}

	public IEnumerable<T> Enumerate(SparseSet knownUsedIndices) {
		foreach (int i in knownUsedIndices.EnumerateInOrder(ascending: true))
			yield return data[i].element;
	}
}
