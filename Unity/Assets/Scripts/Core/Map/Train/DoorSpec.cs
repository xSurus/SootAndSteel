namespace Gamelab.Map.Train
{
    public readonly struct DoorSpec
    {
        public bool OnBottom { get; }
        public int Column { get; }

        public DoorSpec(bool onBottom, int column)
        {
            OnBottom = onBottom;
            Column = column;
        }
    }
}
