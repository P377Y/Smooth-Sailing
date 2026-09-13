# Changelog

## 0.1.1

### Important
The internal mod ID was renamed in 0.1.1. Existing 0.1.0 settings may need to be reconfigured because Smooth Sailing now uses a new config file.

- Renamed the internal mod/project branding from P377Y Tailwind to Smooth Sailing.
- Changed tailwind handling to better follow the active ship captain in multiplayer.
- Improved favorable-wind behavior during captain changes and multiplayer sailing.
- Renamed the compiled mod to `SmoothSailing.dll`.
- Added public GitHub source repository and bug tracker.
- Added structured GitHub bug reports for multiplayer and sailing issues.

### Testing
Multiplayer tailwind behavior is still being actively tested. If you encounter a sailing, captain-switching, exploration-radius, or networking issue, please report it here:

https://github.com/P377Y/Smooth-Sailing/issues

## 0.1.0
- Initial development build.
- Tailwind lock while controlling a ship at half/full sail.
- Matching local wind indicator.
- Ship exploration-radius multiplier.
- Separate forward and reverse rowing multipliers.
- Server-synchronized admin-only configuration through Jotunn.
