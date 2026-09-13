# P377Y Tailwind

A small Valheim 1.0 sailing-tweaks mod for BepInEx + Jotunn.

## Features
- Locks sail wind angle to tailwind while the local player controls a ship at Half/Full sail.
- Updates the local wind indicator to match the forced tailwind.
- Multiplies map exploration radius while aboard a ship.
- Separate forward and reverse rowing multipliers.
- Gameplay settings are Jotunn AdminOnly configs, so the dedicated server pushes them to clients.

## Requirements
- Valheim 1.0.x
- BepInExPack Valheim 5.4.2350+
- Jotunn 2.30.0+
- Install the mod on the dedicated server and every participating client.

## Build
Set these environment variables to your local installs:

- `VALHEIM_MANAGED` = `...\Valheim\valheim_Data\Managed`
- `BEPINEX_CORE` = `...\Valheim\BepInEx\core`
- `JOTUNN_DLL` = full path to `Jotunn.dll`

Then run:

```powershell
dotnet build .\P377YTailwind.csproj -c Release
```

Output:
`bin\Release\net472\P377YTailwind.dll`

## Default config
```ini
[General]
Enabled = true

[Tailwind]
Lock Tailwind = true
Update Wind Indicator = true

[Exploration]
Ship Exploration Radius Multiplier = 2

[Rowing]
Forward Row Speed Multiplier = 1.5
Backward Row Speed Multiplier = 2
```

## Notes
- The ship physics patch runs only on the network owner.
- Exploration is inherently client-side in Valheim, so clients need the DLL to receive the server-controlled larger reveal radius.
- The wind-indicator patch changes the local `EnvMan.GetWindDir()` result while you are actively sailing. That keeps the HUD aligned, but can also make local wind-driven visual effects point with the forced tailwind while at the helm. It does not write permanent weather state.
