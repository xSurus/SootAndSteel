using System;
using System.Numerics;
using Gamelab.PhysicalEntities.Interfaces;

namespace Gamelab.UI
{
    /// <summary>Static tooltip content copied once, plus the credit-dependent state of the left button.</summary>
    public sealed class TooltipModel
    {
        private TooltipModel() { }

        public string Title { get; private set; }
        public string Description { get; private set; }
        public string Functionality { get; private set; }
        public string CategoryName { get; private set; }
        public SpriteRect? IconSourceRect { get; private set; }
        public ButtonSpec Left { get; private set; }
        public ButtonSpec Right { get; private set; }
        public int? Cost { get; private set; }
        public bool LeftCanAfford { get; private set; } = true;

        public event Action Changed;

        public static TooltipModel Create(ITooltipable item, TooltipKind kind, int credits)
        {
            var (left, right) = TooltipInteractions.For(kind, item.Cost ?? 0);
            var m = new TooltipModel
            {
                Title = item.GetTitle(),
                Description = item.GetDescription(),
                Functionality = string.IsNullOrEmpty(item.FunctionalityName) ? "" : $"- {item.FunctionalityName} -",
                CategoryName = item.CategoryName,
                IconSourceRect = item.IconSourceRect,
                Left = left,
                Right = right,
                Cost = item.Cost,
            };
            m.LeftCanAfford = m.Affords(credits);
            return m;
        }

        public void Refresh(int credits)
        {
            bool now = Affords(credits);
            if (now == LeftCanAfford) return;
            LeftCanAfford = now;
            Changed?.Invoke();
        }

        private bool Affords(int credits) => Cost == null || credits >= Cost.Value;
    }
}
