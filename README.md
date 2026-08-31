# Jacky's Magic Storage Tweaks

A lightweight client-side companion for the official [Magic Storage](https://steamcommunity.com/sharedfiles/filedetails/?id=2563309347) mod. It adds a crafting filter that keeps a recipe visible when the connected Magic Storage network supplies at least one of its direct ingredients.

The filter deliberately ignores crafting stations and recipe conditions. Magic Storage still computes normal availability, so a matching recipe remains marked unavailable when its full ingredient quantities, stations, or conditions are missing.

## Compatibility

- Internal mod name: `JackysMagicStorageTweaks`
- Display name: `Jacky's Magic Storage Tweaks`
- Side: client-only
- Required mod: Magic Storage `v0.7.0.11` or a compatible `v0.7.0.x` patch
- Adds no tiles, items, or world data; removing it does not alter Magic Storage networks

The implementation uses Magic Storage's public filtering extension and narrowly hooks its public recipe query and refresh methods. It refuses the unpublished `v0.7.1` line because that line substantially changes refresh internals.

## Using the filter

Open a Magic Storage Crafting Interface and select the material-filter icon whose tooltip says “Show recipes with at least one stored ingredient.” In the modern configurable button layout, first enable `Partial Ingredients` from Magic Storage's filtering configuration page if it is not already assigned to a visible button.

Recipe-group alternatives and Magic Storage infinite/module-provided ingredients count as present. Only ingredients required directly by the recipe are considered; recursive subrecipe ingredients are not used for inclusion.

The local test build is not a Workshop release. A distinct, non-template `icon.png` must be added before a future Workshop publication.

## Local build

The project expects the standard tModLoader `tModLoader.targets` file one directory above the source and the official Magic Storage `v0.7.0.11` DLL at `../references/1.4.4/MagicStorage.dll`.

No Steam Workshop item is published by this repository.
