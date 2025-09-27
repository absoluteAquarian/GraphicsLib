using GraphicsLib.Drawing;
using SerousCommonLib.API.ModCall;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace GraphicsLib;

public class GraphlicsLibMod : Mod {
	// TODO: people would like an easy Zenith trail implementation... find it in the 1.4 source and implement it in GraphicsLib

	public override void Load() {
		if (Main.netMode == NetmodeID.Server)
			return;

		PrimitiveUploader.Load();
	}

	public override void Unload() {
		if (Main.netMode == NetmodeID.Server)
			return;

		PrimitiveUploader.Unload();
	}

	/// <inheritdoc/>
	public override object Call(params object[] args) => BaseCallFunction.Call(this, args);
}