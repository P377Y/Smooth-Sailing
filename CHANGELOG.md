# Changelog

## 0.3.4 - Multiplayer Sync Improvements

- Added server-authoritative synchronization for Smooth Sailing mode changes.
- Admin hotkey changes are now sent to the server, validated, and synchronized to connected players.
- Improved sail visual synchronization so non-controlling players should see the same favorable-wind sail orientation as the captain.
- Added proper handling for solo and locally hosted worlds.
- Mode-change notifications now appear after the server confirms the change.
- Improved support for multiple actively sailing ships, with favorable wind calculated independently relative to each ship's heading.
- Additional multiplayer testing and feedback is welcome, especially with multiple players and multiple ships sailing simultaneously.

## 0.3.3

### Changed

- Improved sail visual behavior to more closely preserve Valheim's native sail rotation and positioning.
- Smooth Sailing now substitutes its effective wind direction while retaining Valheim's native sail-angle calculation and rotation speed.

### Technical

- Audited sail propulsion, rowing propulsion, wind intensity, ship exploration, and MagicaCloth wind handling against the current Valheim implementation.

## 0.3.2

### Fixed

- Fixed ships bouncing or jumping while rowing, particularly at high rowing speed multipliers.
- Corrected rowing force handling so forward and reverse multipliers no longer affect ship buoyancy.
- Improved rowing stability across the full configurable multiplier range.

## 0.3.1

### Fixed

- Updated ship cleanup handling for compatibility with Valheim 1.0.12.
- Replaced the removed `Ship.OnDestroy` lifecycle hook with `Ship.OnDisable`.
- Restored clean Smooth Sailing initialization following the latest Valheim/Jotunn update.

### Changed

- Added "boat" terminology to package metadata and README for improved discoverability.

## 0.3.0

### Added

- Added configurable sailing wind intensity with three modes:
  - **Vanilla** — uses Valheim's current environmental wind intensity.
  - **Minimum** — enforces a configurable minimum sailing wind intensity while preserving stronger natural wind.
  - **Maximum** — always uses maximum sailing wind intensity.
- Added configurable **Minimum Wind Intensity**, defaulting to `0.5`.
- Added ship-local MagicaCloth wind handling so sail cloth visually responds to Smooth Sailing's effective wind strength.

### Improved

- Ship propulsion now uses Smooth Sailing's effective wind direction and intensity together in the sail-force calculation.
- Sail cloth strength now matches the effective sailing wind instead of remaining dependent on the world's current wind intensity.
- Sailing HUD favorable-wind state now reflects the effective Smooth Sailing wind used for propulsion.
- Wind direction, wind intensity, propulsion, sail orientation, sail cloth, and the sailing HUD now use a consistent effective sailing-wind model.
- Valheim's true environmental wind remains unchanged outside Smooth Sailing's ship-specific sailing behavior.
- The minimap wind arrow continues to display the true environmental wind.
- Offset modes continue to default to 60°.

### Fixed

- Fixed Maximum wind intensity producing full-speed calculations while the physical sail could remain visually flat during low environmental wind.
- Fixed ships losing sail propulsion when sailing into the true environmental headwind even though Smooth Sailing was supplying a favorable effective wind.
- Fixed the sailing wind indicator appearing unfavorable when the effective Smooth Sailing wind was favorable.
- Fixed residual environmental wind direction leaking into the final sail-force vector and causing stalled or unintended lateral movement in Dead Astern mode.

### Testing

Version 0.3.0 has been tested with:

- Vanilla, Minimum, and Maximum wind-intensity modes.
- Dead Astern, positive offset, negative offset, and Off sailing modes.
- Maximum effective wind while sailing directly into the true environmental headwind.
- Sail-cloth response at different environmental wind intensities.
- Sailing HUD and minimap wind-indicator behavior.

Additional multiplayer and dedicated-server testing is welcome, particularly around captain changes, network ownership, and sail visuals across clients.

Please report issues at:

https://github.com/P377Y/Smooth-Sailing/issues


## 0.2.0

### Added

- Added selectable sailing wind modes:
  - Off — vanilla wind behavior.
  - Dead Astern — favorable wind directly behind the ship.
  - +60° Offset — favorable wind offset to one side.
  - -60° Offset — favorable wind offset to the opposite side.
- Added a configurable favorable-wind offset angle, defaulting to 60°.
- Added in-game wind-mode cycling for the local host and server administrators.
- Added configurable Admin Toggle Key, defaulting to `K`.
- Added on-screen feedback when changing sailing modes.
- Added synchronization of the selected sailing mode for multiplayer and dedicated servers.

### Improved

- Expanded favorable-wind support beyond the original dead-astern tailwind lock.
- Physical sail orientation now reflects Smooth Sailing's effective sailing wind.
- The circular sailing HUD indicator now reflects the effective sailing wind.
- Positive and negative offset modes correctly display the effective wind on their respective sides.
- The minimap wind arrow continues to display Valheim's true environmental wind rather than the wind being applied by Smooth Sailing.
- Improved multiplayer synchronization so participating clients use the server-selected sailing mode.
- Retained support for captain/network-owner differences introduced in previous multiplayer updates.

### Credits & Inspiration

- Added selectable offset-wind modes inspired by **HelmWind by SirRomey**.
- SirRomey's analysis of Valheim's vanilla sail-force curve identifies peak efficiency at approximately 63° off dead astern. HelmWind uses a practical 60° default, which sits effectively at the peak and is estimated to provide roughly 2.5% more forward speed than dead-astern sailing, at the cost of additional lateral drift.
- Smooth Sailing builds on this concept with configurable positive and negative offsets, dedicated-server support, synchronized sailing behavior, administrator-controlled live mode cycling, and integrated sail/HUD visuals while preserving Valheim's true environmental wind on the minimap.

HelmWind:
https://thunderstore.io/c/valheim/p/SirRomey/HelmWind/

### Testing

Version 0.2.0 significantly expands Smooth Sailing's wind-handling and multiplayer behavior. Additional community testing is especially welcome for:

- Dedicated servers with multiple players.
- Switching captains while a favorable-wind mode is active.
- Cases where the captain is not the current network owner of the ship.
- Switching between Off, Dead Astern, +Offset, and -Offset modes during sailing.
- Sail orientation as viewed by passengers and other players.
- Sailing HUD and minimap wind-indicator behavior across clients.
- Different ship types.

Please report issues at:

https://github.com/P377Y/Smooth-Sailing/issues


## 0.1.2

### Multiplayer Tailwind Improvements

- Improved multiplayer tailwind handling when the ship captain and network owner are different players.
- Favorable wind is now applied to ship physics for both the active captain and the client currently responsible for the ship's network simulation.
- This addresses cases where a non-owner captain could see favorable wind on their HUD while the ship itself still behaved as though it were sailing into the real environmental wind.
- Improved forced-tailwind sail visuals so the favorable sail orientation can be rendered for non-captain players as well.
- Improved sail visual stability while turning and while the underlying environmental wind changes.
- Updated internal sailing checks to better separate ship physics, captain-specific wind behavior, and shared sail visuals.

### Testing

Multiplayer tailwind and sail visual behavior are still being actively tested.

In particular, additional testing is welcome for:
- Captain changes between players.
- Cases where the captain is not the current network owner of the ship.
- Sailing directly into unfavorable environmental wind.
- Sail orientation as viewed by passengers and other non-captain players.

If you encounter a sailing, captain-switching, sail-visual, exploration-radius, or networking issue, please report it here:

https://github.com/P377Y/Smooth-Sailing/issues


## 0.1.1

### Important

The internal mod ID was renamed in 0.1.1. Existing 0.1.0 settings may need to be reconfigured because Smooth Sailing now uses a new config file.

- Renamed the internal mod/project branding from P377Y Tailwind to Smooth Sailing.
- Changed tailwind handling to better follow the active ship captain in multiplayer.
- Improved favorable-wind behavior during captain changes and multiplayer sailing.
- Renamed the compiled mod to `SmoothSailing.dll`.
- Added public GitHub source repository and bug tracker.
- Added structured GitHub bug reports for multiplayer and sailing issues.


## 0.1.0

- Initial development build.
- Tailwind lock while controlling a ship at half/full sail.
- Matching local wind indicator.
- Ship exploration-radius multiplier.
- Separate forward and reverse rowing multipliers.
- Server-synchronized admin-only configuration through Jotunn.