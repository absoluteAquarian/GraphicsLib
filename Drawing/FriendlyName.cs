namespace GraphicsLib.Drawing;

/// <summary>
/// A helper class for providing readable type names in exception messages.
/// </summary>
/// <typeparam name="T">The type to get the friendly name for.</typeparam>
public static class FriendlyName<T> {
	/// <summary>
	/// The friendly name of the type <typeparamref name="T"/>
	/// </summary>
	public static readonly string Value;

	static FriendlyName() {
		string name = typeof(T).Name;
		int index = name.IndexOf('`');
		Value = index >= 0 ? name[..index] : name;
	}
}
