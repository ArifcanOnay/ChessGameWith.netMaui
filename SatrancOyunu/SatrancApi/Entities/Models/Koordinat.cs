namespace SatrancApi.Entities.Models
{
    public class Koordinat
    {
        public int X { get; set; }
        public int Y { get; set; }

        public Koordinat() { }

        public Koordinat(int x, int y)
        {
            X = x;
            Y = y;
        }
    }
}
