namespace Knight_Bishop
{
    public partial class Form1 : Form
    {
        Board board;
        Piece? selected = null;
        public Form1()
        {
            List<Piece> pieces = new List<Piece>();
            pieces.Add(new Piece(PieceColor.White, PieceVariant.Knight, 3, 3));
            pieces.Add(new Piece(PieceColor.White, PieceVariant.Bishop, 2, 1));
            pieces.Add(new Piece(PieceColor.White, PieceVariant.King, 2, 2));
            pieces.Add(new Piece(PieceColor.Black, PieceVariant.King, 1, 0));
            pieces.Add(new Piece(PieceColor.Black, PieceVariant.Knight, 0, 3));

            board = new(pieces, 40, 40);
            selected = pieces[1];
            InitializeComponent();
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            board.Draw(e.Graphics, selected);
        }
        private void pictureBox1OnClick(object sender, MouseEventArgs e)
        {

        }
    }
}