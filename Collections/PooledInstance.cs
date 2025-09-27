using System;

namespace GraphicsLib.Collections;

/// <summary>
/// A structure representing an object in a <see cref="InstancePool{T}"/>
/// </summary>
public readonly struct PooledInstance<T> : IEquatable<PooledInstance<T>> {
	/// <summary>
	/// The index of the source pool in <see cref="InstancePool{T}"/>
	/// </summary>
	public readonly int poolIndex = -1;

	/// <summary>
	/// The index of this instance in its pool
	/// </summary>
	public readonly int index = -1;

	private readonly bool properlyInitialized = false;

	public bool IsInvalid => !properlyInitialized || poolIndex < 0 || index < 0;

	public PooledInstance() { }

	internal PooledInstance(int poolIndex, int index) {
		this.poolIndex = poolIndex;
		this.index = index;
		properlyInitialized = true;
	}

	public ref T Data {
		get {
			if (IsInvalid)
				throw new InvalidOperationException("Instance was invalid");

			return ref InstancePool<T>.Get(poolIndex, index);
		}
	}

	public InstancePool<T> Pool => !IsInvalid ? InstancePool<T>.GetPool(poolIndex) : throw new InvalidOperationException("Instance was invalid");

	public override bool Equals(object obj) => obj is PooledInstance<T> instance && Equals(instance);

	public bool Equals(PooledInstance<T> other) => poolIndex == other.poolIndex && index == other.index && properlyInitialized == other.properlyInitialized;

	public override int GetHashCode() => HashCode.Combine(poolIndex, index);

	public static bool operator ==(PooledInstance<T> left, PooledInstance<T> right) => left.Equals(right);

	public static bool operator !=(PooledInstance<T> left, PooledInstance<T> right) => !(left == right);
}
