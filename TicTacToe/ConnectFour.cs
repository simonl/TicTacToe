using System;
using System.Collections.Generic;
using System.Linq;

namespace TicTacToe
{
    public enum ConnectIndices
    {
        One, Two, Three, Four, Five, Six, Seven,
    }

    public sealed class ConnectFour : Game<ConnectIndices>
    {
        public override IEnumerable<Transform> Basis
        {
            get
            {
                return new Transform[]
                {
                    new Transform(true, false, 0),
                    new Transform(false, true, 0),
                };
            }
        }

        public override IDomain<Players, IBoard<ConnectIndices>, Position<ConnectIndices>> Dynamic
        {
            get
            {
                var domain = Domains.Board<ConnectIndices>();

                return new Domain<Players, IBoard<ConnectIndices>, Position<ConnectIndices>>(
                    eventsF: (board, position) => Winner(board) == null && domain.Events(board, position),
                    createF: player => domain.Create(player),
                    updateF: (board, position) => Play(board, position));
            }
        }

        public override IDiscrete<Players, IBoard<ConnectIndices>, Position<ConnectIndices>> Links
        {
            get
            {
                var transitions = Dynamic.Transitions();

                return new Discrete<Players, IBoard<ConnectIndices>, Position<ConnectIndices>>(
                    selectors: transitions.Selectors,
                    eventsF: board => Moves(board).ToArray());
            }
        }

        public IEnumerable<Position<ConnectIndices>> Moves(IBoard<ConnectIndices> board)
        {
            if (Winner(board) != null)
            {
                yield break;
            }

            var indices = Morphisms.Indices<ConnectIndices>();

            foreach (var index in indices)
            {
                if (board[new Position<ConnectIndices> { X = index, Y = ConnectIndices.Six }] == null)
                {
                    yield return new Position<ConnectIndices> { X = index, Y = ConnectIndices.Seven };
                }
            }
        }

        public IBoard<ConnectIndices> Play(IBoard<ConnectIndices> board, Position<ConnectIndices> position)
        {
            if (position.Y != ConnectIndices.Seven)
            {
                throw new InvalidOperationException();
            }

            var indices = Morphisms.Indices<ConnectIndices>();

            foreach (var height in indices)
            {
                if (height == ConnectIndices.Seven)
                {
                    continue;
                }

                var target = new Position<ConnectIndices> { X = position.X, Y = height };

                if (board[target] == null)
                {
                    return new Board<ConnectIndices>(
                        turn: Program.Opposite(board.Turn),
                        fetchF: position =>
                        {
                            if (position.Equals(target))
                            {
                                return board.Turn;
                            }

                            return board[position];
                        }).Copy();
                }
            }

            throw new InvalidOperationException();
        }

        private Position<ConnectIndices>[][] LinesCache = null;

        public override Players? Winning(IBoard<ConnectIndices> board)
        {
            var lines = LinesCache;
            if (LinesCache == null)
            {
                lines = LinesCache = Lines<ConnectIndices>().Select(_ => _.ToArray()).ToArray();
            }

            foreach (var line in lines)
            {
                var score = line.Sum(position => Program.Score(board[position]));

                if (score == -4)
                {
                    return Players.Cross;
                }

                if (score == +4)
                {
                    return Players.Cross;
                }
            }

            foreach (var position in Moves(board))
            {
                return null;
            }

            return 0;
        }

        public static IEnumerable<IEnumerable<Position<I>>> Lines<I>()
        {
            foreach (var position in Morphisms.Positions<I>())
            {
                yield return position.Line(new Arrow { X = Arrows.Zero, Y = Arrows.Increment }).Take(4);
                yield return position.Line(new Arrow { X = Arrows.Increment, Y = Arrows.Increment }).Take(4);
                yield return position.Line(new Arrow { X = Arrows.Increment, Y = Arrows.Zero }).Take(4);
                yield return position.Line(new Arrow { X = Arrows.Increment, Y = Arrows.Decrement }).Take(4);
            }
        }
    }
}
