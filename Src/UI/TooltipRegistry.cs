using System.Collections.Generic;
using Gamelab.Utils;

namespace Gamelab.UI;

public class TooltipData
{
    public string Title { get; set; }
    public string Description { get; set; }
}

public class TooltipRegistry
{
    private Dictionary<string, TooltipData> tooltips = new();

    public void Load(JsonLoader loader)
    {
        var loadedData = loader.LoadJson<Dictionary<string, TooltipData>>("tooltips.json");
        if (loadedData != null)
        {
            tooltips = loadedData;
        }
    }

    public TooltipData Get(string stationType)
    {
        return tooltips.GetValueOrDefault(stationType);
    }
}