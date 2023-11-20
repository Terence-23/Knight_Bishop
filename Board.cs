using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Knight_Bishop
{
    public class Board
    {
        public List<Piece> pieces { get; }
        public PieceColor?[,] cellOccupants { get; }
        public int cell_width, cell_height;
        private SolidBrush lightSpace = new(Color.FromArgb(255, 150, 150, 150)), 
            darkSpace = new(Color.FromArgb(255, 80, 80, 80));
        private SolidBrush lightHighlight = new(Color.FromArgb(200, 150, 150)), 
            darkHighlight = new(Color.FromArgb(80, 80, 200));

        public Board(List<Piece> pieces)
        {
            this.pieces = pieces;
            cellOccupants = new PieceColor?[8, 8];
            for (int i = 0; i < 8; ++i)
            {
                for (int j = 0; j < 8; ++j)
                {
                    cellOccupants[i, j] = null;
                }
            }

            foreach (Piece piece in pieces)
            {
                cellOccupants[piece.position.x, piece.position.y] = piece.color;
            }
        }

        public Board(List<Piece> pieces, int cell_width, int cell_height) : this(pieces)
        {
            this.cell_width = cell_width;
            this.cell_height = cell_height;
        }

        public void Draw(Graphics g, Piece? selected)
        {
            for (int i = 0; i< 8; ++i)
            {
                for (int j = 0; j < 8; ++j)
                { 
                    g.FillRectangle(( 7 - i + j) % 2 == 0 ? darkSpace : lightSpace, 
                        i * cell_width, j * cell_height, 
                        cell_width, cell_height);
                }
            }

            if (selected != null)
            {
                foreach(var move in selected.PossibleMoves(this, true))
                {
                    g.FillRectangle((7-move.x + move.y) % 2==0?darkHighlight: lightHighlight,
                        move.x * cell_width, move.y * cell_height,
                        cell_width, cell_height);
                }
            }

            foreach (Piece piece in pieces)
            {
                piece.Draw(g, cell_width, cell_height);
            }

        }
    
        public bool Move(Piece piece, BoardPosition new_pos)
        {
            var possibleMoves = piece.PossibleMoves(this, true);
            if (possibleMoves.Contains(new_pos)){

                cellOccupants[piece.position.x , piece.position.y] = null;
                if (cellOccupants[new_pos.x, new_pos.y] != null)
                {
                    pieces.RemoveAll((Piece piece) => piece.position == new_pos);

                }
                cellOccupants[new_pos.x, new_pos.y] = piece.color;
                piece.position = new_pos;
                

                return true;
            }
            return false;
        }
    }
}
