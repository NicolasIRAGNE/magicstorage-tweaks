# Jacky's Magic Storage Tweaks

A lightweight client-side companion for the official [Magic Storage](https://steamcommunity.com/sharedfiles/filedetails/?id=2563309347) mod. It adds an “Include partial recipes” toggle that extends the available-recipes view with recipes for which the connected Magic Storage network supplies at least one direct ingredient.

The filter deliberately ignores crafting stations and recipe conditions. Magic Storage still computes normal availability, so a matching recipe remains marked unavailable when its full ingredient quantities, stations, or conditions are missing.

## Compatibility

- Internal mod name: `JackysMagicStorageTweaks`
- Display name: `Jacky's Magic Storage Tweaks`
- Side: client-only
- Required mod: Magic Storage `v0.7.0.11` or a compatible `v0.7.0.x` patch
- Adds no tiles, items, or world data; removing it does not alter Magic Storage networks

The implementation narrowly hooks Magic Storage's recipe-button initialization, public recipe query, and refresh methods. It refuses the unpublished `v0.7.1` line because that line substantially changes refresh internals.

## Using the filter

Open a Magic Storage Crafting Interface and toggle the gold crafting-grid icon with one green ingredient slot whose tooltip says “Include partial recipes.” It appears in the primary recipe-mode row after Magic Storage's optional favorites and blacklist buttons. The selected recipe mode remains unchanged: with the available-recipes mode selected, the toggle switches between fully available recipes and fully available plus partial recipes.

Recipe-group alternatives and Magic Storage infinite/module-provided ingredients count as present. Only ingredients required directly by the recipe are considered; recursive subrecipe ingredients are not used for inclusion.

The local test build is not a Workshop release.

## Local build

The project expects the standard tModLoader `tModLoader.targets` file one directory above the source and the official Magic Storage `v0.7.0.11` DLL at `../references/1.4.4/MagicStorage.dll`.

No Steam Workshop item is published by this repository.
