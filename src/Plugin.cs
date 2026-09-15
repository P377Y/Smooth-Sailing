using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using Jotunn.Utils;
using System;
using UnityEngine;

namespace SmoothSailing
{
    [BepInPlugin(ModGuid, ModName, ModVersion)]
    [BepInDependency(Jotunn.Main.ModGuid, BepInDependency.DependencyFlags.HardDependency)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Minor)]
    public sealed class Plugin : BaseUnityPlugin
    {
        public const string ModGuid = "p377y.valheim.smoothsailing";
        public const string ModName = "Smooth Sailing";
        public const string ModVersion = "0.1.2";

        internal static Plugin Instance;
        internal static ConfigEntry<bool> Enabled;
        internal static ConfigEntry<bool> LockTailwind;
        internal static ConfigEntry<bool> UpdateWindIndicator;
        internal static ConfigEntry<float> ShipExploreRadiusMultiplier;
        internal static ConfigEntry<float> ForwardRowMultiplier;
        internal static ConfigEntry<float> ReverseRowMultiplier;

        private Harmony _harmony;

        private void Awake()
        {
            Instance = this;
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

            LockTailwind = Config.Bind(
                "Tailwind",
                "Lock Tailwind",
                true,
                new ConfigDescription(
                    "Treat wind as directly behind the controlled ship while using half/full sail.",
                    null,
                    admin
                )
            );

            UpdateWindIndicator = Config.Bind(
                "Tailwind",
                "Update Wind Indicator",
                true,
                new ConfigDescription(
                    "Make the local wind direction indicator match the forced tailwind while sailing.",
                    null,
                    admin
                )
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

        internal static bool IsLocallyControlledSailingShip(Ship ship)
        {
            if (!Enabled.Value || ship == null || Player.m_localPlayer == null)
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
            if (!Enabled.Value || ship == null)
                return false;

            Ship.Speed speed = ship.GetSpeedSetting();

            return speed == Ship.Speed.Half ||
                   speed == Ship.Speed.Full;
        }

        internal static bool IsLocallyOwnedSailingShip(Ship ship)
        {
            if (!Enabled.Value || ship == null)
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

    [HarmonyPatch(typeof(Ship), nameof(Ship.GetWindAngle))]
    internal static class ShipGetWindAnglePatch
    {
        private static void Postfix(
            Ship __instance,
            ref float __result
        )
        {
            if (!Plugin.Enabled.Value || !Plugin.LockTailwind.Value)
                return;

            if (Plugin.IsLocallyControlledSailingShip(__instance) ||
                Plugin.IsLocallyOwnedSailingShip(__instance))
            {
                __result = 0f;
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
            if (!Plugin.Enabled.Value ||
                !Plugin.LockTailwind.Value)
            {
                return;
            }

            if (!Plugin.IsSailingShip(__instance))
                return;

            if (__instance.m_mastObject == null)
                return;

            Vector3 windDir =
                __instance.transform.forward;

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
            if (!Plugin.Enabled.Value ||
                !Plugin.LockTailwind.Value)
            {
                return;
            }

            Player player =
                Player.m_localPlayer;

            Ship ship =
                player != null
                    ? player.GetControlledShip()
                    : null;

            if (!Plugin.IsLocallyControlledSailingShip(ship))
                return;

            __result =
                ship.transform.forward;
        }
    }
}