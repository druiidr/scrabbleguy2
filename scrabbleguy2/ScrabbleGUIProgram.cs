using System;
using System.Windows.Forms;

namespace scrabbleguy
{
    class ScrabbleGuiProgram
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Ask the user if they want to play against AI
            if (MessageBox.Show("Play against AI?", "Game Mode", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                Application.Run(new ScrabbleGUI(true));
            }
            else
            {
                Application.Run(new ScrabbleGUI(false));
            }
        }
    }
}