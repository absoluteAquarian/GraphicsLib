using System.Linq;
using System;
using System.Runtime.CompilerServices;

namespace GraphicsLib.Collections;

public class InstancePool<T> : IDisposable {
	private static readonly FreeList<InstancePool<T>> pool = new();
	private static readonly SparseSet reservedIndices = new(8);

	public int Index { get; private set; } = -1;

	private T[] instancePool;
	public T[] Pool {
		get => instancePool;
		private set => instancePool = value;
	}

	private readonly SparseSet freeIndices = new(8);

	public int Count => freeIndices.Length - freeIndices.Count;

	private InstancePool() {
		freeIndices.Fill();
	}

	public PooledInstance<T> this[int index] => CreateOrGetInstance(index);

	public void Dispose() {
		if (Index < 0)
			return;

		foreach (var disposable in pool[Index].instancePool.OfType<IDisposable>())
			disposable.Dispose();

		pool[Index].instancePool = [];
		freeIndices.Fill();
		pool.Remove(Index);
		reservedIndices.Remove(Index);
		Index = -1;
	}

	/// <summary>
	/// Resizes <see cref="Pool"/> accordingly and returns a structure which points to an index within <see cref="Pool"/>
	/// </summary>
	public PooledInstance<T> CreateOrGetInstance(int index) {
		if (Pool.Length <= index) {
			int oldLength = Pool.Length;
			Array.Resize(ref instancePool, index + 1);
			freeIndices.Fill(oldLength, Pool.Length);
		}

		freeIndices.Remove(index);

		return new(Index, index);
	}

	/// <summary>
	/// Resizes <see cref="Pool"/> accordingly and returns a structure which points to the first "unused" index within <see cref="Pool"/>
	/// </summary>
	public PooledInstance<T> CreateNextInstance() {
		int index = freeIndices.EnumerateInOrder(ascending: true).FirstOrDefault(-1);

		if (index < 0)
			index = instancePool.Length;

		return CreateOrGetInstance(index);
	}

	/// <summary>
	/// Resizes <see cref="Pool"/> accordingly and returns a contiguous sequence of objects representing indices within the pool
	/// </summary>
	public PooledInstance<T>[] CreateNextSequence(int count) {
		int index = freeIndices.FindSequence(count, findEmpty: false);

		for (int i = 0; i < index; i++)
			freeIndices.Remove(i + index);

		PooledInstance<T>[] ret = new PooledInstance<T>[count];
		ref PooledInstance<T> retRef = ref ret[0];
		for (int i = 0; i < count; i++, retRef = ref Unsafe.Add(ref retRef, 1))
			retRef = new(Index, index + i);

		return ret;
	}

	/// <summary>
	/// Marks the index pointed to by <paramref name="instance"/> within <see cref="Pool"/> free to use by <see cref="CreateNextInstance"/> and (optionally) destroys the data at the index
	/// </summary>
	/// <exception cref="ArgumentException"/>
	public void FreeInstance(ref PooledInstance<T> instance, bool destroy = true) {
		if (instance.poolIndex != Index)
			throw new ArgumentException("Pooled instance did not refer to this pool");

		if (instance.index < 0)
			throw new ArgumentException("Pooled instance was invalid");

		freeIndices.Add(instance.index);
		
		if (destroy) {
			(instance.Data as IDisposable)?.Dispose();
			instance.Data = default!;
		}

		instance = default;
	}

	/// <summary>
	/// Swaps the data of two pooled indices
	/// </summary>
	public static void Swap(in PooledInstance<T> first, in PooledInstance<T> second) {
		(second.Data, first.Data) = (first.Data, second.Data);
	}

	/// <summary>
	/// Swaps the data of two ranges of pooled indices.  Both ranges must have the same length.
	/// </summary>
	/// <exception cref="ArgumentException"/>
	public static void Swap(in Span<PooledInstance<T>> first, in Span<PooledInstance<T>> second) {
		if (first.Length != second.Length)
			throw new ArgumentException("Arguments did not have the same length");

		if (first.Overlaps(second))
			throw new ArgumentException("Arguments did not refer to unique sequences (data overlap detected)");

		int length = first.Length;
		Span<PooledInstance<T>> temp = length < 1024 ? stackalloc PooledInstance<T>[length] : new PooledInstance<T>[length];

		first.CopyTo(temp);
		second.CopyTo(first);
		temp.CopyTo(second);
	}

	/// <summary>
	/// Copies the data from the <paramref name="copyFrom"/> instance to the <paramref name="copyTo"/> instance
	/// </summary>
	/// <param name="copyFrom">The copy source</param>
	/// <param name="copyTo">The copy target</param>
	public static void Copy(in PooledInstance<T> copyFrom, in PooledInstance<T> copyTo) {
		copyTo.Data = copyFrom.Data;
	}

	/// <summary>
	/// Copies the data from the <paramref name="copyFrom"/> range of instances to the <paramref name="copyTo"/> range of indices
	/// </summary>
	/// <param name="copyFrom">The copy source</param>
	/// <param name="copyTo">The copy target</param>
	/// <exception cref="ArgumentException"/>
	public static void Copy(in Span<PooledInstance<T>> copyFrom, in Span<PooledInstance<T>> copyTo) {
		if (copyFrom.Length != copyTo.Length)
			throw new ArgumentException("Arguments did not have the same length");

		if (copyFrom.Overlaps(copyTo))
			throw new ArgumentException("Arguments did not refer to unique sequences (data overlap detected)");

		ref PooledInstance<T> fromRef = ref copyFrom[0];
		ref PooledInstance<T> toRef = ref copyTo[0];

		for (int i = 0; i < copyFrom.Length; i++, fromRef = ref Unsafe.Add(ref fromRef, 1), toRef = ref Unsafe.Add(ref toRef, 1))
			toRef.Data = fromRef.Data;
	}

	public static ref T Get(int poolIndex, int instanceIndex) => ref pool[poolIndex].Pool[instanceIndex];

	public static InstancePool<T> GetPool(int index) => pool[index];

	public static InstancePool<T> Reserve(int capacity) {
		InstancePool<T> instance = new();
		int index = pool.Insert(instance);
		instance.Index = index;
		instance.Pool = new T[capacity];
		reservedIndices.Add(index);
		return instance;
	}

	public static void Clear() {
		var set = new SparseSet(reservedIndices, reservedIndices.Length);

		foreach (int index in set)
			pool[index].Dispose();

		reservedIndices.Clear();
	}
}
