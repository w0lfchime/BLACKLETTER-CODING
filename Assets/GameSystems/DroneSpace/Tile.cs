namespace DroneSpace
{
    public sealed class Tile
    {
        public readonly int x;
        public readonly int y;

        // fields (expand as needed)
        public byte type;   
        public byte flags; 

        public Tile(int x, int y)
        {
            this.x = x;
            this.y = y;
            type = 0;
            flags = 0;
        }

        public override string ToString() => $"Tile({x},{y}) type={type}";
    }
}
