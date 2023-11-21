using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Knight_Bishop
{
    public interface Mover
    {
        (Piece, BoardPosition) NextMove(Board board, PieceColor color);
    }
}
