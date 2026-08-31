using MagicStorage;
using MagicStorage.Common.Systems;
using MagicStorage.CrossMod;
using MagicStorage.Sorting;
using System;
using System.Linq;
using System.Reflection;
using Terraria;
using Terraria.ModLoader;

namespace JackysMagicStorageTweaks;

public sealed class JackysMagicStorageTweaksMod : Mod {
	private delegate ParallelQuery<Recipe> OrigGetRecipes(StorageGUI.ThreadContext thread);
	private delegate ParallelQuery<Recipe> HookGetRecipes(OrigGetRecipes orig, StorageGUI.ThreadContext thread);
	private delegate void OrigSetRefresh(bool forceFullRefresh);
	private delegate void HookSetRefresh(OrigSetRefresh orig, bool forceFullRefresh);

	public override void Load() {
		if (!ModLoader.TryGetMod("MagicStorage", out Mod magicStorage))
			throw new InvalidOperationException("Magic Storage is required.");

		Version version = magicStorage.Version;
		if (version.Major != 0 || version.Minor != 7 || version.Build != 0 || version.Revision < 11)
			throw new NotSupportedException($"Magic Storage {version} is not supported. Jacky's Magic Storage Tweaks targets the v0.7.0.11 stable line.");

		MethodInfo getRecipes = typeof(ItemSorter).GetMethod(
			nameof(ItemSorter.GetRecipes),
			BindingFlags.Public | BindingFlags.Static,
			binder: null,
			types: [typeof(StorageGUI.ThreadContext)],
			modifiers: null)
			?? throw new MissingMethodException(typeof(ItemSorter).FullName, nameof(ItemSorter.GetRecipes));

		MethodInfo setRefresh = typeof(MagicUI).GetMethod(
			nameof(MagicUI.SetRefresh),
			BindingFlags.Public | BindingFlags.Static,
			binder: null,
			types: [typeof(bool)],
			modifiers: null)
			?? throw new MissingMethodException(typeof(MagicUI).FullName, nameof(MagicUI.SetRefresh));

		MonoModHooks.Add(getRecipes, (HookGetRecipes)FilterRecipes);
		MonoModHooks.Add(setRefresh, (HookSetRefresh)ForceFullRefreshWhenSelected);
	}

	private static ParallelQuery<Recipe> FilterRecipes(OrigGetRecipes orig, StorageGUI.ThreadContext thread) {
		ParallelQuery<Recipe> recipes = orig(thread);
		if (thread.filterMode != ModContent.GetInstance<PartialIngredientsFilter>().Type)
			return recipes;

		var inventory = CraftingGUI.GetCurrentInventory();
		return recipes.Where(recipe => recipe.requiredItem.Any(ingredient => inventory.GetTotalIngredientQuantity(recipe, ingredient.type) > 0));
	}

	private static void ForceFullRefreshWhenSelected(OrigSetRefresh orig, bool forceFullRefresh) {
		bool partialFilterSelected = MagicUI.IsCraftingUIOpen()
			&& FilteringOptionLoader.Selected == ModContent.GetInstance<PartialIngredientsFilter>().Type;

		orig(forceFullRefresh || partialFilterSelected);
	}
}
