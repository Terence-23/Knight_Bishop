using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Knight_Bishop
{

    public class Game
    {
        public GameType type { get; set; } = GameType.None;
        internal Mover? whiteMover, blackMover;
        public GameStatus status { get; set; } = GameStatus.Pending;

        internal Board board;
        internal PieceColor currentMove { get; set; } = PieceColor.White;
        internal int turnCount { get; set; } = 0;
        internal Stack<(List<BoardPosition>, int)> previousPositions { get; }

        public Game(GameType type, Mover? whiteMover, Mover? blackMover, Board board)
        {
            this.type = type;
            this.whiteMover = whiteMover;
            this.blackMover = blackMover;
            this.board = board;
            Init();
        }
        private void Init()
        {
            if (whiteMover == null &&  blackMover == null)
            {
                type = GameType.None;
            }
            else if (whiteMover == null && type != GameType.None)
            {
                type = GameType.Black;
            }
            else if (blackMover == null && type != GameType.None)
            {
                type = GameType.White;
            }

            if (type ==GameType.White || type == GameType.All)
            {
                NextMove();
            }
        }

        internal void NextMove()
        {
        begin:
            if (turnCount >= 50)
            {
                End(GameEndReason.FiftyMoveRule);
                return;
            }


            if (currentMove == PieceColor.White)
            {
                if (type == GameType.White || type == GameType.All)
                {
                    //calc next move
                    var (piece, pos) = whiteMover.NextMove(board, PieceColor.White);
                }
                
                currentMove = PieceColor.Black;
                if (CheckForRepetition())
                {
                    status = GameStatus.Draw;
                    End(GameEndReason.Repetition);

                    return;
                }
                // check if next move is automatic
                if (type == GameType.White ||  type == GameType.None) { return; }
            }
            else
            {
                if (type == GameType.Black || type == GameType.All)
                {
                    //calc next move
                    var (piece, pos) = blackMover.NextMove(board, PieceColor.Black);
                }


                ++turnCount;
                currentMove = PieceColor.White;
                if (CheckForRepetition())
                {
                    status = GameStatus.Draw;
                    End(GameEndReason.Repetition);
                    
                    return;
                }
                // check if next move is automatic
                if (type == GameType.Black || type == GameType.None) { return; }
            }
            
            goto begin;
        }

        internal bool CheckForRepetition()
        {
            List<BoardPosition> positions = new List<BoardPosition>();
            foreach (Piece piece in board.pieces)
            {
                positions.Add(piece.position);
            }

            foreach (var (list, num) in previousPositions)
            {
                if (list.Equals(positions))
                {
                    if (num + 1 >= 3)
                    { 
                        return true;
                    }
                    else
                    {
                        previousPositions.Push((positions, num + 1));
                        break;
                    }
                }
            }
            return false;
        }

        internal void End(GameEndReason reason)
        {

        }
    }
}
