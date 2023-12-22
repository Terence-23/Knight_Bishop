using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Knight_Bishop
{
    public struct Move
    {
        public MoveStatus status;
        public Piece? taken;
        public Piece moved;
        public BoardPosition from, to;

        public Move(MoveStatus status, Piece moved, BoardPosition from, BoardPosition to, Piece? taken)
        {
            this.status = status;
            this.taken = taken;
            this.moved = moved;
            this.from = from;
            this.to = to;
        }

        public override bool Equals(object? other)
        {   
            if (other == null){
                return false;
            }
            Move oth = (Move)other;
            return status == oth.status
                && taken == oth.taken
                && moved == oth.moved
                && from == oth.from
                && to == oth.to;
        }
        public static bool operator ==(Move left, Move right)
        {
            return left.status == right.status
                && left.taken == right.taken
                && left.moved == right.moved
                && left.from == right.from
                && left.to == right.to;
        }
        public static bool operator !=(Move left, Move right)
        {
            return left.status != right.status
                || left.taken != right.taken
                || left.moved != right.moved
                || left.from != right.from
                || left.to != right.to;
        }

        public override int GetHashCode()
        {
            return ((int)status) ^ (taken != null ? taken.GetHashCode() : 0) ^ moved.GetHashCode() ^ from.GetHashCode() ^ to.GetHashCode();
        }
    }

    public class Board : ICloneable
    {
        public List<Piece> pieces { get; }
        public PieceColor?[,] cellOccupants { get; }
        public int cell_width, cell_height;
        private SolidBrush lightSpace = new(Color.FromArgb(255, 150, 150, 150)), 
            darkSpace = new(Color.FromArgb(255, 80, 80, 80));
        private SolidBrush lightHighlight = new(Color.FromArgb(200, 150, 150)), 
            darkHighlight = new(Color.FromArgb(80, 80, 200));

        public Stack<Move> previousMoves;
        
        public static PieceColor fieldColor(BoardPosition pos)
        {
            int p = (pos.x ^ pos.y) & 1;
            if (p == 0)
            {
                return PieceColor.White;
            }
            else
            {
                return PieceColor.Black;
            }
        }

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
            previousMoves = new Stack<Move>();
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
                    var pos = new BoardPosition(i, j);
                    g.FillRectangle(fieldColor(pos) == PieceColor.Black ? darkSpace : lightSpace, 
                        i * cell_width, j * cell_height, 
                        cell_width, cell_height);
                    
                    //g.DrawString(
                    //    pos.ToString(),
                    //    new Font("Arial", 9),
                    //    Brushes.Yellow,
                    //    (float)(pos.x + 0.0) * cell_width,
                    //    (float)(pos.y + 0.0) * cell_height);

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
    
        public bool Equals(Board other)
        {
            if (other.pieces.Count != pieces.Count) return false;
            foreach (var piece in pieces)
            {
                if (piece != null && other.pieces.Find((p)=> p.IsEqual(piece) ) == null) {
                    Debug.WriteLine("Not all pieces are good");
                    return false;
                }
            }

            for (int i = 0; i < 8; ++i)
            {
                for (int j = 0; j < 8; ++j)
                {
                    if (cellOccupants[i, j] != other.cellOccupants[i, j]) {

                        Debug.WriteLine("cells are different"); 
                        return false; 
                    }
                }
            }
            return true;
        }

        public Move ManualMove(Piece piece, BoardPosition newPos)
        {
            var possibleMoves = piece.PossibleMoves(this, true);

            if (possibleMoves.Contains(newPos))
            {
                return UncheckedMove(piece, newPos);
            }
            return new Move(MoveStatus.Failed, piece, piece.position, newPos, null);
        }
        public Move UncheckedMove(Piece piece, BoardPosition newPos)
        {
            var status = MoveStatus.Move;
            BoardPosition from = piece.position;
            Piece? taken = null;
            cellOccupants[piece.position.x, piece.position.y] = null;
            if (cellOccupants[newPos.x, newPos.y] != null)
            {
                foreach(Piece piece1 in pieces)
                {
                    if (piece1.position == newPos)
                    {
                        taken = piece1;
                        break;
                    }
                }
                Debug.Assert(taken != null);
                pieces.RemoveAll((Piece piece) => piece.position == newPos);
                status = MoveStatus.Take;
                Debug.Assert(taken != null);
            }
            cellOccupants[newPos.x, newPos.y] = piece.color;
            piece.position = newPos;

            Move move = new Move(status, piece, from, newPos, taken);
            previousMoves.Push(move);

            return move;
        }

        public void UnMove()
        {
            Move move = previousMoves.Pop();
            //Console.WriteLine($"{move.status}, from: {move.from}, to: {move.to}, who: {move.moved.color} {move.moved.variant}");

            Debug.Assert(UncheckedMove(move.moved, move.from).status != MoveStatus.Failed);
            previousMoves.Pop();
            
            if (move.status == MoveStatus.Take )
            {
                Debug.Assert(move.taken != null);
                cellOccupants[move.to.x, move.to.y] = move.taken.color;
                pieces.Add(move.taken);
            }


        }

        public object Clone() => new Board(
            pieces.ConvertAll(x => (Piece)x.Clone()), 
            cell_width, 
            cell_height
            );

    }
}
