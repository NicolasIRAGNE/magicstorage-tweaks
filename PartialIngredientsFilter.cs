using MagicStorage.CrossMod;
using MagicStorage.Sorting;

namespace JackysMagicStorageTweaks;

public sealed class PartialIngredientsFilter : FilteringOption {
	public override ItemFilter.Filter Filter => ItemFilter.All;

	public override string Texture => "JackysMagicStorageTweaks/Assets/PartialIngredients";

	public override string Name => "PartialIngredients";

	public override bool UsesFilterCache => false;

	public override bool GetDefaultVisibility(bool craftingGUI) => craftingGUI;

	public override Position GetDefaultPosition() => new AfterParent(FilteringOptionLoader.Definitions.All);
}
