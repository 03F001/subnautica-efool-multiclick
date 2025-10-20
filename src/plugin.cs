using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;

using HarmonyLib;

using UnityEngine;

using Nautilus.Handlers;

namespace org.efool.subnautica.multiclick {

[BepInEx.BepInPlugin(
	org.efool.subnautica.multiclick.Info.FQN,
	org.efool.subnautica.multiclick.Info.title,
	org.efool.subnautica.multiclick.Info.version)]
[BepInEx.BepInDependency("com.snmodding.nautilus")]
public class Plugin : BepInEx.BaseUnityPlugin
{
	public static BepInEx.Logging.ManualLogSource log;
	public static ConfigGlobal config { get; private set;}

	private void Awake()
	{
		log = Logger;
		config = Nautilus.Handlers.OptionsPanelHandler.RegisterModOptions<ConfigGlobal>();
		Nautilus.Handlers.ConsoleCommandsHandler.RegisterConsoleCommands(typeof(Commands));
		new HarmonyLib.Harmony(org.efool.subnautica.multiclick.Info.FQN).PatchAll();
	}
}

[Nautilus.Options.Attributes.Menu(Info.title)]
public class ConfigGlobal : Nautilus.Json.ConfigFile
{
	[Nautilus.Options.Attributes.Toggle("Use single-click", Tooltip="Single-click by default like the original game, using alternate key for multi-click")]
	public bool useSingleClick = false;

	[Nautilus.Options.Attributes.Slider("Click interval", 0.0f, 0.2f, Step = 0.005f, DefaultValue = 0.05f, Format = "{0:F2}", Tooltip="Minimal seconds between multi-clicks")]
	public float timeClickInterval = 0.05f;
}

public static class Commands
{
	[Nautilus.Commands.ConsoleCommand("multiclick_interval")] public static void set_multiclick_interval(float x) { Plugin.config.timeClickInterval = x; }
}

[HarmonyPatch(typeof(GUIHand), "OnUpdate")]
public static class Patch
	{
		public static GameInput.Button inputAltClick = Nautilus.Handlers.EnumHandler.AddEntry<GameInput.Button>("Alternate click")
			.CreateInput()
			.WithCategory(Info.title)
			.WithKeyboardBinding("<Keyboard>/leftAlt")
			//.WithControllerBinding("<Gamepad>/leftTrigger") // there's no good button for this. Let users manually bind for gamepad
			.AvoidConflicts()
			.SetBindable();

	public static float _timeNextClick = Time.time;

	public static bool canClick(bool down, bool held)
	{
		var now = Time.time;
		var nextClickAvailable = now >= _timeNextClick;
		var singleClick = Plugin.config.useSingleClick ^ GameInput.GetButtonHeld(inputAltClick);

		var ret = (singleClick & down) | (!singleClick & held & nextClickAvailable);
		if ( ret )
			_timeNextClick = now + Plugin.config.timeClickInterval;

		return ret;
	}

	public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
	{
		var methodSend = AccessTools.Method(typeof(GUIHand), "Send");
		var methodGetInput = AccessTools.Method(typeof(GUIHand), "GetInput");

		var codes = new List<CodeInstruction>(instructions);
		for ( int i = codes.Count - 1; i >= 0; --i ) {
			if ( codes[i].Calls(methodSend) ) {
				for ( --i; i >= 0; --i ) {
					if ( codes[i].Calls(methodGetInput) ) {
						++i;

						if (
							i < 4
							|| !codes[i-4].IsLdarg()
							|| !codes[i-3].IsLdloc()
							|| !codes[i-2].LoadsConstant()
							|| !codes[i-1].Calls(methodGetInput)
							|| !codes[i].Branches(out Label? lbl)
						) {
							Plugin.log.LogError($"Failed to install efool-multiclick");
							return codes.AsEnumerable();
						}

						/* Keep existing this.GetInput(button1, GUIHand.InputState.down)
						 * Add this.GetInput(button1, GUIHand.InputState.held)
						 * => 2 bools on stack
						 * Call canClick
						 * => bool result is good for following branch for original this.GetInput(button1, GUIHand.InputState.down) result
						 * => original this.GetInput(button1, GUIHand.InputState.down)
						 *    replaced by canCheck(this.GetInput(button1, GUIHand.InputState.down), this.GetInput(button1, GUIHand.InputState.held))
						 */
						var instrs = new List<CodeInstruction>() {
							new CodeInstruction(OpCodes.Ldarg_0),              // this
							new CodeInstruction(OpCodes.Ldloc_0),              // button1
							new CodeInstruction(OpCodes.Ldc_I4_2),             // 2
							new CodeInstruction(OpCodes.Call, methodGetInput), // this.GetInput(button1, 2)

							new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Patch), "canClick"))
						};
						codes.InsertRange(i, instrs);
						return codes.AsEnumerable();
					}
				}
			}
		}

		return codes.AsEnumerable();
	}
}

} // org.efool.subnautica.multiclick