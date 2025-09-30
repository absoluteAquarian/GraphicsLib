using Microsoft.Xna.Framework;
using System.Diagnostics;

namespace GraphicsLib.ColorMath;

/// <summary>
/// Represents a color in the HSV (Hue, Saturation, Value) color space.
/// </summary>
[DebuggerDisplay("{DebuggerDisplayString,nq}")]
public partial struct HSVA {
	private readonly string DebuggerDisplayString => $"H = {H:0.##}°, S = {S*100:0.##}%, V = {V*100:0.##}%, A = {A*100:0.##}%";

	private float _hue;
	private float _saturation;
	private float _value;
	private float _alpha;

	/// <summary>
	/// The hue component, in degrees.  Always in the range [0, 360) with 360 wrapping to 0.
	/// </summary>
	public float H {
		readonly get => _hue;
		set {
			value %= 360;
			if (value < 0)
				value += 360;
			_hue = value;
		}
	}

	/// <summary>
	/// The saturation component.  Always in the range [0, 1].
	/// </summary>
	public float S {
		readonly get => _saturation;
		set => _saturation = float.Clamp(value, 0, 1);
	}

	/// <summary>
	/// The value component.  Always in the range [0, 1].
	/// </summary>
	public float V {
		readonly get => _value;
		set => _value = float.Clamp(value, 0, 1);
	}

	/// <summary>
	/// The alpha (opacity) component.  Always in the range [0, 1].
	/// </summary>
	public float A {
		readonly get => _alpha;
		set => _alpha = float.Clamp(value, 0, 1);
	}

	/// <summary>
	/// Creates a new instance of the <see cref="HSVA"/> struct with the specified hue, saturation, and value, and an alpha of 1 (fully opaque).
	/// </summary>
	/// <param name="h">The hue component.  Clamped to the range [0, 360).</param>
	/// <param name="s">The saturation component.  Clamped to the range [0, 1].</param>
	/// <param name="v">The value component.  Clamped to the range [0, 1].</param>
	public HSVA(float h, float s, float v) : this(h, s, v, 1) { }

	/// <summary>
	/// Creates a new instance of the <see cref="HSVA"/> struct with the specified hue, saturation, and value.
	/// </summary>
	/// <param name="h">The hue component.  Clamped to the range [0, 360).</param>
	/// <param name="s">The saturation component.  Clamped to the range [0, 1].</param>
	/// <param name="v">The value component.  Clamped to the range [0, 1].</param>
	/// <param name="a">The alpha (opacity) component.  Clamped to the range [0, 1].</param>
	public HSVA(float h, float s, float v, float a) {
		_hue = _saturation = _value = _alpha = 0;
		H = h;
		S = s;
		V = v;
		A = a;
	}

	/// <summary>
	/// Copies the values from another <see cref="HSVA"/> instance.
	/// </summary>
	/// <param name="other">The other <see cref="HSVA"/> instance to copy values from.</param>
	public HSVA(HSVA other) {
		_hue = other._hue;
		_saturation = other._saturation;
		_value = other._value;
		_alpha = other._alpha;
	}

	/// <summary>
	/// Returns a string representation of the HSV color.
	/// </summary>
	public override readonly string ToString() => $"{{{DebuggerDisplayString}}}";

	/// <summary>
	/// Linearly interpolates between two HSV colors.
	/// </summary>
	/// <param name="from">The starting color.</param>
	/// <param name="to">The ending color.</param>
	/// <param name="amount">The interpolation factor, typically in the range [0, 1].</param>
	public static HSVA Lerp(HSVA from, HSVA to, float amount) {
		// Interpolate through the shortest Hue direction
		// This results in correct lerping when Hue would wrap around

		float fromH = from.H, toH = to.H;
		if (float.Abs(toH - fromH) > 180) {
			if (toH > fromH)
				fromH += 360;
			else
				toH += 360;
		}

		return new HSVA(
			float.Lerp(fromH, toH, amount),
			float.Lerp(from.S, to.S, amount),
			float.Lerp(from.V, to.V, amount),
			float.Lerp(from.A, to.A, amount)
		);
	}

	/// <summary>
	/// Converts an <see cref="HSVA"/> color to its equivalent <see cref="Color"/> representation.
	/// </summary>
	/// <param name="hsva">The HSV color to convert.</param>
	/// <returns>The equivalent <see cref="Color"/> representation.</returns>
	public static Color ToColor(HSVA hsva) {
		// From: https://en.wikipedia.org/wiki/HSL_and_HSV#HSV_to_RGB_alternative
		
		static float f(float h, float s, float v, int n) {
			float k = mod(n + h / 60f, 6);
			return v - v * s * float.Max(0, float.Min(float.Min(k, 4 - k), 1));
		}

		static float mod(float v, float m) {
			return (v %= m) < 0 ? (v += m) : v;
		}

		float h = hsva.H, s = hsva.S, v = hsva.V;
		float r = f(h, s, v, 5);
		float g = f(h, s, v, 3);
		float b = f(h, s, v, 1);

		return new Color(r, g, b, hsva.A);
	}
}

/// <summary>
/// Provides extension methods for converting between <see cref="HSVA"/> and other color representations.
/// </summary>
public static class HSVExtensions {
	/// <summary>
	/// Converts a <see cref="Color"/> to its equivalent <see cref="HSVA"/> representation.
	/// </summary>
	/// <param name="this">The color to convert.</param>
	/// <returns>The equivalent <see cref="HSVA"/> representation.</returns>
	public static HSVA ToHSV(this Color @this) {
		// From: https://en.wikipedia.org/wiki/HSL_and_HSV#From_RGB
		float r = @this.R / 255f;
		float g = @this.G / 255f;
		float b = @this.B / 255f;

		float max = float.Max(float.Max(r, g), b);
		float min = float.Min(float.Min(r, g), b);
		float chroma = max - min;

		float h;
		if (chroma > 0) {
			if (max == r)
				h = 60 * ((g - b) / chroma % 6);
			else if (max == g)
				h = 60 * ((b - r) / chroma + 2);
			else // max == b
				h = 60 * ((r - g) / chroma + 4);
		} else
			h = 0;

		float s;
		if (max > 0)
			s = chroma / max;
		else
			s = 0;

		float v = max;

		return new HSVA(h, s, v, @this.A / 255f);
	}
}
