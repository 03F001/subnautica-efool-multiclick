# efool's Multiclick Mod for Subnautica

- Hold `Left Hand/Use` button to quickly break resources and collect items
	- No longer need to bind mouse wheel to `Left Hand/Use` button
- Use `Left Alt` to revert to single-click behavior
	- Modifier key is configurable
- Revert to single-click by default and instead use `Left Alt` for multi-click
	- Configure how you prefer to play, the mod supports both

# Requirements

- Subnautica patch Oct-2025 83031
- Tobey's BepInEx Pack v5
	- [Nexus Mods](https://www.nexusmods.com/subnautica/mods/1108)
	- [Github](https://github.com/toebeann/BepInEx.Subnautica)
- Nautilus v1.0.0-pre.46 (or later)
	- [Nexus Mods](https://www.nexusmods.com/subnautica/mods/1262)
	- [Github](https://github.com/SubnauticaModding/Nautilus)

# Installation

- Install Tobey's BepInEx Pack v5
- Install Nautilus v1.0.0-pre.46 (or later)
- Extract `efool-multiclick_#.#.#.zip` to `BepInEx/plugins`
	- `[game]/BepInEx/plugins/efool-multiclick/efool-multiclick.dll`

Note: `[game]` is the directory containing `Subnautica.exe`

# Console Commands

| Description                  | Command             |
| ---------------------------- | --------------------|
| Seconds between multi-clicks | multiclick_interval |

# Known Issues

- Some actions like building and editing labels work better with single-click
	- Workaround: use alternate click modifier button

# Other Mods

- [efool's Custom Inventory](https://github.com/03F001/subnautica-efool-custom-inventory)
	- Hoover up everything you find and throw nothing away
	- Bulk inventory operations has similar motivation to this multi-click mod
