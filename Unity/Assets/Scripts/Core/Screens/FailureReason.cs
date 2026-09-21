namespace Gamelab.Screens
{
    public enum FailureReason
    {
        AllPlayersKnockedOut,
        TrainFrozenHullBreached,
        TrainFrozenFurnaceOut,
        TrainFrozenBreachesAndFurnaceOut,
        TrainFrozenOther,
    }

    /// <summary>Cause texts of Src FailScreen.GetReasonText. Literal spacing and typos are Src's and kept.</summary>
    public static class FailureReasonText
    {
        public static string Get(FailureReason reason)
        {
            switch (reason)
            {
                case FailureReason.AllPlayersKnockedOut:
                    return "ALL WORKERS INCAPACITATED. ALL CARGO WAS STOLEN. TRAIN AND CREW WAS LEFT TO THE WEATHER";
                case FailureReason.TrainFrozenHullBreached:
                    return "TRAIN HULL WAS DESTROYED BY HORSE RIDERS, THE COLD CREPT IN" +
                           "AND TOOK OUT THE CREW. ALL CARGO LOST";
                case FailureReason.TrainFrozenFurnaceOut:
                    return "OVEN COULD NOT BE KEPT BURNING, CREW GOT TAKEN OUT BY THE COLD SIBERIAN WINTER, " +
                           "TRAIN WAS LEFT TO THE WEATHER.";
                case FailureReason.TrainFrozenBreachesAndFurnaceOut:
                    return "TRAIN WAS BREACHED, FURNANCE WAS FOUND OUT," +
                           "CREW WAS WIPED OUT BY THE COLD. WILD ANIMALS TRACKS FOUND ON THE TRACKS, NO CREW FOUND ";
                case FailureReason.TrainFrozenOther:
                    return "NO INFORMATION AVAILABLE; NO CREW FOUND";
                default:
                    return string.Empty;
            }
        }
    }
}
