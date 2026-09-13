# Smooth Sailing

Smooth Sailing is a lightweight, configurable sailing quality-of-life mod for Valheim 1.0, built for BepInEx + Jotunn.

It improves sailing without changing the core ship experience, with configurable favorable wind, faster rowing, and increased map exploration while traveling by ship.

## Features

### Favorable Wind
- Forces favorable tailwind while controlling a ship at Half or Full sail.
- Wind direction automatically follows the direction of the ship while sailing.
- Designed to eliminate situations where unfavorable wind brings sailing to a crawl.

### Improved Rowing
- Configurable forward rowing speed multiplier.
- Configurable reverse rowing speed multiplier.
- Forward and reverse speeds can be tuned independently.

### Ship Exploration
- Configurable map exploration radius multiplier while aboard a ship.
- Makes ocean exploration and coastline mapping significantly more convenient.

### Multiplayer Configuration
- Gameplay settings use Jotunn synchronized AdminOnly configuration.
- Server administrators control the gameplay settings.
- Changes can be synchronized to connected clients.

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
Lock Tailwind = true
Update Wind Indicator = true

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

**Lock Tailwind**

Forces favorable tailwind while controlling a ship at Half or Full sail.

Default: `true`

**Update Wind Indicator**

Controls the wind-indicator behavior associated with Smooth Sailing.

Default: `true`

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

## Multiplayer

Smooth Sailing is intended to support both single-player and multiplayer.

For multiplayer, install the mod on:

- The dedicated server, if one is being used.
- Every player connecting to the server.

Gameplay configuration is synchronized using Jotunn. Server administrators control the synchronized settings.

Some Smooth Sailing functionality is necessarily performed client-side, including map exploration and local wind-related behavior.

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
- Steps to reproduce the problem
- Your BepInEx `LogOutput.log` when possible

## Building from Source

Smooth Sailing targets .NET Framework 4.7.2.

Open:

`SmoothSailing.slnx`

in Visual Studio and build using the **Release** configuration.

The compiled mod will be generated at:

`bin/Release/net472/SmoothSailing.dll`

The project requires references to the Valheim, BepInEx, Harmony, and Jotunn assemblies.

## License

Smooth Sailing is released under the MIT License.

See `License.txt` for details.