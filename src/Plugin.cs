using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using Jotunn;
using Jotunn.Managers;
using Jotunn.Utils;
using System;
using UnityEngine;
using static SmoothSailing.Plugin;

namespace SmoothSailing
{
    [BepInPlugin(ModGuid, ModName, ModVersion)]
    [BepInDependency(Jotunn.Main.ModGuid, BepInDependency.DependencyFlags.HardDependency)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Minor)]
    public sealed class Plugin : BaseUnityPlugin
    {
        public const string ModGuid = "p377y.valheim.smoothsailing";
        public const string ModName = "Smooth Sailing";
        public const string ModVersion = "0.2.0";

        internal static Plugin Instance;
        internal static BepInEx.Logging.ManualLogSource ModLog;
        internal static ConfigEntry<bool> Enabled;
        internal enum TailwindMode
        {
            Off = 0,
            DeadAstern = 1,
            PositiveOffset = 2,
            NegativeOffset = 3
        }

        internal static ConfigEntry<TailwindMode> TailwindModeSetting;
        internal static ConfigEntry<float> OffsetAngle;
        internal static ConfigEntry<KeyboardShortcut> AdminToggleKey;
        internal static ConfigEntry<float> ShipExploreRadiusMultiplier;
        internal static ConfigEntry<float> ForwardRowMultiplier;
        internal static ConfigEntry<float> ReverseRowMultiplier;

        private Harmony _harmony;

        private void Awake()
        {
            Instance = this;
            ModLog = Logger;
            BindConfig();

            _harmony = new Harmony(ModGuid);
            _harmony.PatchAll();

            Logger.LogInfo($"{ModName} {ModVersion} loaded.");
        }

        private void OnDestroy()
        {
            _harmony?.UnpatchSelf();
        }

        private void BindConfig()
        {
            var admin = new ConfigurationManagerAttributes
            {
                IsAdminOnly = true
            };

            Enabled = Config.Bind(
                "General",
                "Enabled",
                true,
                new ConfigDescription(
                    "Master switch for Smooth Sailing.",
                    null,
                    admin
                )
            );

            TailwindModeSetting = Config.Bind(
                "Tailwind",
                "Tailwind Mode",
                TailwindMode.DeadAstern,
                new ConfigDescription(
                    "Favorable-wind mode: Off, DeadAstern, PositiveOffset, or NegativeOffset.",
                    null,
                    admin
                )
            );

            OffsetAngle = Config.Bind(
                "Tailwind",
                "Offset Angle",
                60f,
                new ConfigDescription(
                    "Wind angle used by PositiveOffset and NegativeOffset. 0 = dead astern; 60 is near peak vanilla sail efficiency.",
                    new AcceptableValueRange<float>(0f, 90f),
                    admin
                )
            );

            AdminToggleKey = Config.Bind(
                "Tailwind",
                "Admin Toggle Key",
                new KeyboardShortcut(KeyCode.K),
                "Local hotkey used by a server administrator to cycle Off -> DeadAstern -> PositiveOffset -> NegativeOffset."
            );

            ShipExploreRadiusMultiplier = Config.Bind(
                "Exploration",
                "Ship Exploration Radius Multiplier",
                2f,
                new ConfigDescription(
                    "Multiplier applied to map exploration radius while aboard a ship. 1 = vanilla.",
                    new AcceptableValueRange<float>(1f, 20f),
                    admin
                )
            );

            ForwardRowMultiplier = Config.Bind(
                "Rowing",
                "Forward Row Speed Multiplier",
                1.5f,
                new ConfigDescription(
                    "Multiplier for forward rowing force (Slow speed). 1 = vanilla.",
                    new AcceptableValueRange<float>(0.1f, 20f),
                    admin
                )
            );

            ReverseRowMultiplier = Config.Bind(
                "Rowing",
                "Backward Row Speed Multiplier",
                2f,
                new ConfigDescription(
                    "Multiplier for reverse rowing force (Back speed). 1 = vanilla.",
                    new AcceptableValueRange<float>(0.1f, 20f),
                    admin
                )
            );
        }



        internal static string GetTailwindModeDisplay(TailwindMode mode)
        {
            switch (mode)
            {
                case TailwindMode.DeadAstern:
                    return "Smooth Sailing: DEAD ASTERN";

                case TailwindMode.PositiveOffset:
                    return $"Smooth Sailing: OFFSET +{OffsetAngle.Value:0.#}°";

                case TailwindMode.NegativeOffset:
                    return $"Smooth Sailing: OFFSET -{OffsetAngle.Value:0.#}°";

                default:
                    return "Smooth Sailing: OFF";
            }
        }

        internal static bool IsTailwindEnabled()
        {
            return Enabled.Value &&
                   TailwindModeSetting.Value != TailwindMode.Off;
        }

        internal static float GetTailwindAngle()
        {
            switch (TailwindModeSetting.Value)
            {
                case TailwindMode.PositiveOffset:
                    return OffsetAngle.Value;

                case TailwindMode.NegativeOffset:
                    return -OffsetAngle.Value;

                default:
                    return 0f;
            }
        }

        internal static Vector3 GetDesiredWindDirection(Ship ship)
        {
            if (ship == null)
                return Vector3.zero;

            return Quaternion.AngleAxis(
                GetTailwindAngle(),
                ship.transform.up
            ) * ship.transform.forward;
        }

        internal static bool IsLocallyControlledSailingShip(Ship ship)
        {
            if (!IsTailwindEnabled() || ship == null || Player.m_localPlayer == null)
                return false;

            if (Player.m_localPlayer.GetControlledShip() != ship)
                return false;

            Ship.Speed speed = ship.GetSpeedSetting();

            return speed == Ship.Speed.Half ||
                   speed == Ship.Speed.Full;
        }

        // Used for sail visuals.
        // Unlike IsLocallyControlledSailingShip(), this does not require
        // the local player to be the captain. This allows every client
        // to render the same favorable-wind sail orientation.
        internal static bool IsSailingShip(Ship ship)
        {
            if (!IsTailwindEnabled() || ship == null)
                return false;

            Ship.Speed speed = ship.GetSpeedSetting();

            return speed == Ship.Speed.Half ||
                   speed == Ship.Speed.Full;
        }

        internal static bool IsLocallyOwnedSailingShip(Ship ship)
        {
            if (!IsTailwindEnabled() || ship == null)
                return false;

            ZNetView nview = ship.GetComponent<ZNetView>();

            if (nview == null || !nview.IsOwner())
                return false;

            Ship.Speed speed = ship.GetSpeedSetting();

            return speed == Ship.Speed.Half ||
                   speed == Ship.Speed.Full;
        }

        internal static bool IsLocalPlayerAboardAnyShip()
        {
            return Enabled.Value &&
                   Player.m_localPlayer != null &&
                   ((Character)Player.m_localPlayer).IsAttachedToShip();
        }
    }


    // ------------------------------------------------------------
    // FAVORABLE WIND - SHIP PHYSICS
    // ------------------------------------------------------------

    [HarmonyPatch(typeof(Player), "Update")]
    internal static class PlayerUpdateHotkeyPatch
    {
        private static bool _toggleWasDown;

        private static void Postfix(Player __instance)
        {
            if (__instance == null || __instance != Player.m_localPlayer)
                return;

            KeyCode toggleKey = Plugin.AdminToggleKey.Value.MainKey;
            bool keyDown = Input.GetKey(toggleKey);

            if (!keyDown)
            {
                _toggleWasDown = false;
                return;
            }

            if (_toggleWasDown)
                return;

            _toggleWasDown = true;

            if (ZNet.instance == null)
            {
                ModLog.LogInfo("Smooth Sailing: ZNet.instance is null");
                return;
            }

            bool isAdmin = SynchronizationManager.Instance.PlayerIsAdmin;


            if (!isAdmin)
            {
                __instance.Message(
                    MessageHud.MessageType.TopLeft,
                    "Smooth Sailing: Admin only"
                );
                return;
            }

            TailwindMode currentMode = Plugin.TailwindModeSetting.Value;
            TailwindMode nextMode;

            switch (currentMode)
            {
                case TailwindMode.Off:
                    nextMode = TailwindMode.DeadAstern;
                    break;
                case TailwindMode.DeadAstern:
                    nextMode = TailwindMode.PositiveOffset;
                    break;
                case TailwindMode.PositiveOffset:
                    nextMode = TailwindMode.NegativeOffset;
                    break;
                default:
                    nextMode = TailwindMode.Off;
                    break;
            }

            Plugin.TailwindModeSetting.Value = nextMode;

            __instance.Message(
                MessageHud.MessageType.TopLeft,
                Plugin.GetTailwindModeDisplay(nextMode)
            );
        }
    }

    // ------------------------------------------------------------
    // SHIP HUD WIND INDICATOR
    // ------------------------------------------------------------

    // ------------------------------------------------------------
    // MINIMAP REAL-WIND SCOPE
    //
    // Let Valheim's Minimap.UpdateWindMarker run completely normally.
    // This scope only tells our EnvMan.GetWindDir postfix not to replace
    // the environmental wind while that exact minimap method is running.
    // ------------------------------------------------------------

    internal static class MinimapRealWindScope
    {
        internal static bool UpdatingWindMarker;
    }

    [HarmonyPatch(typeof(Minimap), "UpdateWindMarker")]
    internal static class MinimapUpdateWindMarkerScopePatch
    {
        private static void Prefix()
        {
            MinimapRealWindScope.UpdatingWindMarker = true;
        }

        private static void Postfix()
        {
            MinimapRealWindScope.UpdatingWindMarker = false;
        }

        private static Exception Finalizer(Exception __exception)
        {
            MinimapRealWindScope.UpdatingWindMarker = false;
            return __exception;
        }
    }


    internal static class ShipHudWindScope
    {
        internal static bool UpdatingShipHud;
    }

    [HarmonyPatch(typeof(Hud), "UpdateShipHud")]
    internal static class HudUpdateShipHudPatch
    {
        private static void Prefix()
        {
            ShipHudWindScope.UpdatingShipHud = true;
        }

        private static void Postfix()
        {
            ShipHudWindScope.UpdatingShipHud = false;
        }

        private static Exception Finalizer(Exception __exception)
        {
            // Make sure the scope cannot remain stuck on if UpdateShipHud throws.
            ShipHudWindScope.UpdatingShipHud = false;
            return __exception;
        }
    }

    [HarmonyPatch(typeof(Ship), nameof(Ship.GetWindAngle))]
    internal static class ShipGetWindAnglePatch
    {
        private static void Postfix(
            Ship __instance,
            ref float __result
        )
        {
            // Do not globally alter the game's wind angle. Only substitute the
            // Smooth Sailing angle while Valheim is updating the circular ship HUD.
            if (!ShipHudWindScope.UpdatingShipHud)
                return;

            if (!Plugin.IsTailwindEnabled())
                return;

            if (Plugin.IsLocallyControlledSailingShip(__instance) ||
                Plugin.IsLocallyOwnedSailingShip(__instance))
            {
                // Propulsion and physical sail are intentionally unchanged.
                // Circular HUD uses a 180-degree Dead Astern baseline, but its
                // signed +/- offset must be mirrored relative to the propulsion
                // angle: +60 -> 120 degrees, -60 -> 240 degrees.
                float visualAngle = Plugin.GetTailwindAngle();
                __result = Mathf.Repeat(360f - visualAngle, 360f);
            }
        }
    }


    // ------------------------------------------------------------
    // ROWING SPEED
    // ------------------------------------------------------------

    [HarmonyPatch(
        typeof(Ship),
        "CustomFixedUpdate",
        new Type[] { typeof(float) }
    )]
    internal static class ShipCustomFixedUpdatePatch
    {
        private static readonly System.Reflection.FieldInfo ForceField =
            AccessTools.Field(typeof(Ship), "m_force");

        private static readonly System.Reflection.FieldInfo BackwardForceField =
            AccessTools.Field(typeof(Ship), "m_backwardForce");

        internal sealed class State
        {
            public float Force;
            public float BackwardForce;
            public bool Modified;
        }

        private static void Prefix(
            Ship __instance,
            out State __state
        )
        {
            __state = new State();

            if (!Plugin.Enabled.Value || __instance == null)
                return;

            ZNetView nview = __instance.GetComponent<ZNetView>();

            if (nview == null || !nview.IsOwner())
                return;

            Ship.Speed speed = __instance.GetSpeedSetting();

            if (speed != Ship.Speed.Slow &&
                speed != Ship.Speed.Back)
            {
                return;
            }

            if (ForceField == null ||
                BackwardForceField == null)
            {
                return;
            }

            __state.Force =
                (float)ForceField.GetValue(__instance);

            __state.BackwardForce =
                (float)BackwardForceField.GetValue(__instance);

            __state.Modified = true;

            float multiplier =
                speed == Ship.Speed.Slow
                    ? Plugin.ForwardRowMultiplier.Value
                    : Plugin.ReverseRowMultiplier.Value;

            ForceField.SetValue(
                __instance,
                __state.Force * multiplier
            );

            BackwardForceField.SetValue(
                __instance,
                __state.BackwardForce * multiplier
            );
        }

        private static void Postfix(
            Ship __instance,
            State __state
        )
        {
            if (__state == null ||
                !__state.Modified ||
                __instance == null)
            {
                return;
            }

            ForceField?.SetValue(
                __instance,
                __state.Force
            );

            BackwardForceField?.SetValue(
                __instance,
                __state.BackwardForce
            );
        }
    }


    // ------------------------------------------------------------
    // MAP EXPLORATION
    // ------------------------------------------------------------

    [HarmonyPatch(
        typeof(Minimap),
        "Explore",
        new Type[] { typeof(Vector3), typeof(float) }
    )]
    internal static class MinimapExplorePatch
    {
        private static void Prefix(ref float radius)
        {
            if (!Plugin.Enabled.Value)
                return;

            if (Plugin.IsLocalPlayerAboardAnyShip())
            {
                radius *=
                    Plugin.ShipExploreRadiusMultiplier.Value;
            }
        }
    }


    // ------------------------------------------------------------
    // SAIL VISUAL
    //
    // Runs for every client that can see the ship.
    //
    // This intentionally does NOT require the local player to be
    // the captain. All players should therefore render the mast
    // using the same favorable-wind orientation.
    // ------------------------------------------------------------

    [HarmonyPatch(typeof(Ship), "UpdateSail")]
    internal static class ShipUpdateSailPatch
    {
        private static void Postfix(
            Ship __instance,
            float dt
        )
        {
            if (!Plugin.IsTailwindEnabled())
                return;

            if (!Plugin.IsSailingShip(__instance))
                return;

            if (__instance.m_mastObject == null)
                return;

            Vector3 windDir =
                Plugin.GetDesiredWindDirection(__instance);

            windDir = Vector3.Cross(
                Vector3.Cross(
                    windDir,
                    __instance.transform.up
                ),
                __instance.transform.up
            );

            Quaternion targetRotation =
                Quaternion.LookRotation(
                    -windDir,
                    __instance.transform.up
                );

            __instance.m_mastObject.transform.rotation =
                Quaternion.RotateTowards(
                    __instance.m_mastObject.transform.rotation,
                    targetRotation,
                    90f * dt
                );
        }
    }


    // ------------------------------------------------------------
    // LOCAL WIND DIRECTION
    //
    // This remains captain-specific because it affects the local
    // wind direction/HUD and sailing behavior.
    // ------------------------------------------------------------

    [HarmonyPatch(
        typeof(EnvMan),
        nameof(EnvMan.GetWindDir)
    )]
    internal static class EnvManGetWindDirPatch
    {
        private static void Postfix(
            ref Vector3 __result
        )
        {
            // Minimap.UpdateWindMarker already received the real EnvMan.m_wind
            // from the original GetWindDir(). Do not overwrite that result.
            if (MinimapRealWindScope.UpdatingWindMarker)
                return;

            if (!Plugin.IsTailwindEnabled())
                return;

            Player player =
                Player.m_localPlayer;

            Ship ship =
                player != null
                    ? player.GetControlledShip()
                    : null;

            if (!Plugin.IsLocallyControlledSailingShip(ship))
                return;

            __result =
                Plugin.GetDesiredWindDirection(ship);
        }
    }
}