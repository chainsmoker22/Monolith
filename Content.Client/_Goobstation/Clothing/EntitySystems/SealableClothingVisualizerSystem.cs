// SPDX-FileCopyrightText: 2025 NotActuallyMarty
// SPDX-FileCopyrightText: 2025 starch
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Client._Goobstation.Clothing.Components;
using Content.Client.Clothing;
using Content.Shared._Goobstation.Clothing;
using Content.Shared.Clothing;
using Content.Shared.Inventory;
using Content.Shared.Item;
using Robust.Client.GameObjects;
using System.Linq;

namespace Content.Client._Goobstation.Clothing.EntitySystems;

public sealed class SealableClothingVisualizerSystem : VisualizerSystem<SealableClothingVisualsComponent>
{
    [Dependency] private readonly SharedItemSystem _itemSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<SealableClothingVisualsComponent, GetEquipmentVisualsEvent>(OnGetEquipmentVisuals, after: new[] { typeof(ClientClothingSystem) });
    }

    protected override void OnAppearanceChange(EntityUid uid, SealableClothingVisualsComponent component, ref AppearanceChangeEvent args)
    {
        if (!AppearanceSystem.TryGetData<bool>(uid, SealableClothingVisuals.Sealed, out var isSealed, args.Component))
            return;

        if (args.Sprite != null && component.SpriteLayer != null && args.Sprite.LayerMapTryGet(component.SpriteLayer, out var layer))
        {
            args.Sprite.LayerSetVisible(layer, isSealed);
        }

        _itemSystem.VisualsChanged(uid);
    }

    private void OnGetEquipmentVisuals(Entity<SealableClothingVisualsComponent> sealable, ref GetEquipmentVisualsEvent args)
    {
        var (uid, comp) = sealable;

        if (!TryComp(uid, out AppearanceComponent? appearance)
            || !AppearanceSystem.TryGetData<bool>(uid, SealableClothingVisuals.Sealed, out var isSealed, appearance)
            || !isSealed)
            return;

        if (!comp.ClothingVisuals.TryGetValue(args.Slot, out var layers))
            return;
        // attempt to get species specific data || if none found will use generic data instead
        if (TryComp(args.Equipee, out InventoryComponent? inventory) &&
            inventory.SpeciesId != null &&
            comp.ClothingVisuals.TryGetValue($"{args.Slot}-{inventory.SpeciesId}", out var speciesLayers))
            layers = speciesLayers;

        var i = 0;
        foreach (var layer in layers)
        {
            var key = layer.MapKeys?.FirstOrDefault();
            if (key == null)
            {
                key = i == 0 ? $"{args.Slot}-sealed" : $"{args.Slot}-sealed-{i}";
                i++;
            }

            args.Layers.Add((key, layer));
        }
    }
}
