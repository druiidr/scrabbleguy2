namespace scrabbleguy
{

    public class TileBag
    {
        Random random = new Random();
        private List<Tile> tiles;

        public TileBag()
        {
            tiles = new List<Tile>();
            InitializeBag();
        }

        // Initialize the bag with the correct number of tiles and scores
        private void InitializeBag()
        {
            AddTiles('A', 9, 1);
            AddTiles('B', 2, 3);
            AddTiles('C', 2, 3);
            AddTiles('D', 4, 2);
            AddTiles('E', 12, 1);
            AddTiles('F', 2, 4);
            AddTiles('G', 3, 2);
            AddTiles('H', 2, 4);
            AddTiles('I', 9, 1);
            AddTiles('J', 1, 8);
            AddTiles('K', 1, 5);
            AddTiles('L', 4, 1);
            AddTiles('M', 2, 3);
            AddTiles('N', 6, 1);
            AddTiles('O', 8, 1);
            AddTiles('P', 2, 3);
            AddTiles('Q', 1, 10);
            AddTiles('R', 6, 1);
            AddTiles('S', 4, 1);
            AddTiles('T', 6, 1);
            AddTiles('U', 4, 1);
            AddTiles('V', 2, 4);
            AddTiles('W', 2, 4);
            AddTiles('X', 1, 8);
            AddTiles('Y', 2, 4);
            AddTiles('Z', 1, 10);
        }

        // Method to add tiles to the bag
        public void AddTiles(char letter, int count, int score)
        {
            for (int i = 0; i < count; i++)
            {
                tiles.Add(new Tile(letter, score));
            }
        }

        // Draw a random tile from the bag
        public Tile DrawTile()
        {
            if (tiles.Count == 0) return null; // No more tiles in the bag
            int index = random.Next(tiles.Count);
            Tile drawnTile = tiles[index];
            tiles.RemoveAt(index);
            return drawnTile;
        }
        public bool IsEmpty()
        {
            return tiles.Count == 0;
        }
    }
}