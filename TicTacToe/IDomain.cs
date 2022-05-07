using System;
using System.Collections.Generic;
using System.Linq;

namespace TicTacToe
{
    public sealed class Unit
    {
        public readonly Unit Singleton = new Unit();

        private Unit() { }
    }

    public interface IDomain<in S, T, in E>
    {
        bool Events(T state, E @event);

        T Create(S selector);
        T Update(T state, E @event);
    }

    public sealed class Domain<S, T, E> : IDomain<S, T, E>
    {
        private readonly Func<T, E, bool> EventsF;
        private readonly Func<S, T> CreateF;
        private readonly Func<T, E, T> UpdateF;

        public Domain(Func<T, E, bool> eventsF, Func<S, T> createF, Func<T, E, T> updateF)
        {
            EventsF = eventsF;
            CreateF = createF;
            UpdateF = updateF;
        }

        public bool Events(T state, E @event)
        {
            return this.EventsF(state, @event);
        }

        public T Create(S selector)
        {
            return this.CreateF(selector);
        }

        public T Update(T state, E @event)
        {
            return this.UpdateF(state, @event);
        }
    }

    public interface IDiscrete<out S, in T, out E>
    {
        S[] Selectors { get; }
        E[] Events(T state);
    }

    public sealed class Discrete<S, T, E> : IDiscrete<S, T, E>
    {
        private readonly Func<T, E[]> EventsF;

        public Discrete(S[] selectors, Func<T, E[]> eventsF)
        {
            Selectors = selectors;
            EventsF = eventsF;
        }

        public S[] Selectors { get; private set; }

        public E[] Events(T state)
        {
            return this.EventsF(state);
        }
    }

    public static class Domains
    {
        public static IDiscrete<Players, IBoard<I>, Position<I>> Transitions<I>(this IDomain<Players, IBoard<I>, Position<I>> domain)
        {
            return new Discrete<Players, IBoard<I>, Position<I>>(
                selectors: new[] { Players.Cross, Players.Circle },
                eventsF: board =>
                {
                    return Morphisms.Positions<I>().Where(position => domain.Events(board, position)).ToArray();
                });
        }

        public static IDomain<Players, IBoard<I>, Position<I>> Board<I>()
        {
            return new Domain<Players, IBoard<I>, Position<I>>(
                @eventsF: (board, position) =>
                {
                    return board[position] == null;
                },
                createF: first =>
                {
                    return new Board<I>(first, fetchF: _ => null);
                },
                updateF: (board, target) =>
                {
                    if (board[target] != null)
                    {
                        throw new InvalidOperationException("Board position is already occupied: " + target);
                    }

                    return new Board<I>(
                        turn: Program.Opposite(board.Turn),
                        fetchF: position =>
                        {
                            if (position.Equals(target))
                            {
                                return board.Turn;
                            }

                            return board[position];
                        }).Copy();
                });
        }
    }
}
