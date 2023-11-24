using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Knight_Bishop
{
    public interface Mover
    {
        (Piece?, BoardPosition) NextMove(Board board, PieceColor color);
    }

    public class NaiveMover : Mover
    {
        public (Piece?, BoardPosition) NextMove(Board board, PieceColor color)
        {
            foreach (var piece in board.pieces)
            {
                if (color == piece.color)
                {
                    var possibleMoves = piece.PossibleMoves(board, true);
                    if (possibleMoves.Count > 0)
                    {
                        return (piece, possibleMoves.First());
                    }
                }
            }

            return (null, new (-1, -1));
        }
    }

    public class CenterMover : Mover
    {
        public (Piece?, BoardPosition) NextMove(Board board, PieceColor color)
        {
            foreach(var piece in board.pieces)
            {
                if (color == piece.color 
                    && piece.variant == PieceVariant.King)
                {
                    var move = piece.PossibleMoves(board, true).MinBy(x => centerDistance(x));
                    if (move != null) {
                        return (piece, move);
                    }
                }
            }
            return new NaiveMover().NextMove(board, color);

        }
        private static int centerDistance(BoardPosition pos)
        {
            var (x, y) = pos - new BoardPosition(4, 4);
            return Math.Max(Math.Abs((int)x), Math.Abs((int)y));
        }
    }

    public class AwareMinMaxMover : Mover
    {
        Mover other;
        int depth = 50;
        private PieceColor selfColor;

        public AwareMinMaxMover(Mover other, int depth)
        {
            //DOnt use a MinMax mover as the other mover. use UnAwareMover for both instead.
            this.other = other;
            this.depth = depth;
        }

        public (Piece?, BoardPosition) NextMove(Board board, PieceColor color)
        {
            var (piece, pos, rating) = _NextMove(board, color, depth);

            return (piece, pos);
        }

        public (Piece?, BoardPosition, int rating) _NextMove(Board board, PieceColor color, int depth)
        {
            if (color != selfColor)
            {
                // run foregin Mover
                var (piece, move) = other.NextMove(board, color);
                var pos = piece.position;
                board.UncheckedMove(piece, move);
                _NextMove(board, selfColor, depth-1);
                board.UncheckedMove(piece, pos);
            }
            

            throw new NotImplementedException();
        }
    }

    public class UnAwareMinMaxMover : Mover
    {
        int depth = 50;

        public UnAwareMinMaxMover(int depth)
        {
            this.depth = depth;
        }

        public (Piece?, BoardPosition) NextMove(Board board, PieceColor color)
        {
            var (piece, pos, rating) = _NextMove(board, color, depth);

            return (piece, pos);
        }

        public (Piece?, BoardPosition, int rating) _NextMove(Board board, PieceColor color, int depth)
        {

            throw new NotImplementedException();
        }
    }


}
