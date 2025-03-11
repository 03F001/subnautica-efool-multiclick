using UnityEngine;
using static System.Net.WebRequestMethods;

namespace org.efool.subnautica.multiclick {

[BepInEx.BepInPlugin(
	org.efool.subnautica.multiclick.Info.FQN,
	org.efool.subnautica.multiclick.Info.title,
	org.efool.subnautica.multiclick.Info.version)]
[BepInEx.BepInDependency("com.snmodding.nautilus")]
public class Plugin : BepInEx.BaseUnityPlugin
{
	public static ConfigGlobal config { get; private set;}

	private void Awake()
	{
		config = Nautilus.Handlers.OptionsPanelHandler.RegisterModOptions<ConfigGlobal>();
		new HarmonyLib.Harmony(org.efool.subnautica.multiclick.Info.FQN).PatchAll();
	}
}

[Nautilus.Options.Attributes.Menu(Info.title)]
public class ConfigGlobal : Nautilus.Json.ConfigFile
{
	[Nautilus.Options.Attributes.Toggle("Use single-click", Tooltip="Single-click by default like the original game, using alternate key for multi-click")]
	public bool useSingleClick = false;

	[Nautilus.Options.Attributes.Keybind("Use alternate click", Tooltip="Performs the opposite clicking behavior of 'Use single-click' option")]
	public KeyCode keyAltClick = KeyCode.LeftAlt;

	[Nautilus.Options.Attributes.Slider("Click interval", 0.0f, 0.2f, Step = 0.005f, DefaultValue = 0.05f, Format = "{0:F2}", Tooltip="Minimal seconds between multi-clicks")]
	public float timeClickInterval = 0.05f;
}

public static class Commands
{
	[Nautilus.Commands.ConsoleCommand("multiclick_interval")] public static void set_multiclick_interval(float x) { Plugin.config.timeClickInterval = x; }
}

[HarmonyLib.HarmonyPatch]
static class Patch
{
	private static readonly HarmonyLib.FastInvokeHandler invoke_IsPDAInUse         = HarmonyLib.MethodInvoker.GetHandler(HarmonyLib.AccessTools.Method(typeof(GUIHand), "IsPDAInUse"        ));
	private static readonly HarmonyLib.FastInvokeHandler invoke_UpdateActiveTarget = HarmonyLib.MethodInvoker.GetHandler(HarmonyLib.AccessTools.Method(typeof(GUIHand), "UpdateActiveTarget"));

	private static float _timeNextClick = Time.time;

#if original
	[HarmonyPrefix]
	[HarmonyPatch(typeof(GUIHand), "OnUpdate")]
	static public bool GUIHand_OnUpdate(GUIHand __instance)
	{
		var _this = new GUIHand_Subverted(__instance);

		_this.usedToolThisFrame = false; // __instance.usedToolThisFrame = false;
		_this.usedAltAttackThisFrame = false; // __instance.usedAltAttackThisFrame = false;
		_this.suppressTooltip = false; // __instance.suppressTooltip = false;
		if (__instance.player.IsFreeToInteract() && AvatarInputHandler.main.IsEnabled())
		{
			string text = string.Empty;
			PlayerTool tool = __instance.GetTool();
			EnergyMixin energyMixin = (EnergyMixin)null;
			if ((UnityEngine.Object)tool != (UnityEngine.Object)null)
			{
				text = tool.GetCustomUseText();
				energyMixin = tool.GetComponent<EnergyMixin>();
			}
			if ((UnityEngine.Object)energyMixin != (UnityEngine.Object)null && energyMixin.allowBatteryReplacement)
			{
				int num = Mathf.FloorToInt(energyMixin.GetEnergyScalar() * 100f);
				if (_this.cachedTextEnergyScalar != num)
				{
					_this.cachedEnergyHudText = num > 0 ? Language.main.GetFormat<float>("PowerPercent", energyMixin.GetEnergyScalar()) : LanguageCache.GetButtonFormat("ExchangePowerSource", GameInput.Button.Reload);
					_this.cachedTextEnergyScalar = num;
				}
				HandReticle.main.SetTextRaw(HandReticle.TextType.Use, text);
				HandReticle.main.SetTextRaw(HandReticle.TextType.UseSubscript, _this.cachedEnergyHudText);
			}
			else if (!string.IsNullOrEmpty(text))
				HandReticle.main.SetTextRaw(HandReticle.TextType.Use, text);
			if (!_this.IsPDAInUse())
			{
				if (_this.grabMode == GUIHand.GrabMode.None)
					_this.UpdateActiveTarget();
				HandReticle.main.SetTargetDistance(_this.activeHitDistance);
				if ((UnityEngine.Object)_this.activeTarget != (UnityEngine.Object)null && !_this.suppressTooltip)
				{
					TechType techType = CraftData.GetTechType(_this.activeTarget);
					if (techType != TechType.None)
						HandReticle.main.SetText(HandReticle.TextType.Hand, techType.AsString(), true);
					GUIHand.Send(_this.activeTarget, HandTargetEventType.Hover, __instance);
				}
				bool flag1 = GameInput.GetButtonDown(GameInput.Button.LeftHand);
				bool buttonHeld1 = GameInput.GetButtonHeld(GameInput.Button.LeftHand);
				bool buttonUp1 = GameInput.GetButtonUp(GameInput.Button.LeftHand);
				bool flag2 = GameInput.GetButtonDown(GameInput.Button.RightHand);
				bool buttonHeld2 = GameInput.GetButtonHeld(GameInput.Button.RightHand);
				bool buttonUp2 = GameInput.GetButtonUp(GameInput.Button.RightHand);
				bool buttonDown1 = GameInput.GetButtonDown(GameInput.Button.Reload);
				bool buttonDown2 = GameInput.GetButtonDown(GameInput.Button.Exit);
				bool buttonDown3 = GameInput.GetButtonDown(GameInput.Button.AltTool);
				bool buttonHeld3 = GameInput.GetButtonHeld(GameInput.Button.AltTool);
				bool buttonUp3 = GameInput.GetButtonUp(GameInput.Button.AltTool);
				PDAScanner.UpdateTarget(8f, buttonDown3 | buttonHeld3);
				if (PDAScanner.scanTarget.isValid && Inventory.main.container.Contains(TechType.Scanner) && PDAScanner.CanScan() == PDAScanner.Result.Scan && !PDAScanner.scanTarget.isPlayer)
					uGUI_ScannerIcon.main.Show();
				if ((UnityEngine.Object)tool != (UnityEngine.Object)null)
				{
					bool flag3;
					bool flag4;
					if (flag2)
					{
						if (tool.OnRightHandDown())
						{
							_this.usedToolThisFrame = true;
							tool.OnToolActionStart();
							flag2 = false;
							flag3 = false;
							flag4 = false;
						}
					}
					else if (buttonHeld2)
					{
						if (tool.OnRightHandHeld())
						{
							flag2 = false;
							flag3 = false;
						}
					}
					else if (buttonUp2 && tool.OnRightHandUp())
					{
						flag4 = false;
					}
					bool flag5;
					bool flag6;
					if (flag1)
					{
						if (tool.OnLeftHandDown())
						{
							tool.OnToolActionStart();
							flag1 = false;
							flag5 = false;
							flag6 = false;
						}
					}
					else if (buttonHeld1)
					{
						if (tool.OnLeftHandHeld())
						{
							flag1 = false;
							flag5 = false;
						}
					}
					else if (buttonUp1 && tool.OnLeftHandUp())
					{
						flag6 = false;
					}
					bool flag7;
					bool flag8;
					bool flag9;
					if (buttonDown3)
					{
						if (tool.OnAltDown())
						{
							_this.usedAltAttackThisFrame = true;
							tool.OnToolActionStart();
							flag7 = false;
							flag8 = false;
							flag9 = false;
						}
					}
					else if (buttonHeld3)
					{
						if (tool.OnAltHeld())
						{
						flag7 = false;
						flag8 = false;
						}
					}
					else if (buttonUp3 && tool.OnAltUp())
					{
						flag9 = false;
					}
					if (buttonDown1 && tool.OnReloadDown())
					{
					}
					if (buttonDown2 && tool.OnExitDown())
					{
					}
				}
				if ((UnityEngine.Object)tool == (UnityEngine.Object)null & flag2)
					Inventory.main.DropHeldItem(true);
				if (__instance.player.IsFreeToInteract() && !_this.usedToolThisFrame && (UnityEngine.Object)_this.activeTarget != (UnityEngine.Object)null && flag1)
					GUIHand.Send(_this.activeTarget, HandTargetEventType.Click, __instance);
			}
		}
		if (AvatarInputHandler.main.IsEnabled() && GameInput.GetButtonDown(GameInput.Button.AutoMove))
			GameInput.SetAutoMove(!GameInput.GetAutoMove());
		if (!AvatarInputHandler.main.IsEnabled() || !GameInput.GetButtonDown(GameInput.Button.PDA) || uGUI.isIntro || IntroLifepodDirector.IsActive)
			return false;
		__instance.player.GetPDA().Open();

		return false;
	}
#else
	[HarmonyLib.HarmonyPrefix]
	[HarmonyLib.HarmonyPatch(typeof(GUIHand), "OnUpdate")]
	static public bool GUIHand_OnUpdate(GUIHand __instance,
		ref bool ___usedToolThisFrame,
		ref bool ___usedAltAttackThisFrame,
		ref bool ___suppressTooltip,
		ref int ___cachedTextEnergyScalar,
		ref string ___cachedEnergyHudText,
		GUIHand.GrabMode ___grabMode,
		float ___activeHitDistance,
		GameObject ___activeTarget)
	{
		___usedToolThisFrame = false;
		___usedAltAttackThisFrame = false;
		___suppressTooltip = false;
		if ( __instance.player.IsFreeToInteract() && AvatarInputHandler.main.IsEnabled() ) {
			string text = string.Empty;
			PlayerTool tool = __instance.GetTool();
			EnergyMixin energyMixin = null;
			if ( tool != null ) {
				text = tool.GetCustomUseText();
				energyMixin = tool.GetComponent<EnergyMixin>();
			}

			if ( energyMixin != null && energyMixin.allowBatteryReplacement ) {
				int num = Mathf.FloorToInt(energyMixin.GetEnergyScalar() * 100f);
				if ( ___cachedTextEnergyScalar != num ) {
					___cachedEnergyHudText = num > 0
						? Language.main.GetFormat<float>("PowerPercent", energyMixin.GetEnergyScalar())
						: LanguageCache.GetButtonFormat("ExchangePowerSource", GameInput.Button.Reload);
					___cachedTextEnergyScalar = num;
				}
				HandReticle.main.SetTextRaw(HandReticle.TextType.Use, text);
				HandReticle.main.SetTextRaw(HandReticle.TextType.UseSubscript, ___cachedEnergyHudText);
			}
			else if ( !string.IsNullOrEmpty(text) ) {
				HandReticle.main.SetTextRaw(HandReticle.TextType.Use, text);
			}

			if ( !(bool)invoke_IsPDAInUse(__instance) ) {
				if ( ___grabMode == GUIHand.GrabMode.None )
					invoke_UpdateActiveTarget(__instance);

				HandReticle.main.SetTargetDistance(___activeHitDistance);
				if ( ___activeTarget != null && !___suppressTooltip ) {
					TechType techType = CraftData.GetTechType(___activeTarget);
					if ( techType != TechType.None )
						HandReticle.main.SetText(HandReticle.TextType.Hand, techType.AsString(), true);
					GUIHand.Send(___activeTarget, HandTargetEventType.Hover, __instance);
				}

				bool leftHandDown = GameInput.GetButtonDown(GameInput.Button.LeftHand);
				bool leftHandHeld = GameInput.GetButtonHeld(GameInput.Button.LeftHand);
				bool leftHandUp   = GameInput.GetButtonUp  (GameInput.Button.LeftHand);

				bool rightHandDown = GameInput.GetButtonDown(GameInput.Button.RightHand);
				bool rightHandHeld = GameInput.GetButtonHeld(GameInput.Button.RightHand);
				bool rightHandUp   = GameInput.GetButtonUp  (GameInput.Button.RightHand);

				bool reloadDown = GameInput.GetButtonDown(GameInput.Button.Reload);

				bool exitDown = GameInput.GetButtonDown(GameInput.Button.Exit);

				bool altToolDown = GameInput.GetButtonDown(GameInput.Button.AltTool);
				bool altToolHeld = GameInput.GetButtonHeld(GameInput.Button.AltTool);
				bool altToolUp   = GameInput.GetButtonUp  (GameInput.Button.AltTool);
				PDAScanner.UpdateTarget(8f, altToolDown | altToolHeld);
				if ( PDAScanner.scanTarget.isValid && Inventory.main.container.Contains(TechType.Scanner) && PDAScanner.CanScan() == PDAScanner.Result.Scan && !PDAScanner.scanTarget.isPlayer )
					uGUI_ScannerIcon.main.Show();

				if ( tool != null ) {
					if ( rightHandDown ) {
						if ( tool.OnRightHandDown() ) {
							___usedToolThisFrame = true;
							tool.OnToolActionStart();
							rightHandDown = false;
						}
					}
					else if ( rightHandHeld ) {
						if ( tool.OnRightHandHeld() )
							rightHandDown = false;
					}
					else if ( rightHandUp ) {
						tool.OnRightHandUp();
					}

					if ( leftHandDown ) {
						if ( tool.OnLeftHandDown() ) {
							tool.OnToolActionStart();
							leftHandDown = false;
						}
					}
					else if ( leftHandHeld ) {
						if ( tool.OnLeftHandHeld() )
							leftHandDown = false;
					}
					else if ( leftHandUp ) {
						tool.OnLeftHandUp();
					}

					if ( altToolDown ) {
						if ( tool.OnAltDown() ) {
							___usedAltAttackThisFrame = true;
							tool.OnToolActionStart();
						}
					}
					else if ( altToolHeld ) {
						tool.OnAltHeld();
					}
					else if ( altToolUp ) {
						tool.OnAltUp();
					}

					if ( reloadDown )
						tool.OnReloadDown();

					if ( exitDown )
						tool.OnExitDown();
				}

				if ( tool == null & rightHandDown )
					Inventory.main.DropHeldItem(true);

				var now = Time.time;
				var nextClickAvailable = now >= _timeNextClick;
				var singleClick = Plugin.config.useSingleClick ^ Nautilus.Utility.KeyCodeUtils.GetKeyHeld(Plugin.config.keyAltClick);
				var canClick = (singleClick & leftHandDown) | (!singleClick & leftHandHeld & nextClickAvailable);
				if ( __instance.player.IsFreeToInteract() && !___usedToolThisFrame && ___activeTarget != null && canClick ) {
					_timeNextClick = now + Plugin.config.timeClickInterval;
					GUIHand.Send(___activeTarget, HandTargetEventType.Click, __instance);
				}
			}
		}

		if ( AvatarInputHandler.main.IsEnabled() && GameInput.GetButtonDown(GameInput.Button.AutoMove) )
			GameInput.SetAutoMove(!GameInput.GetAutoMove());

		if ( !AvatarInputHandler.main.IsEnabled() || !GameInput.GetButtonDown(GameInput.Button.PDA) || uGUI.isIntro || IntroLifepodDirector.IsActive )
			return false;

		__instance.player.GetPDA().Open();

		return false;
	}
#endif
}

} // org.efool.subnautica.multiclick