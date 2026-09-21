using System.Collections.Generic;
using Gamelab.Data;

namespace Gamelab.Services.Shop
{
    public interface IShopService
    {
        List<CatalogItem> GenerateCatalog();
        CatalogItem GetCatalogItem(string itemId);
    }
}
