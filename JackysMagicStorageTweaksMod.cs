using MagicStorage;
using MagicStorage.Common.Systems;
using MagicStorage.Sorting;
using MagicStorage.UI;
using MagicStorage.UI.States;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Terraria;
using Terraria.ModLoader;

namespace JackysMagicStorageTweaks;

public sealed class JackysMagicStorageTweaksMod : Mod {
	private delegate ParallelQuery<Recipe> OrigGetRecipes(StorageGUI.ThreadContext thread);
	private delegate ParallelQuery<Recipe> HookGetRecipes(OrigGetRecipes orig, StorageGUI.ThreadContext thread);
	private delegate void OrigRefreshItemsInner();
	private delegate void HookRefreshItemsInner(OrigRefreshItemsInner orig);
	private delegate void OrigInitFilterButtons(CraftingUIState.RecipesPage self);
	private delegate void HookInitFilterButtons(OrigInitFilterButtons orig, CraftingUIState.RecipesPage self);

	private static readonly FieldInfo RecipeButtonsField = typeof(CraftingUIState.RecipesPage).GetField(
		"recipeButtons",
		BindingFlags.Instance | BindingFlags.NonPublic)
		?? throw new MissingFieldException(typeof(CraftingUIState.RecipesPage).FullName, "recipeButtons");
	private static readonly HashSet<NewUIButtonChoice> HookedRecipeButtons = [];
	private static volatile bool includePartialRecipes;

	private static int PartialToggleChoice => 2
		+ (MagicStorageConfig.CraftingFavoritingEnabled ? 1 : 0)
		+ (MagicStorageConfig.RecipeBlacklistEnabled ? 1 : 0);

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

		MethodInfo refreshItemsInner = typeof(CraftingGUI).GetMethod(
			"RefreshItems_Inner",
			BindingFlags.NonPublic | BindingFlags.Static)
			?? throw new MissingMethodException(typeof(CraftingGUI).FullName, "RefreshItems_Inner");

		MethodInfo initFilterButtons = typeof(CraftingUIState.RecipesPage).GetMethod(
			"InitFilterButtons",
			BindingFlags.Instance | BindingFlags.NonPublic)
			?? throw new MissingMethodException(typeof(CraftingUIState.RecipesPage).FullName, "InitFilterButtons");

		MonoModHooks.Add(getRecipes, (HookGetRecipes)FilterRecipes);
		MonoModHooks.Add(refreshItemsInner, (HookRefreshItemsInner)ForceFullRecipeRefreshWhenIncluded);
		MonoModHooks.Add(initFilterButtons, (HookInitFilterButtons)AddPartialRecipeButton);
	}

	private static ParallelQuery<Recipe> FilterRecipes(OrigGetRecipes orig, StorageGUI.ThreadContext thread) {
		ParallelQuery<Recipe> recipes = orig(thread);
		if (!includePartialRecipes || !ReplaceAvailableChoiceWithCombinedChoice(thread.state))
			return recipes;

		var inventory = CraftingGUI.GetCurrentInventory();
		return recipes.Where(recipe => CraftingGUI.IsAvailable(recipe)
			|| recipe.requiredItem.Any(ingredient => inventory.GetTotalIngredientQuantity(recipe, ingredient.type) > 0));
	}

	private static void ForceFullRecipeRefreshWhenIncluded(OrigRefreshItemsInner orig) {
		if (MagicUI.IsCraftingUIOpen() && includePartialRecipes)
			MagicUI.ForceNextRefreshToBeFull = true;

		orig();
	}

	private static void AddPartialRecipeButton(OrigInitFilterButtons orig, CraftingUIState.RecipesPage self) {
		orig(self);

		if (self is DecraftingUIState.ShimmeringPage)
			return;

		NewUIButtonChoice buttons = GetRecipeButtons(self);
		buttons.AssignButtons(CreateRecipeButtons());

		if (includePartialRecipes)
			buttons.GeneralChoices.Add(PartialToggleChoice);

		PositionToggleOnRecipeRow(buttons);

		if (HookedRecipeButtons.Add(buttons)) {
			buttons.OnChoiceClicked += (choice, _) => {
				if (choice != PartialToggleChoice)
					return;

				includePartialRecipes = buttons.GeneralChoices.Contains(choice);
				MagicUI.SetRefresh(forceFullRefresh: true);
			};
		}
	}

	private static IEnumerable<ButtonChoiceInfo> CreateRecipeButtons() {
		yield return new ButtonChoiceInfo("MagicStorage/Assets/RecipeAvailable", "Mods.MagicStorage.RecipeAvailable", false);
		yield return new ButtonChoiceInfo("MagicStorage/Assets/RecipeAll", "Mods.MagicStorage.RecipeAll", false);

		if (MagicStorageConfig.CraftingFavoritingEnabled)
			yield return new ButtonChoiceInfo("MagicStorage/Assets/FilterMisc", "Mods.MagicStorage.ShowOnlyFavorited", false);

		if (MagicStorageConfig.RecipeBlacklistEnabled)
			yield return new ButtonChoiceInfo("MagicStorage/Assets/RecipeAll", "Mods.MagicStorage.RecipeBlacklist", false);

		yield return new ButtonChoiceInfo(
			"JackysMagicStorageTweaks/Assets/PartialIngredients",
			"Mods.JackysMagicStorageTweaks.IncludePartialRecipes",
			true);
	}

	private static NewUIButtonChoice GetRecipeButtons(CraftingUIState.RecipesPage page)
		=> (NewUIButtonChoice)RecipeButtonsField.GetValue(page)!;

	private static void PositionToggleOnRecipeRow(NewUIButtonChoice buttons) {
		Terraria.UI.UIElement[] elements = buttons.Children.ToArray();
		Terraria.UI.UIElement toggle = elements[^1];
		const int buttonSize = 32;
		const int buttonPadding = 1;
		const int togglePosition = 2;
		int buttonCount = PartialToggleChoice + 1;

		for (int i = togglePosition; i < elements.Length - 1; i++)
			elements[i].Left.Set((i + 1) * (buttonSize + buttonPadding), 0f);

		toggle.Left.Set(togglePosition * (buttonSize + buttonPadding), 0f);
		toggle.Top.Set(0f, 0f);

		float width = buttonCount * (buttonSize + buttonPadding) - buttonPadding;
		buttons.Width.Set(width, 0f);
		buttons.MinWidth.Set(width, 0f);
		buttons.Height.Set(buttonSize, 0f);
		buttons.MinHeight.Set(buttonSize, 0f);
	}

	private static bool ReplaceAvailableChoiceWithCombinedChoice(object state) {
		if (state is null)
			return false;

		FieldInfo recipeFilterChoice = state.GetType().GetField(
			"recipeFilterChoice",
			BindingFlags.Instance | BindingFlags.Public);

		if (recipeFilterChoice?.GetValue(state) is not int choice)
			return false;

		if (choice == CraftingGUI.RecipeButtonsAvailableChoice) {
			recipeFilterChoice.SetValue(state, PartialToggleChoice);
			return true;
		}

		return choice == PartialToggleChoice;
	}
}
