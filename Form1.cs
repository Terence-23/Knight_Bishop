namespace Knight_Bishop
{
    public partial class Form1 : Form
    {
        Board board;
        Piece? selected = null;
        Game? game = null;
        public Form1()
        {
            List<Piece> pieces = new List<Piece>
            {
                new Piece(PieceColor.Black, PieceVariant.King, 7, 7),
                new Piece(PieceColor.White, PieceVariant.Bishop, 0, 6),
                new Piece(PieceColor.White, PieceVariant.King, 4, 4),
                new Piece(PieceColor.Black, PieceVariant.Bishop, 1, 1),
                //new Piece(PieceColor.Black, PieceVariant.Knight, 0, 3)
            };

            board = new(pieces, 50, 50);
            InitializeComponent();
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            board.Draw(e.Graphics, selected);
        }
        private void pictureBox1OnClick(object sender, MouseEventArgs e)
        {
            BoardPosition tile = new(e.X / board.cell_width, e.Y / board.cell_height);

            if (selected != null && game != null)
            {

                if (MoveStatus.Failed == board.ManualMove(selected, tile))
                {
                    selected = null;
                    goto end;
                }
                textBox1.Text += 
                    selected.color == PieceColor.White ? 
                    $"{game.turnCount + 1}. {(char)selected.variant}{tile} ":
                    $"{(char)selected.variant}{tile}" + Environment.NewLine;
                
                selected = null;
                game.NextMove();
            }
            else if (game != null && board.cellOccupants[tile.x, tile.y] == game.currentMove)
            {
                foreach (Piece piece in board.pieces)
                {
                    if (piece.position == tile)
                    {
                        selected = piece;
                    }
                }
            }
            else
            {
                selected = null;
            }

            end:
            pictureBox1.Refresh();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            // game start all
            game = new Game(
                GameType.All,
                new NaiveMover(),
                new CenterMover(),
                board,
                pictureBox1,
                textBox1
                );
            selected = null;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            // game start black
            game = new Game(
                GameType.Black,
                new NaiveMover(),
                new CenterMover(),
                board,
                pictureBox1,
                textBox1
                );
            selected = null;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // game start white
            game = new Game(
                GameType.White,
                new NaiveMover(),
                new CenterMover(),
                board,
                pictureBox1,
                textBox1
                );
            selected = null;
        }
    }
}