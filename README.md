# Smooth Sailing

Smooth Sailing is a lightweight, configurable boat and sailing quality-of-life mod for Valheim 1.0, built for BepInEx + Jotunn.

It improves sailing without replacing the core ship experience, with selectable favorable-wind modes, configurable sailing wind intensity, improved rowing, and increased map exploration while traveling by ship.

## Features

### Favorable Wind

Smooth Sailing can provide a favorable effective wind while controlling a ship at Half or Full sail.

Four sailing modes are available:

- **Off** - Vanilla wind behavior.
- **Dead Astern** - Effective wind directly behind the ship.
- **+60° Offset** - Favorable wind offset to one side of the ship.
- **-60° Offset** - Favorable wind offset to the opposite side.

The offset angle is configurable and defaults to 60°.

A server administrator can cycle the modes in-game using the configured Admin Toggle Key (default: **K**):

`Off -> Dead Astern -> +60° -> -60° -> Off`

The selected mode is synchronized so participating clients use the same sailing behavior.

### Sailing Wind Intensity

Smooth Sailing can independently control the effective wind intensity used for sailing while a favorable-wind mode is active.

Three intensity modes are available:

- **Vanilla** - Uses Valheim's current environmental wind intensity.
- **Minimum** - Enforces a configurable minimum sailing wind intensity while preserving stronger natural wind.
- **Maximum** - Always uses maximum sailing wind intensity.

The default intensity mode is **Vanilla**, preserving Valheim's normal variation in wind strength unless an administrator chooses otherwise.

### Accurate Sailing Visuals

When a favorable-wind mode is active:

- The physical sail responds to the effective sailing wind direction.
- Sail cloth strength reflects the effective Smooth Sailing wind intensity.
- The circular sailing wind indicator displays the effective wind direction and favorable-wind state.
- Positive and negative offsets appear on their corresponding sides.
- The minimap wind arrow remains tied to Valheim's true environmental wind.
- Valheim's world/environmental wind remains unchanged.

This allows the ship's propulsion, sail, and sailing HUD to represent the effective wind actually affecting the ship while preserving normal world-wind information outside the sailing system.

### Improved Rowing

- Configurable forward rowing speed multiplier.
- Configurable reverse rowing speed multiplier.
- Forward and reverse speeds can be tuned independently.

### Ship Exploration

- Configurable map exploration radius multiplier while aboard a ship.
- Makes ocean exploration and coastline mapping significantly more convenient.

### Multiplayer & Server Configuration

- Gameplay settings use Jotunn synchronized AdminOnly configuration.
- Server administrators control synchronized gameplay settings.
- Favorable-wind modes can be changed live by an administrator.
- Selected wind modes are synchronized to participating clients.
- Designed for dedicated-server as well as single-player use.

## Requirements

- Valheim 1.0.x
- BepInExPack Valheim 5.4.2350+
- Jotunn 2.30.0+
- Smooth Sailing must be installed on the server and all participating clients for multiplayer use.

## Installation

### Thunderstore

Install **Smooth Sailing** through a compatible Thunderstore mod manager along with its required dependencies.

### Manual Installation

Place:

`SmoothSailing.dll`

inside:

`BepInEx/plugins/SmoothSailing/`

The mod will generate its configuration file after the game is launched.

## Default Configuration

```ini
[General]
Enabled = true

[Tailwind]
Tailwind Mode = DeadAstern
Offset Angle = 60
Wind Intensity Mode = Vanilla
Minimum Wind Intensity = 0.5
Admin Toggle Key = K

[Exploration]
Ship Exploration Radius Multiplier = 2

[Rowing]
Forward Row Speed Multiplier = 1.5
Backward Row Speed Multiplier = 2
```

## Configuration

### General

**Enabled**

Enables or disables Smooth Sailing.

Default: `true`

### Tailwind

**Tailwind Mode**

Controls the effective sailing-wind mode.

Available values:

- `Off`
- `DeadAstern`
- `PositiveOffset`
- `NegativeOffset`

Default: `DeadAstern`

**Offset Angle**

Controls the angle used by the PositiveOffset and NegativeOffset modes.

`0` represents dead astern. The default is `60` degrees.

Default: `60`

**Wind Intensity Mode**

Controls the effective wind strength used for ship propulsion and sail visuals while a favorable-wind mode is active.

Available values:

- `Vanilla` - Uses the current environmental wind intensity.
- `Minimum` - Uses at least the configured Minimum Wind Intensity, while preserving stronger natural wind.
- `Maximum` - Uses maximum wind intensity.

Default: `Vanilla`

**Minimum Wind Intensity**

Sets the minimum effective sailing wind intensity when Wind Intensity Mode is set to `Minimum`.

The value ranges from `0` to `1`. Natural wind stronger than this setting is preserved.

Default: `0.5`

**Admin Toggle Key**

Allows a server administrator to cycle the Tailwind Mode while in game.

Default: `K`

Cycle order:

`Off -> DeadAstern -> PositiveOffset -> NegativeOffset -> Off`

Changing modes displays the selected mode on screen.

### Exploration

**Ship Exploration Radius Multiplier**

Multiplies the normal map exploration radius while aboard a ship.

Default: `2`

### Rowing

**Forward Row Speed Multiplier**

Multiplier applied while rowing forward.

Default: `1.5`

**Backward Row Speed Multiplier**

Multiplier applied while rowing in reverse.

Default: `2`

## Why 60 Degrees?

Valheim does not produce its maximum forward sailing force with the wind directly behind the ship.

The selectable offset-wind modes in Smooth Sailing were inspired by **HelmWind by SirRomey**, which analyzed Valheim's vanilla sail-force behavior.

SirRomey's analysis found peak sail efficiency at approximately **63° off dead astern**, with a 60° offset effectively reaching that peak. HelmWind estimates this produces approximately **2.5% more forward speed than sailing directly downwind**, with the tradeoff of additional lateral drift.

Smooth Sailing therefore defaults its offset modes to 60°, while allowing the angle to be configured.

## Credits & Inspiration

Special thanks to **SirRomey**, creator of **HelmWind**, for the analysis and concept that inspired Smooth Sailing's offset-wind modes.

HelmWind demonstrated the advantage of maintaining approximately a 60° favorable wind angle rather than simply forcing the wind directly astern.

Smooth Sailing expands on that concept with:

- Server-synchronized favorable-wind modes.
- Dedicated-server support.
- Administrator-only live mode cycling.
- Both positive and negative configurable offsets.
- Synchronized sailing behavior for participating clients.
- Physical sail and sailing-indicator integration.
- Configurable Vanilla, Minimum, and Maximum sailing wind intensity.
- Sail-cloth visuals that reflect the effective sailing wind strength.
- Preservation of Valheim's true environmental wind on the minimap and outside the sailing system.

HelmWind:

https://thunderstore.io/c/valheim/p/SirRomey/HelmWind/

## Multiplayer

Smooth Sailing supports both single-player and multiplayer.

For multiplayer, install the mod on:

- The dedicated server, if one is being used.
- Every player connecting to the server.

Gameplay configuration is synchronized using Jotunn's AdminOnly configuration system, allowing the server administrator to control synchronized settings for all players.

Some Smooth Sailing functionality is necessarily performed client-side, including map exploration and visual presentation.

Favorable-wind ship behavior is designed to remain consistent when different players take control of the ship, including situations where the active captain and the ship's network owner are different players. Favorable sail visuals are also applied across participating clients.

Multiplayer captain switching, network ownership behavior, and sail visuals continue to benefit from community testing. Bug reports are welcome.

## Reporting Bugs

Found a problem with Smooth Sailing?

Please report it through the **Issues** section of the Smooth Sailing GitHub repository:

https://github.com/P377Y/Smooth-Sailing/issues

When reporting a multiplayer or sailing issue, please include:

- Smooth Sailing version
- Valheim version
- BepInEx and Jotunn versions
- Single-player or multiplayer
- Dedicated server or locally hosted game
- Number of players
- Whether you were the ship captain when the problem occurred
- Ship type and sail setting
- Selected Smooth Sailing wind mode
- Selected Wind Intensity Mode and Minimum Wind Intensity, when relevant
- Steps to reproduce the problem
- Your BepInEx `LogOutput.log` when possible

## Building from Source

Smooth Sailing targets .NET Framework 4.7.2.

Open:

`SmoothSailing.slnx`

in Visual Studio and build using the **Release** configuration.

The compiled mod will be generated at:

`bin/Release/net472/SmoothSailing.dll`

The project requires references to the Valheim, BepInEx, Harmony, Jotunn, and MagicaCloth2 assemblies.

## License

Smooth Sailing is released under the MIT License.

See `License.txt` for details.