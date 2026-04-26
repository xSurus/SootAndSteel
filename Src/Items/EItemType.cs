namespace Gamelab.Items;

/// <summary>
/// High-level category of a buyable / interactable game item, used to drive the tooltip's
/// category label and icon. <see cref="Basic"/> and <see cref="Upgrading"/> are the two
/// flavours of bullet-component dispensers; the others tag standalone stations.
/// </summary>
public enum EItemType
{
    /// <summary>A baseline bullet component (BasicCasing, BasicProjectile, BasicPropellant, ...).</summary>
    Basic,

    /// <summary>A non-baseline bullet component that upgrades a basic one (HomingCasing, ScatterProjectile, ...).</summary>
    Upgrading,

    /// <summary>A station that performs a non-crafting world utility action (e.g. SpeedLever, Cannon).</summary>
    Utility,

    /// <summary>A station the player actively crafts at (e.g. Anvil).</summary>
    Crafting,

    /// <summary>A station that primarily holds / passes items between players (e.g. Workbench, Counter).</summary>
    Storage,
}
