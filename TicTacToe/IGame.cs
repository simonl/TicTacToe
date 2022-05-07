using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TicTacToe
{
    public interface IGame<I>
    {
        IEnumerable<Transform> Symmetries { get; }
        Players? Winner(IBoard<I> board);
        IDomain<Players, IBoard<I>, Position<I>> Dynamic { get; }
        IDiscrete<Players, IBoard<I>, Position<I>> Links { get; }
    }

    public abstract class Game<I> : IGame<I>
    {
        public abstract IEnumerable<Transform> Basis { get; }

        private IEnumerable<Transform> CachedTransforms = null;

        public IEnumerable<Transform> Symmetries
        {
            get
            {
                if (CachedTransforms != null)
                {
                    return CachedTransforms;
                }

                var basis = Basis.ToArray();

                return CachedTransforms = Transforms.Group.Closure(basis);
            }
        }

        private Players? WinnerCache = null;
        private IBoard<I> BoardCache = null;

        public Players? Winner(IBoard<I> board)
        {
            if (board == BoardCache)
            {
                return WinnerCache;
            }

            return WinnerCache = Winning(BoardCache = board);
        }

        public abstract Players? Winning(IBoard<I> board);
        public abstract IDomain<Players, IBoard<I>, Position<I>> Dynamic { get; }
        public abstract IDiscrete<Players, IBoard<I>, Position<I>> Links { get; }
    }


    public enum Players
    {
        Cross,
        Circle,
    }

    public interface IBoard<I> : IComparable<IBoard<I>>
    {
        Players Turn { get; }
        Players? this[Position<I> position] { get; }
    }

    public sealed class Board<I> : IBoard<I>, IEquatable<IBoard<I>>
    {
        private readonly Func<Position<I>, Players?> FetchF;

        public Board(Players turn, Func<Position<I>, Players?> fetchF)
        {
            this.Turn = turn;
            this.FetchF = fetchF;
        }

        public Players Turn { get; private set; }

        public Players? this[Position<I> position]
        {
            get { return this.FetchF(position); }
        }

        public override bool Equals(object obj)
        {
            return obj is Board<I> board && Equals(board);
        }

        public bool Equals(IBoard<I> other)
        {
            return this.Compress() == other.Compress();
        }

        public override int GetHashCode()
        {
            return this.Compress().GetHashCode();
        }

        public int CompareTo(IBoard<I> other)
        {
            return this.Compress().CompareTo(other.Compress());
        }

        public override string ToString()
        {
            //return this.Compress().ToString();

            var builder = new StringBuilder();

            builder.Append("Turn: " + (this.Turn == Players.Circle ? 'O' : 'X'));
            builder.AppendLine();
            builder.Append("-------");
            builder.AppendLine();

            var indices = Morphisms.Indices<I>();

            foreach (var row in indices)
            {
                builder.Append("|");

                foreach (var column in indices)
                {
                    var cell = this[new Position<I>() { X = column, Y = row }];

                    builder.Append(cell == null ? ' ' : cell == Players.Circle ? 'O' : 'X');
                    builder.Append('|');
                }

                builder.AppendLine();
                builder.Append("-");

                foreach (var column in indices)
                {
                    builder.Append("--");
                }

                builder.AppendLine();
            }

            return builder.ToString();
        }
    }
}
