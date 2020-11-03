namespace CovidStatsCH.Components
{
    public class ComparedExtendedInput : ExtendedInput
    {
        public int? Additional { get; set; }
        public ComparedExtendedInput(ExtendedInput input, int? additional)
        {
            Additional = additional;
            Cumulative = input.Cumulative;
            New = input.New;
            SemiUnreliable = input.SemiUnreliable;
            SevenDayAverage = input.SevenDayAverage;
            SevenDayAverageAWeekEarlier = input.SevenDayAverageAWeekEarlier;
            Unreliable = input.Unreliable;
        }
        public ComparedExtendedInput(ExtendedInput input, ExtendedInput? reference) : this(input, input.New - (reference?.New ?? 0))
        {
        }

        public ComparedExtendedInput(ExtendedInput input)
        {
            Cumulative = input.Cumulative;
            New = input.New;
            SemiUnreliable = input.SemiUnreliable;
            SevenDayAverage = input.SevenDayAverage;
            SevenDayAverageAWeekEarlier = input.SevenDayAverageAWeekEarlier;
            Unreliable = input.Unreliable;
        }
        public ComparedExtendedInput()
        {
        }
    }
}
