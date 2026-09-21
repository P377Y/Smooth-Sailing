using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using Jotunn;
using Jotunn.Managers;
using Jotunn.Utils;
using MagicaCloth2;
using System;
using System.Collections.Generic;
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
        public const string ModVersion = "0.3.2";

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

        internal enum WindIntensityMode
        {
            Vanilla = 0,
            Minimum = 1,
            Maximum = 2
        }

        internal static ConfigEntry<TailwindMode> TailwindModeSetting;
        internal static ConfigEntry<float> OffsetAngle;
        internal static ConfigEntry<WindIntensityMode> WindIntensityModeSetting;
        internal static ConfigEntry<float> MinimumWindIntensity;
        internal static ConfigEntry<KeyboardShortcut> AdminToggleKey;
        internal static ConfigEntry<float> ShipExploreRadiusMultiplier;
        internal static ConfigEntry<float> ForwardRowMultiplier;
        internal static ConfigEntry<float> ReverseRowMultiplier;

        // Local MagicaCloth wind zones used only around ship sails.
        private static readonly Dictionary<Ship, MagicaWindZone> ShipWindZones =
            new Dictionary<Ship, MagicaWindZone>();

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

            foreach (KeyValuePair<Ship, MagicaWindZone> pair in ShipWindZones)
            {
                if (pair.Value != null)
                    Destroy(pair.Value.gameObject);
            }

            ShipWindZones.Clear();
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

            WindIntensityModeSetting = Config.Bind(
                "Tailwind",
                "Wind Intensity Mode",
                WindIntensityMode.Vanilla,
                new ConfigDescription(
                    "Sailing wind intensity while a favorable-wind mode is active: Vanilla uses world intensity, Minimum enforces the configured minimum, Maximum always uses full intensity.",
                    null,
                    admin
                )
            );

            MinimumWindIntensity = Config.Bind(
                "Tailwind",
                "Minimum Wind Intensity",
                0.5f,
                new ConfigDescription(
                    "Minimum sailing wind intensity used when Wind Intensity Mode is Minimum. Natural wind stronger than this value is preserved.",
                    new AcceptableValueRange<float>(0f, 1f),
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

        internal static float GetEffectiveWindIntensity(float worldIntensity)
        {
            if (!IsTailwindEnabled())
                return worldIntensity;

            switch (WindIntensityModeSetting.Value)
            {
                case WindIntensityMode.Minimum:
                    return Mathf.Max(worldIntensity, MinimumWindIntensity.Value);
                case WindIntensityMode.Maximum:
                    return 1f;
                default:
                    return worldIntensity;
            }
        }

        internal static MagicaWindZone GetOrCreateShipWindZone(Ship ship)
        {
            if (ship == null)
                return null;

            MagicaWindZone existing;
            if (ShipWindZones.TryGetValue(ship, out existing) && existing != null)
                return existing;

            MagicaCloth sailCloth = ship.GetComponentInChildren<MagicaCloth>(true);
            if (sailCloth == null)
                return null;

            GameObject zoneObject = new GameObject("SmoothSailing_SailWindZone");
            zoneObject.transform.SetParent(sailCloth.transform, false);
            zoneObject.transform.localPosition = Vector3.zero;
            zoneObject.transform.localRotation = Quaternion.identity;
            zoneObject.transform.localScale = Vector3.one;

            MagicaWindZone zone = zoneObject.AddComponent<MagicaWindZone>();
            zone.mode = MagicaWindZone.Mode.BoxDirection;
            zone.size = new Vector3(12f, 12f, 4f);
            zone.main = 0f;
            zone.turbulence = 1f;
            zone.isAddition = false;

            ShipWindZones[ship] = zone;
            return zone;
        }

        internal static void UpdateShipWindZone(Ship ship)
        {
            if (ship == null)
                return;

            MagicaWindZone zone;
            ShipWindZones.TryGetValue(ship, out zone);

            if (!IsSailingShip(ship))
            {
                if (zone != null)
                    zone.enabled = false;
                return;
            }

            zone = GetOrCreateShipWindZone(ship);
            if (zone == null)
                return;

            if (!zone.enabled)
                zone.enabled = true;

            zone.SetWindDirection(GetDesiredWindDirection(ship));

            float worldIntensity = EnvMan.instance != null
                ? EnvMan.instance.GetWindIntensity()
                : 0f;
            float effectiveIntensity = GetEffectiveWindIntensity(worldIntensity);

            // Same conversion Valheim uses for EnvMan.m_clothWindZone.
            zone.main = Mathf.Pow(effectiveIntensity, 2f) * 100f;
        }

        internal static void RemoveShipWindZone(Ship ship)
        {
            if (ship == null)
                return;

            MagicaWindZone zone;
            if (!ShipWindZones.TryGetValue(ship, out zone))
                return;

            ShipWindZones.Remove(ship);
            if (zone != null)
                Destroy(zone.gameObject);
        }

        private static readonly System.Reflection.FieldInfo SailForceFactorField =
            AccessTools.Field(typeof(Ship), "m_sailForceFactor");

        private static readonly System.Reflection.FieldInfo SailForceField =
            AccessTools.Field(typeof(Ship), "m_sailForce");

        private static readonly System.Reflection.FieldInfo WindChangeVelocityField =
            AccessTools.Field(typeof(Ship), "m_windChangeVelocity");

        internal static bool TryCalculateSmoothSailForce(
            Ship ship,
            float sailSize,
            float dt,
            out Vector3 result)
        {
            result = Vector3.zero;

            if (!IsTailwindEnabled() || ship == null ||
                SailForceFactorField == null ||
                SailForceField == null ||
                WindChangeVelocityField == null)
                return false;

            // Only replace vanilla propulsion on the client responsible for
            // simulating this sailing ship.
            if (!IsLocallyControlledSailingShip(ship) &&
                !IsLocallyOwnedSailingShip(ship))
                return false;

            Vector3 windDir = GetDesiredWindDirection(ship).normalized;

            float worldIntensity =
                EnvMan.instance != null
                    ? EnvMan.instance.GetWindIntensity()
                    : 0f;

            float effectiveIntensity =
                GetEffectiveWindIntensity(worldIntensity);

            float intensityFactor =
                Mathf.Lerp(0.25f, 1f, effectiveIntensity);

            float dot =
                Vector3.Dot(windDir, -ship.transform.forward);

            float angleEfficiency =
                Mathf.Lerp(0.7f, 1f, 1f - Utils.Abs(dot));

            float headwindCutoff =
                1f - Utils.LerpStep(0.75f, 0.8f, dot);

            float windAngleFactor =
                angleEfficiency * headwindCutoff * intensityFactor;

            float sailForceFactor =
                (float)SailForceFactorField.GetValue(ship);

            Vector3 currentSailForce =
                (Vector3)SailForceField.GetValue(ship);

            Vector3 windChangeVelocity =
                (Vector3)WindChangeVelocityField.GetValue(ship);

            Vector3 combined =
                windDir + ship.transform.forward;

            Vector3 target =
                combined.sqrMagnitude > 0.000001f
                    ? combined.normalized *
                      (windAngleFactor * sailForceFactor * sailSize)
                    : Vector3.zero;

            currentSailForce =
                Vector3.SmoothDamp(
                    currentSailForce,
                    target,
                    ref windChangeVelocity,
                    1f,
                    99f,
                    dt
                );

            SailForceField.SetValue(ship, currentSailForce);
            WindChangeVelocityField.SetValue(ship, windChangeVelocity);

            result = currentSailForce;
            return true;
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

            if (ZNet.instance == null ||
                !SynchronizationManager.Instance.PlayerIsAdmin)
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

    [HarmonyPatch(typeof(Ship), nameof(Ship.GetWindAngleFactor))]
    internal static class ShipGetWindAngleFactorPatch
    {
        private static void Postfix(Ship __instance, ref float __result)
        {
            if (!Plugin.IsTailwindEnabled() || __instance == null)
                return;

            // Apply only while this client is actually responsible for the
            // sailing ship: either the local captain or the network owner.
            if (!Plugin.IsLocallyControlledSailingShip(__instance) &&
                !Plugin.IsLocallyOwnedSailingShip(__instance))
            {
                return;
            }

            // Reproduce Valheim's vanilla GetWindAngleFactor() math, but use
            // Smooth Sailing's effective wind direction instead of EnvMan's
            // environmental wind.
            Vector3 windDir = Plugin.GetDesiredWindDirection(__instance);
            float dot = Vector3.Dot(windDir, -__instance.transform.forward);
            float angleEfficiency = Mathf.Lerp(0.7f, 1f, 1f - Utils.Abs(dot));
            float headwindCutoff = 1f - Utils.LerpStep(0.75f, 0.8f, dot);

            __result = angleEfficiency * headwindCutoff;
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
    // SAILING WIND INTENSITY
    // ------------------------------------------------------------

    internal static class ShipSailForceScope
    {
        internal static bool CalculatingSailForce;
        internal static Ship CurrentShip;
    }

    [HarmonyPatch(typeof(Ship), "GetSailForce")]
    internal static class ShipGetSailForceScopePatch
    {
        private static bool Prefix(
            Ship __instance,
            float sailSize,
            float dt,
            ref Vector3 __result)
        {
            ShipSailForceScope.CalculatingSailForce = true;
            ShipSailForceScope.CurrentShip = __instance;

            // When Smooth Sailing is active, calculate the complete vanilla-style
            // sail force ourselves using the effective direction and intensity.
            // Returning false skips vanilla GetSailForce(), preventing real world
            // wind from leaking into the final target force vector.
            if (Plugin.TryCalculateSmoothSailForce(
                    __instance,
                    sailSize,
                    dt,
                    out Vector3 smoothResult))
            {
                __result = smoothResult;
                ShipSailForceScope.CalculatingSailForce = false;
                ShipSailForceScope.CurrentShip = null;
                return false;
            }

            return true;
        }

        private static void Postfix()
        {
            ShipSailForceScope.CalculatingSailForce = false;
            ShipSailForceScope.CurrentShip = null;
        }

        private static Exception Finalizer(Exception __exception)
        {
            ShipSailForceScope.CalculatingSailForce = false;
            ShipSailForceScope.CurrentShip = null;
            return __exception;
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
        private static readonly System.Reflection.FieldInfo BackwardForceField =
            AccessTools.Field(typeof(Ship), "m_backwardForce");

        internal sealed class State
        {
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

            if (BackwardForceField == null)
                return;

            __state.BackwardForce =
                (float)BackwardForceField.GetValue(__instance);

            __state.Modified = true;

            float multiplier =
                speed == Ship.Speed.Slow
                    ? Plugin.ForwardRowMultiplier.Value
                    : Plugin.ReverseRowMultiplier.Value;

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
            // Keep the sail-local cloth zone synchronized, and disable it
            // immediately whenever Smooth Sailing/active sailing is off.
            Plugin.UpdateShipWindZone(__instance);

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


    [HarmonyPatch(typeof(Ship), "OnDisable")]
    internal static class ShipOnDisablePatch
    {
        private static void Prefix(Ship __instance)
        {
            Plugin.RemoveShipWindZone(__instance);
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
            // Propulsion gets first priority. GetSailForce() reads wind direction
            // directly and GetWindAngleFactor() reads it again, so force both calls
            // to use the exact same Smooth Sailing direction for the ship currently
            // being simulated. This is independent of local captain/network ownership.
            if (ShipSailForceScope.CalculatingSailForce &&
                Plugin.IsTailwindEnabled() &&
                ShipSailForceScope.CurrentShip != null)
            {
                __result =
                    Plugin.GetDesiredWindDirection(
                        ShipSailForceScope.CurrentShip
                    );
                return;
            }

            // The minimap must continue to show true environmental wind.
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