# FearMe

**Gear up, become feared!**

This mod aims to make progression slightly more rewarding by causing enemies to react to the level of your character.
As you get better gear, enemies think twice about attacking, or even outright flee.

## Requirements

- https://github.com/BepInEx/BepInEx
- https://github.com/Valheim-Modding/Jotunn

## Installation

Recommended to use https://thunderstore.io/package/ebkr/r2modman/

### Manual

***BepInExPack for Valheim** and **Jotunn** are required*

1. Install BepInExPack Valheim https://thunderstore.io/c/valheim/p/denikson/BepInExPack_Valheim/
2. Install Jotunn https://thunderstore.io/c/valheim/p/ValheimModding/Jotunn/
3. Extract this archive into your Bepinex\plugins folder.

## Configuration

If using https://github.com/shudnal/ConfigurationManager, levels for existing mods/items can be modified in-game. A value of -1 will cause the entry to be ignored.

Alternatively, all handled values can be modified in the config file. Entries can be added, removed, or set to -1 to ignore.

## Compatibility

- Primarily modifies the monsters' AI, so possibly incompatible with other similar mods.
- Only the base monsters and gear are configured. Anything custom should be ignored.

## Source

Github: https://github.com/tulivu/ValheimMods/tree/main/src/FearMe

## Changelog

### v0.1.0

Initial release

### v0.2.0

https://github.com/tulivu/ValheimMods/issues/1

- Fix config not loading correctly
- Add crude UI for ConfigurationManager