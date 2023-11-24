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

                    var position = (BoardPosition)this.position.Clone();
                    position.x--;
                    position.y--;

                    while (position.IsValid())
                    {

                        if (board.cellOccupants[position.x, position.y] == color)
                        {
                            if (!blocked) { possibleMoves.Add((BoardPosition)position.Clone()); }
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
                        position.y--;

                    }

                    position = (BoardPosition)this.position.Clone();

                    position.x--;
                    position.y++;
                    while (position.IsValid())
                    {

                        if (board.cellOccupants[position.x, position.y] == color)
                        {
                            if (!blocked) { possibleMoves.Add((BoardPosition)position.Clone()); }
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
                            if (!blocked) { possibleMoves.Add((BoardPosition)position.Clone()); }
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
                            if (!blocked) { possibleMoves.Add((BoardPosition)position.Clone()); }
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

                    goto check_king;
                case PieceVariant.Knight:
                    for (int i = -2; i <= 2; i += 4)
                    {
                        for (int j = -1; j < 2; j += 2)
                        {
                            position = new BoardPosition(this.position.x + i, this.position.y + j);
                            if (position.IsValid()
                                && (board.cellOccupants[position.x, position.y] != color
                                || !blocked))
                            {
                                possibleMoves.Add(position);
                            }
                            position = new BoardPosition(this.position.x + j, this.position.y + i);
                            if (position.IsValid()
                                && (board.cellOccupants[position.x, position.y] != color
                                || !blocked))
                            {
                                possibleMoves.Add(position);
                            }
                        }
                    }
                    goto check_king;
                case PieceVariant.King:
                    int x = this.position.x, y = this.position.y;

                    for (int i = -1; i <= 1; ++i)
                    {
                        for (int j = -1; j <= 1; ++j)
                        {
                            if (i == 0 && j == 0) continue;
                            position = new BoardPosition(x + i, y + j);
                            if (position.IsValid() && (board.cellOccupants[position.x, position.y] != color || !blocked))
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
            if (!blocked)
            {
                return possibleMoves;
            }
            // check if king is attacked
            Piece? king = null;
            foreach (Piece piece in board.pieces)
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
            int checkCount;
            BoardPosition? checkPos;
            (checkCount, checkPos) = IsCheck(board, king);
            


            if (checkCount > 1)
            {
                return new();
            }
            else if (checkCount > 0 && checkPos != null)
            {
                var moves = Interpose(checkPos, king.position);
                moves.Add(checkPos);

                return possibleMoves.Intersect(moves).ToList();
            }

            return possibleMoves;
            
        }

        public static (int, BoardPosition?) IsCheck(Board board, Piece king)
        {
            int checkCount = 0;
            BoardPosition? checkPos = null;
            foreach (Piece piece in board.pieces)
            {
                if (piece.color != king.color
                    && piece.PossibleMoves(board, false).Contains(king.position)
                )
                {
                    ++checkCount;
                    checkPos = piece.position;
                }
                else if (piece.color != king.color &&
                    (Math.Abs((piece.position - king.position).x)
                        == Math.Abs((piece.position - king.position).y))
                        && piece.variant == PieceVariant.Bishop &&
                        !IsInterposed(piece.position, king.position, board, king.color)
                       )
                {
                    ++checkCount;
                    checkPos = piece.position;
                }
            }
            return (checkCount, checkPos);
        }

        private static bool IsInterposed(BoardPosition piece, BoardPosition king, Board board, PieceColor color)
        {
            
            int pCount = 0;
            foreach (BoardPosition pos in Interpose(piece, king) )
            {
                pCount +=
                    board.cellOccupants[pos.x, pos.y] == color ?
                    1 : 0;
            }

            return pCount > 1;
        }
        internal static List<BoardPosition> Interpose(BoardPosition piece, BoardPosition king)
        {
            List<BoardPosition> result = new();
            int dirX = (king - piece).x > 0 ? 1 : -1, dirY = (king - piece).y > 0 ? 1 : -1;

            BoardPosition pos = new BoardPosition(piece.x + dirX, piece.y + dirY);
            //int pCount = 0;
            while (pos != king)
            {
                result.Add((BoardPosition)pos.Clone());
                pos += new BoardPosition(dirX, dirY);
            }
            return result;
        }
    }
}
