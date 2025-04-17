using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace scrabbleguy
{
    public class Tile
    {
        public char Letter { get; private set; }
        public int Score { get; private set; }

        public Tile(char letter, int score)
        {
            Letter = letter;
            Score = score;
        }
        
        public override string ToString()
        {
            return (""+Letter); 
        }
    }

}
