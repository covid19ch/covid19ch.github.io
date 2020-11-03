namespace CovidStatsCH.Components
{
    public class Input
    {
        public int New { get; set; }
        public int Cumulative { get; set; }
        public static implicit operator Input((int New, int Cumulative) values)
        {
            return new Input
            {
                New = values.New,
                Cumulative = values.Cumulative
            };
        }
    }
}
