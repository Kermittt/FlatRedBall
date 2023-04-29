namespace WfcCore.Wfc
{
    public class WfcTerrain
    {
        public WfcTerrain(string name, string color, double probability)
        {
            Name = name;
            Color = color;
            Probability = probability;
        }

        public string Name { get; }
        public string Color { get; }
        public double Probability { get; set; }
    }
}
