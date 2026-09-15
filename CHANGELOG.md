# Changelog

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
