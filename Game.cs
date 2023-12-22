using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Knight_Bishop
{

    public class Game
    {
        public GameType type { get; set; } = GameType.None;
        internal Mover? whiteMover, blackMover;
        private PictureBox pictureBox;
        private TextBox textBox;
        public GameStatus status { get; set; } = GameStatus.Pending;

        internal Board board;
        internal PieceColor currentMove { get; set; } = PieceColor.White;
        internal int turnCount { get; set; } = 0;
        internal Stack<(List<BoardPosition>, int)> previousPositions { get; }

        public Game(GameType type, Mover? whiteMover, Mover? blackMover, Board board, PictureBox pictureBox, TextBox textBox)
        {
            this.type = type;
            this.whiteMover = whiteMover;
            this.blackMover = blackMover;
            this.board = board;
            previousPositions = new Stack<(List<BoardPosition>, int)>();
            this.pictureBox = pictureBox;
            this.textBox = textBox;

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

            if (type == GameType.White || type == GameType.All)
            {
                NextMove();
            }
        }

        internal void NextMove()
        {
            if (status != GameStatus.Pending)
            {
                return;
            }
            while (turnCount < 50)
            {
                if (currentMove == PieceColor.White)
                {
                    if (type == GameType.White || type == GameType.All)
                    {
                        //calc next move
                        if (whiteMover == null) throw new InvalidDataException();
                        var (piece, pos) = whiteMover.NextMove(board, PieceColor.White);
                        if (piece == null) { Stalemate(); return; }
                        board.UncheckedMove(piece, pos);
                        textBox.Text += $"{turnCount + 1}. {(char)piece.variant}{pos} ";

                    }


                    currentMove = PieceColor.Black;
                    Stalemate();
                    if (CheckForRepetition()) { Repetition(); return; }
                    // check if next move is automatic
                    if (type == GameType.White || type == GameType.None) { return; }
                }
                else
                {
                    if (type == GameType.Black || type == GameType.All)
                    {
                        //calc next move
                        if (blackMover == null) throw new InvalidDataException();
                        var (piece, pos) = blackMover.NextMove(board, PieceColor.Black);
                        if (piece == null) { Stalemate(); return; }
                        board.UncheckedMove(piece, pos);
                        textBox.Text += $"{(char)piece.variant}{pos}" + Environment.NewLine;
                    }

                    ++turnCount;
                    currentMove = PieceColor.White;

                    Stalemate();
                    if (CheckForRepetition()) { Repetition(); return; }
                    // check if next move is automatic
                    if (type == GameType.Black || type == GameType.None) { return; }
                }

                
                textBox.Refresh();
                pictureBox.Refresh();

                Thread.Sleep(30);
            }

            End(GameEndReason.FiftyMoveRule);
            return;
        }
        
        void Repetition()
        {
            status = GameStatus.Draw;
            End(GameEndReason.Repetition);
            
        }
        void Stalemate()
        {
            Piece? king = null;
            foreach (Piece piece in board.pieces)
            {
                if (piece.color == currentMove && piece.variant == PieceVariant.King)
                {
                    king = piece;
                    break;
                }
            }
            if (king == null)
            {
                status = GameStatus.Draw;
                End(GameEndReason.Stalemate);
                return;
            }
            
            // check if a move is possible
            foreach(Piece piece in board.pieces)
            {
                if (piece.color == currentMove && piece.PossibleMoves(board, true).Count() > 0)
                {
                    return;
                }
            }
            
            var (count, _) = Piece.IsCheck(board, king);
            if (count > 0)
            {
                status =
                    currentMove == PieceColor.White ?
                    GameStatus.BlackWin :
                    GameStatus.WhiteWin
                    ;
                End(GameEndReason.CheckMate);
                return;
            }
            else
            {
                status = GameStatus.Draw;
                End(GameEndReason.Stalemate);
                return;
            }
        }

        internal bool CheckForRepetition()
        { 
            List<BoardPosition> positions = new();
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
            Console.WriteLine(reason.ToString());
            Debug.WriteLine(reason.ToString());
            textBox.Text += 
                Environment.NewLine + Environment.NewLine + 
                $"Game Ended: {reason}" 
                + Environment.NewLine + Environment.NewLine;


        }
    }
}
