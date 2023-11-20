using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Knight_Bishop
{
    public class Piece
    {
        public PieceColor color { get; }
        public PieceVariant variant { get; }
        public BoardPosition position { get; set; }

        public Piece(PieceColor color, PieceVariant variant)
        {
            this.color = color;
            this.variant = variant;
            position = new(0, 0);
        }

        public Piece(PieceColor color, PieceVariant variant, BoardPosition position) : this(color, variant)
        {
            this.position = position;
        }
        public Piece(PieceColor color, PieceVariant variant, int x, int y) : this(color, variant, new(x, y)) { }        

        public void Draw(Graphics g, int cell_width, int cell_height)
        {
            switch (color) {
                case PieceColor.White: 
                    g.DrawString(
                        ((char)this.variant).ToString(), 
                        new Font("Arial", 12), 
                        Brushes.White, 
                        (float)(position.x + 0.2) * cell_width, 
                        (float)(position.y + 0.2) * cell_height);
                    break;
                case PieceColor.Black:
                    g.DrawString(
                        ((char)this.variant).ToString(), 
                        new Font("Arial", 12), 
                        Brushes.Black, 
                        (float)(position.x + 0.2) * cell_width, 
                        (float)(position.y + 0.2) * cell_height);
                    break;
            }
        }
        public List<BoardPosition> PossibleMoves(Board board, bool blocked)
        {
            List<BoardPosition> possibleMoves = new();
            switch (variant)
            {
                case PieceVariant.Bishop:

                    var position = (BoardPosition) this.position.Clone();
                    position.x--;
                    position.y--;

                    while (position.IsValid())
                    {

                        if (board.cellOccupants[position.x, position.y] == color)
                        {
                            break;
                        }
                        else if (board.cellOccupants[position.x, position.y] != null)
                        {
                            // Enemy piece
                            possibleMoves.Add((BoardPosition) position.Clone());
                            break;
                        }
                        possibleMoves.Add((BoardPosition) position.Clone());

                        position.x--;
                        position.y--;

                    }

                    position = (BoardPosition)this.position.Clone();

                    position.x--;
                    position.y++;
                    while (position.IsValid()) 
                    {

                        if (board.cellOccupants[position.x, position.y] == color)
                        {
                            break;
                        }
                        else if (board.cellOccupants[position.x, position.y] != null)
                        {
                            // Enemy piece
                            possibleMoves.Add((BoardPosition)position.Clone());
                            break;
                        }
                        possibleMoves.Add((BoardPosition)position.Clone());

                        position.x--;
                        position.y++;

                    }

                    position = (BoardPosition)this.position.Clone();
                    position.x++;
                    position.y--;

                    while (position.IsValid())
                    {

                        if (board.cellOccupants[position.x, position.y] == color)
                        {
                            break;
                        }
                        else if (board.cellOccupants[position.x, position.y] != null)
                        {
                            // Enemy piece
                            possibleMoves.Add((BoardPosition)position.Clone());
                            break;
                        }
                        possibleMoves.Add((BoardPosition)position.Clone());

                        position.x++;
                        position.y--;

                    } 
                    
                    position = (BoardPosition)this.position.Clone();
                    position.x++;
                    position.y++;

                    while (position.IsValid())
                    {

                        if (board.cellOccupants[position.x, position.y] == color)
                        {
                            break;
                        }
                        else if (board.cellOccupants[position.x, position.y] != null)
                        {
                            // Enemy piece
                            possibleMoves.Add((BoardPosition)position.Clone());
                            break;
                        }
                        possibleMoves.Add((BoardPosition)position.Clone());

                        position.x++;
                        position.y++;

                    }

                    goto check_king ;
                case PieceVariant.Knight:
                    for (int i = -2; i <= 2; i += 4)
                    {
                        for(int j = -1; j< 2; j += 2)
                        {
                            position = new BoardPosition(this.position.x + i, this.position.y + j);
                            if (position.IsValid() && board.cellOccupants[position.x, position.y] != color)
                            {
                                possibleMoves.Add(position);
                            }
                            position = new BoardPosition(this.position.x + j, this.position.y + i);
                            if (position.IsValid() && board.cellOccupants[position.x, position.y] != color)
                            {
                                possibleMoves.Add(position);
                            }
                        }
                    }
                    goto check_king;

                case PieceVariant.King:
                    int x = this.position.x, y = this.position.y;
                    
                    for (int i = -1;  i <=1; ++i)
                    {
                        for (int j = -1; j <= 1; ++j)
                        {
                            if (i == 0 && j == 0) continue;
                            position = new BoardPosition(x + i, y + j);
                            if (position.IsValid() && board.cellOccupants[position.x, position.y] != color)
                            {
                                possibleMoves.Add(position);
                            }
                        }
                    }
                    if (!blocked)
                    {
                        return possibleMoves;
                    }
                    
                    // remove impossible moves;
                    foreach (Piece piece in board.pieces)
                    {
                        if (piece.color == this.color) { continue; }
                        foreach (BoardPosition move in piece.PossibleMoves(board, false))
                        {
                            possibleMoves.Remove(move);
                        }
                    }
                    break;
            }
            

            return possibleMoves;

            check_king:
            if(!blocked)
            {
                return possibleMoves;
            }
            // check if king is attacked
            Piece? king = null;
            foreach(Piece piece in board.pieces)
            {
                if (piece.color == this.color && piece.variant == PieceVariant.King)
                {
                    king = piece;
                    break;
                }
            }
            if (king == null)
            {
                return possibleMoves;
            }
            var checkCount = 0;
            BoardPosition? checkPos = null;
            foreach(Piece piece in board.pieces)
            {
                if (piece.PossibleMoves(board, false).Contains(king.position))
                {
                    ++checkCount;
                    checkPos = piece.position;
                }
            }
            if (checkCount > 1 || (checkPos != null && !possibleMoves.Contains(checkPos)))
            {
                return new();
            }
            else if (checkCount > 0 && checkPos != null) {
                return new() { checkPos };
            }

            return possibleMoves;

        }
    }
}
