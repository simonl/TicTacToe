using System;
using System.Collections.Generic;
using System.Linq;

namespace TicTacToe
{
    public enum TacIndices
    {
        Top = 0, Left = 0,
        Middle = 1, Center = 1,
        Bottom = 2, Right = 2,
    }

    public sealed class TicTacToe : Game<TacIndices>
    {
        private readonly bool Simplify;

        public TicTacToe(bool simplify = false)
        {
            this.Simplify = simplify;
        }

        public override IEnumerable<Transform> Basis
        {
            get
            {
                return Simplify ? Transforms.BaseTransforms() : new Transform[0];
            }
        }

        public override IDomain<Players, IBoard<TacIndices>, Position<TacIndices>> Dynamic
        {
            get
            {
                var domain = Domains.Board<TacIndices>();

                return new Domain<Players, IBoard<TacIndices>, Position<TacIndices>>(
                    eventsF: (board, position) => Winner(board) == null && domain.Events(board, position),
                    createF: player => domain.Create(player),
                    updateF: (board, position) => domain.Update(board, position));
            }
        }

        public override IDiscrete<Players, IBoard<TacIndices>, Position<TacIndices>> Links
        {
            get
            {
                var transitions = Dynamic.Transitions();

                return new Discrete<Players, IBoard<TacIndices>, Position<TacIndices>>(
                    selectors: transitions.Selectors,
                    eventsF: board => Moves(board).ToArray());
            }
        }

        public IEnumerable<Position<TacIndices>> Moves(IBoard<TacIndices> board)
        {
            if (Winner(board) != null)
            {
                return new Position<TacIndices>[0];
            }

            return Domains.Board<TacIndices>().Transitions<TacIndices>().Events(board);
        }

        public override Players? Winning(IBoard<TacIndices> board)
        {
            return TicTacToeWinner(board);
        }

        private Position<TacIndices>[][] LinesCache = null;

        public Players? TicTacToeWinner(IBoard<TacIndices> board)
        {
            var lines = LinesCache;
            if (LinesCache == null)
            {
                lines = LinesCache = Lines<TacIndices>().Select(_ => _.ToArray()).ToArray();
            }

            var total = Morphisms.Indices<TacIndices>();

            foreach (var line in lines)
            {
                var score = line.Sum(position => Program.Score(board[position]));

                if (score == -total.Length)
                {
                    return Players.Cross;
                }

                if (score == +total.Length)
                {
                    return Players.Circle;
                }
            }

            return null;
        }

        public static IEnumerable<IEnumerable<Position<I>>> Lines<I>()
        {
            var indices = Morphisms.Indices<I>();

            yield return new Position<I> { X = indices[0], Y = indices[0] }.Line(new Arrow { X = Arrows.Increment, Y = Arrows.Increment });
            yield return new Position<I> { X = Morphisms.Flip(indices[0]), Y = indices[0] }.Line(new Arrow { X = Arrows.Decrement, Y = Arrows.Increment });

            foreach (var index in Morphisms.Indices<I>())
            {
                yield return new Position<I> { X = index, Y = indices[0] }.Line(new Arrow { X = 0, Y = Arrows.Increment });
                yield return new Position<I> { X = indices[0], Y = index }.Line(new Arrow { X = Arrows.Increment, Y = 0 });
            }
        }
    }
}
