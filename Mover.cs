using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using static System.Formats.Asn1.AsnWriter;

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

            return (null, new(-1, -1));
        }
    }

    public class CenterMover : Mover
    {
        public (Piece?, BoardPosition) NextMove(Board board, PieceColor color)
        {
            foreach (var piece in board.pieces)
            {
                if (color == piece.color
                    && piece.variant == PieceVariant.King)
                {
                    var move = piece.PossibleMoves(board, true).MinBy(x => centerDistance(x));
                    if (move != null)
                    {
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
            var (piece, pos, _) = _NextMove(board, color, depth);

            return (piece, pos);
        }

        public (Piece?, BoardPosition, int rating) _NextMove(Board board, PieceColor color, int depth)
        {
            //possible moves
            Piece? king = null;
            List<(Piece, BoardPosition)> possibleMoves = new();
            foreach (Piece piece in board.pieces)
            {
                if (piece.color != color)
                {
                    continue;
                }
                if (piece.variant == PieceVariant.King) { king = piece; }
                var piecePossible = piece.PossibleMoves(board, true);
                possibleMoves.EnsureCapacity(possibleMoves.Count + piecePossible.Count);
                foreach (BoardPosition pos in piecePossible)
                {

                    possibleMoves.Add((piece, pos));
                }
            }
            // check for mate and stalemate
            if (possibleMoves.Count == 0 && king != null && Piece.IsCheck(board, king).Item1 > 0)
            {
                // checkmate
                return (null, new(-1, -1), color == PieceColor.White ? -1000 : 1000);
            }
            else if (possibleMoves.Count == 0)
            {
                // stalemate
                return (null, new(-1, -1), 0);
            }

            // recursion end, 
            if (depth == 0)
            {
                int black = 0, white = 0;
                foreach (Piece piece in board.pieces)
                {
                    if (piece.variant == PieceVariant.King)
                    {
                        continue;
                    }
                    if (piece.color == PieceColor.White)
                    {
                        white += 3;
                    }
                    else
                    {
                        black += 3;
                    }
                }
                return (null, new(-1, -1), white - black);

            }

            if (color != selfColor)
            {
                // run foregin Mover
                var (piece, move) = other.NextMove(board, color);
                if (piece == null)
                {
                    return (piece, move, 0);
                }
                var pos = piece.position;
                board.UncheckedMove(piece, move);
                var ret_val = _NextMove(board, selfColor, depth - 1);
                board.UnMove();
                return ret_val;
            }
            // rate each one and calc best
            List<(Piece, BoardPosition, int)> moves = new();
            if (color == PieceColor.White)
            {
                foreach (var (piece, pos) in possibleMoves)
                {
                    var start_pos = piece.position;
                    board.UncheckedMove(piece, pos);
                    var (_, _, rating) = _NextMove(board, PieceColor.Black, depth - 1);
                    moves.Add((piece, pos, rating));
                    board.UnMove();
                }

                var move = moves.MaxBy<(Piece, BoardPosition, int), int>(
                    //Func<(Piece, BoardPosition, int), int>
                    (tuple) => tuple.Item3);
                return move;
            }
            else
            {
                foreach (var (piece, pos) in possibleMoves)
                {
                    var start_pos = piece.position;
                    board.UncheckedMove(piece, pos);
                    var (_, _, rating) = _NextMove(board, PieceColor.White, depth - 1);
                    moves.Add((piece, pos, rating));
                    board.UnMove();
                }

                var move = moves.MinBy<(Piece, BoardPosition, int), int>(
                    //Func<(Piece, BoardPosition, int), int>
                    (tuple) => tuple.Item3);
                return move;
            }

        }
    }

    public class UnAwareMinMaxMover : Mover
    {
        int depth = 2;
        int checkedMoves = 0;
        static readonly int mate = 1_000_000;

        int alfa = -mate; // minimum possible
        int beta = mate; // maximum possible 

        public UnAwareMinMaxMover(int depth)
        {
            this.depth = depth;
        }

        public (Piece?, BoardPosition) NextMove(Board board, PieceColor color)
        {
            checkedMoves = 0;
            var (piece, pos, rating) = color == PieceColor.White? 
                alphaBetaMax(board, alfa, beta, depth) : 
                alphaBetaMin(board, alfa, beta, depth);

            Console.WriteLine($"Positon rating: {rating}");

            return (piece, pos);
        }

        (Piece?, BoardPosition, int) alphaBetaMax(Board board, int alpha, int beta, int depth)
        {

            //possible moves
            Piece? king = null;
            List<(Piece, BoardPosition)> possibleMoves = new();
            foreach (Piece piece in board.pieces)
            {
                if (piece.color != PieceColor.White)
                {
                    continue;
                }
                if (piece.variant == PieceVariant.King) { king = piece; }
                var piecePossible = piece.PossibleMoves(board, true);
                possibleMoves.EnsureCapacity(possibleMoves.Count + piecePossible.Count);
                foreach (BoardPosition pos in piecePossible)
                {

                    possibleMoves.Add((piece, pos));
                }
            }

            // check for mate and stalemate
            if (possibleMoves.Count == 0 && king != null && Piece.IsCheckNow(board, king).Item1 > 0)
            {
                
                //Console.WriteLine(Piece.IsCheckNow(board, king).Item1);
                // checkmate
                return (null, new(-1, -1), -mate);
            }
            else if (possibleMoves.Count == 0)
            {
                // stalemate
                return (null, new(-1, -1), 0);
            }

            // recursion end, 
            if (depth == 0)
            {
                int black = 0, white = 0;
                foreach (Piece piece in board.pieces)
                {
                    if (piece.variant == PieceVariant.King)
                    {
                        continue;
                    }
                    if (piece.color == PieceColor.White)
                    {
                        white += 3;
                    }
                    else
                    {
                        black += 3;
                    }
                }
                return (null, new(-1, -1), (white - black) * 100);

            }
            var alfa_move = (possibleMoves[0].Item1, possibleMoves[0].Item2, alpha);
            foreach (var (piece, where) in possibleMoves)
            {
                board.UncheckedMove(piece, where);
                var (_, _, rating) = alphaBetaMin(board, alfa_move.Item3, beta, depth - 1);
                board.UnMove();
                if (rating > 0)
                {
                    rating--;
                }
                else if (rating < 0)
                {
                    rating++;
                }
                if (rating >= beta)
                    return (null, new(-1, -1), beta); // fail hard beta-cutoff
                if (rating > alfa_move.Item3)
                    alfa_move = (piece, where, rating); // alfa acts like max in MiniMax
            }
            checkedMoves++;
            Console.WriteLine(alfa_move.Item3);
            Console.WriteLine(checkedMoves);
            return alfa_move;
        }

        (Piece?, BoardPosition, int) alphaBetaMin(Board board, int alpha, int beta, int depth)
        {
            //possible moves
            Piece? king = null;
            List<(Piece, BoardPosition)> possibleMoves = new();
            foreach (Piece piece in board.pieces)
            {
                if (piece.color != PieceColor.Black)
                {
                    continue;
                }
                if (piece.variant == PieceVariant.King) { king = piece; }
                var piecePossible = piece.PossibleMoves(board, true);
                possibleMoves.EnsureCapacity(possibleMoves.Count + piecePossible.Count);
                foreach (BoardPosition pos in piecePossible)
                {

                    possibleMoves.Add((piece, pos));
                }
            }

            // check for mate and stalemate
            if (possibleMoves.Count == 0 && king != null && Piece.IsCheckNow(board, king).Item1 > 0)
            {
                if (board.cellOccupants[4, 3] != null)
                {
                    Console.WriteLine("something");

                }
                Console.WriteLine(Piece.IsCheckNow(board, king).Item1);
                // checkmate
                return (null, new(-1, -1), mate);
            }
            else if (possibleMoves.Count == 0)
            {
                // stalemate
                return (null, new(-1, -1), 0);
            }

            // recursion end, 
            if (this.depth == 0)
            {
                int black = 0, white = 0;
                foreach (Piece piece in board.pieces)
                {
                    if (piece.variant == PieceVariant.King)
                    {
                        continue;
                    }
                    if (piece.color == PieceColor.White)
                    {
                        white += 3;
                    }
                    else
                    {
                        black += 3;
                    }
                }
                return (null, new(-1, -1), (white - black) * 100);

            }
            var beta_move = (possibleMoves[0].Item1, possibleMoves[0].Item2, beta);
            foreach (var (piece, where) in possibleMoves)
            {
                board.UncheckedMove(piece, where);
                var (_, _, rating) = alphaBetaMax(board, alpha, beta_move.Item3, depth - 1);
                board.UnMove();
                if (rating > 0)
                {
                    rating--;
                }
                else if (rating < 0)
                {
                    rating++;
                }
                if (rating <= alpha)
                    return (null, new(-1, -1), alpha); // fail hard alpha-cutoff
                if (rating < beta_move.Item3)
                    beta_move = (piece, where, rating); // beta acts like min in MiniMax
            }
            Console.WriteLine(beta_move.Item3);
            return beta_move;
        }

        public (Piece?, BoardPosition, int rating) _NextMove(Board board, PieceColor color, int depth)
        {

            //possible moves
            Piece? king = null;
            List<(Piece, BoardPosition)> possibleMoves = new();
            foreach (Piece piece in board.pieces)
            {
                if (piece.color != color)
                {
                    continue;
                }
                if (piece.variant == PieceVariant.King) { king = piece; }
                var piecePossible = piece.PossibleMoves(board, true);
                possibleMoves.EnsureCapacity(possibleMoves.Count + piecePossible.Count);
                foreach (BoardPosition pos in piecePossible)
                {

                    possibleMoves.Add((piece, pos));
                }
            }

            // check for mate and stalemate
            if (possibleMoves.Count == 0 && king != null && Piece.IsCheckNow(board, king).Item1 > 0)
            {
                if (board.cellOccupants[4, 3] != null)
                {
                    Console.WriteLine("something");

                }
                Console.WriteLine(Piece.IsCheckNow(board, king).Item1);
                // checkmate
                return (null, new(-1, -1), color == PieceColor.White ? -mate : mate);
            }
            else if (possibleMoves.Count == 0)
            {
                // stalemate
                return (null, new(-1, -1), 0);
            }

            // recursion end, 
            if (depth == 0)
            {
                int black = 0, white = 0;
                foreach (Piece piece in board.pieces)
                {
                    if (piece.variant == PieceVariant.King)
                    {
                        continue;
                    }
                    if (piece.color == PieceColor.White)
                    {
                        white += 3;
                    }
                    else
                    {
                        black += 3;
                    }
                }
                return (null, new(-1, -1), (white - black) * 100);

            }
            // rate
            List<(Piece, BoardPosition, int)> ratedMoves = new(possibleMoves.Count);
            foreach (var (piece, where) in possibleMoves)
            {
                checkedMoves++;
                Move move = board.UncheckedMove(piece, where);
                //Debug.Assert(move == board.previousMoves.First());
                //Console.WriteLine(depth);
                var (what, where2, rating) = _NextMove(board, color == PieceColor.White ? PieceColor.Black : PieceColor.White, depth - 1);
                if (rating > 0)
                {
                    rating--;
                }
                else if (rating < 0)
                {
                    rating++;
                }
                ratedMoves.Add((piece, where, rating));
                //Console.WriteLine(depth);
                //Debug.Assert(move == board.previousMoves.First());
                board.UnMove();
                if ((rating == mate && color == PieceColor.White)
                    || (rating == -mate && color == PieceColor.Black))
                {
                    Console.WriteLine(rating);
                    break;
                }
            }
            Console.WriteLine(checkedMoves);

            var best = ratedMoves.MaxBy(
                tuple =>
                color == PieceColor.White ?
                    tuple.Item3 :
                    -tuple.Item3
                );

            Console.WriteLine($"{best.Item1.position} to {best.Item2}, rating: {best.Item3}");

            // max
            return best;

        }
    }
    public class KBNKMover : Mover
    {

        Piece bishop, knight, ourKing, enemyKing;
        int[,] cornerDistance;
        ulong checkedMoves = 0;
        ulong maxCheckedMoves = 0;
        int depth = 0;
 
        public KBNKMover(int depth, Board board)
        {
            this.depth = depth;
            PieceColor? color = null;
            Piece? bishop = null, knight = null, ourKing = null, enemyKing = null;
            foreach ( Piece piece in board.pieces)
            {
                
                if (piece.variant == PieceVariant.Bishop && (color == piece.color || color == null))
                {
                    color = piece.color;
                    bishop = piece;
                }
                else if(piece.variant == PieceVariant.Knight && (color == piece.color || color == null))
                {
                    color = piece.color;
                    knight = piece;
                }
                else if( piece.variant == PieceVariant.King)
                {
                    if (ourKing == null)
                    {
                        ourKing = piece;
                    }
                    else
                    {
                        enemyKing = piece;
                    }
                }
            }

            if ((bishop == null) || (knight == null) || (enemyKing == null) || (ourKing == null))
            {
                throw new Exception("The board doesn't have enough pieces");
            }
            if (ourKing.color != bishop.color)
            {
                (ourKing, enemyKing) = (enemyKing, ourKing);
            }

            this.bishop = bishop;
            this.knight = knight;
            this.ourKing = ourKing;
            this.enemyKing = enemyKing;
            cornerDistance = new int[8, 8];

            if (Board.fieldColor(bishop.position) != Board.fieldColor(new(0, 0)))
            {
                for (int i = 7; i >= 0; --i)
                {
                    for (int j = 0; j < 8; j++)
                    {
                        int distance = Math.Min(i + j, 7 + 7 - j - i);
                        cornerDistance[7 - i, j] = distance;
                    }
                }
            }
            else
            {
                for (int i = 0; i < 8; ++i)
                {
                    for (int j = 0; j < 8; j++)
                    {
                        int distance = Math.Min(i + j, 7 + 7 - j - i);
                        cornerDistance[i, j] = distance;
                    }
                }

            }

            for (int i = 0; i < 8; ++i)
            {
                for(int j = 0;j < 8; j++)
                {
                    Console.Write(cornerDistance[i,j]);
                    Console.Write(' ');
                }
                Console.WriteLine("");
            }
            Console.WriteLine();
        }
       
        public (Piece?, BoardPosition) NextMove(Board board, PieceColor color)
        {
            checkedMoves = 0;
            var (piece, pos, _) = _NextMove(board, color, depth, 50);
            maxCheckedMoves = Math.Max(maxCheckedMoves, checkedMoves);
            Console.WriteLine($"checked: {checkedMoves}, max: {maxCheckedMoves}\n");
            
            return (piece, pos);
        }

        public (Piece?, BoardPosition, int rating) _NextMove(Board board, PieceColor color, int depth, int min_rating)
        {

            //possible moves
            Piece? king = null;
            List<(Piece, BoardPosition)> possibleMoves = new();
            foreach (Piece piece in board.pieces)
            {
                if (piece.color != color)
                {
                    continue;
                }
                if (piece.variant == PieceVariant.King) { king = piece; }
                var piecePossible = piece.PossibleMoves(board, true);
                possibleMoves.EnsureCapacity(possibleMoves.Count + piecePossible.Count);
                foreach (BoardPosition pos in piecePossible)
                {

                    possibleMoves.Add((piece, pos));
                }
            }

            // check for mate and stalemate
            if (possibleMoves.Count == 0 && king != null && Piece.IsCheck(board, king).Item1 > 0)
            {
                // checkmate
                return (null, new(-1, -1), color == PieceColor.White ? -1000 : 1000);
            }
            else if (possibleMoves.Count == 0)
            {
                // stalemate
                return (null, new(-1, -1), 0);
            }

            // recursion end, 
            if (depth == 0)
            {
                var pos = enemyKing.position;
                return (null, new(-1, -1), 60 - cornerDistance[pos.y, pos.x]);

            }
            // rate
            if (color == ourKing.color)
            {
                List<(Piece, BoardPosition, int)> ratedMoves = new(possibleMoves.Count);
                foreach (var (piece, where) in possibleMoves)
                {
                    checkedMoves++;
                    Move move = board.UncheckedMove(piece, where);
                    //Debug.Assert(move == board.previousMoves.First());
                    //Console.WriteLine(depth);
                    var (what, where2, rating) = _NextMove(board, enemyKing.color, depth - 1, min_rating);
                    ratedMoves.Add((piece, where, rating));
                    //Console.WriteLine(depth);
                    //Debug.Assert(move == board.previousMoves.First());
                    board.UnMove();
                    if ((rating == 1000 && color == PieceColor.White)
                        || (rating == -1000 && color == PieceColor.Black))
                    {
                        break;
                    }
                }
                //Console.WriteLine(checkedMoves);

                // max
                return ratedMoves.MaxBy(
                    tuple =>
                    color == PieceColor.White ?
                        tuple.Item3 :
                        -tuple.Item3
                    );
            }
            else
            {
                // check if can take else move away from corner
                var eKMoves = enemyKing.PossibleMoves(board, true);
                int best = 0;
                BoardPosition bestPos = eKMoves[0];
                
                foreach(var pos in eKMoves)
                {
                    var move = board.UncheckedMove(enemyKing, pos);
                    
                    if (move.status == MoveStatus.Take)
                    {
                        board.UnMove(); 
                        return (enemyKing, pos, 0);

                    }
                    var (what, where2, rating) = _NextMove(board, ourKing.color, depth - 1, min_rating);
                    if (rating < best)
                    {
                        best = rating;
                        bestPos = pos;
                    }
                    board.UnMove();
                }

                return (enemyKing, bestPos, best);
            }
            throw new NotImplementedException();
        }
    }
}