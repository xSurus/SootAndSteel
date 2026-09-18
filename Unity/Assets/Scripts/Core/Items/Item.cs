namespace Gamelab.Items
{
    // ponytail: minimal stand-in for Src/Items/Item.cs. The real inventory system
    // (ItemRegistry, pricing, textures) is unbuilt and unassigned in Wave A/B as of
    // this port. Extend this type (or replace it) when that system gets built.
    public class Item
    {
        public string Id { get; }

        public Item(string id)
        {
            Id = id;
        }
    }
}
