using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Knight_Bishop
{
    public  class BoardPosition: ICloneable, IEquatable<BoardPosition?>
    {
        public int x { get; set; }
        public int y { get; set; }
        public bool IsValid() { return x >= 0 && x < 8 && y >= 0 && y < 8; }

        public object Clone() => new BoardPosition(x, y);

        public override bool Equals(object? obj)
        {
            return Equals(obj as BoardPosition);
        }

        public bool Equals(BoardPosition? other)
        {
            return other is not null &&
                   x == other.x &&
                   y == other.y;
        }

        public BoardPosition(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public static bool operator ==(BoardPosition? left, BoardPosition? right)
        {
            return EqualityComparer<BoardPosition>.Default.Equals(left, right);
        }

        public static bool operator !=(BoardPosition? left, BoardPosition? right)
        {
            return !(left == right);
        }
        public override int GetHashCode()
        {
            return x ^ y;
        }

        internal void Deconstruct(out object x, out object y)
        {
            x = this.x;
            y = this.y;
        }
    }
}
