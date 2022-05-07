using System;
using System.Collections.Generic;
using System.Linq;

namespace TicTacToe
{
    public interface IMorphism<I>
    {
        Position<I> Forward(Position<I> position);
        Position<I> Backward(Position<I> position);
    }

    public sealed class Morphism<I> : IMorphism<I>
    {
        private readonly Func<Position<I>, Position<I>> ForwardF;
        private readonly Func<Position<I>, Position<I>> BackwardF;

        public Morphism(Func<Position<I>, Position<I>> forwardF, Func<Position<I>, Position<I>> backwardF)
        {
            ForwardF = forwardF;
            BackwardF = backwardF;
        }

        public Position<I> Backward(Position<I> position)
        {
            return this.BackwardF(position);
        }

        public Position<I> Forward(Position<I> position)
        {
            return this.ForwardF(position);
        }
    }
    
    public struct Position<I> : IEquatable<Position<I>>, IComparable<Position<I>>
    {
        public I X;
        public I Y;

        public override bool Equals(object obj)
        {
            return obj is Position<I> position && Equals(position);
        }

        public bool Equals(Position<I> other)
        {
            return this.Number() == other.Number();
        }

        public override int GetHashCode()
        {
            return this.Number();
        }

        public int CompareTo(Position<I> other)
        {
            return this.Number().CompareTo(other.Number());
        }

        public override string ToString()
        {
            return (int) (object) this.X + "," + (int) (object) this.Y;
        }
    }

    public enum Arrows
    {
        Decrement = -1,
        Zero = 0,
        Increment = +1,
    }

    public struct Arrow
    {
        public Arrows X;
        public Arrows Y;
    }

    public static class Morphisms
    {
        public static IEnumerable<Position<I>> Line<I>(this Position<I> position, Arrow arrow)
        {
            yield return position;

            while (Directions(position, arrow))
            {
                position = Neighbour(position, arrow);

                yield return position;
            }
        }

        public static Position<I> Neighbour<I>(Position<I> position, Arrow arrow)
        {
            if (Directions(position, arrow))
            {
                return new Position<I>
                {
                    X = (I)(object)((int)(object)position.X + (int)arrow.X),
                    Y = (I)(object)((int)(object)position.Y + (int)arrow.Y),
                };
            }

            throw new InvalidOperationException();
        }

        public static bool Directions<I>(Position<I> position, Arrow arrow)
        {
            return Directions(position.X, arrow.X) && Directions(position.Y, arrow.Y);
        }

        public static bool Directions<I>(I index, Arrows arrow)
        {
            var indices = Indices<I>();

            switch (arrow)
            {
                case Arrows.Decrement:
                    return !index.Equals(indices[0]);
                case Arrows.Zero:
                    return true;
                case Arrows.Increment:
                    return !index.Equals(indices[indices.Length - 1]);
                default:
                    throw new ArgumentException();
            }
        }

        private static readonly TacIndices[] Tac = new SortedSet<TacIndices>(Enum.GetValues(typeof(TacIndices)).Cast<TacIndices>()).ToArray();
        private static readonly ConnectIndices[] Conn = new SortedSet<ConnectIndices>(Enum.GetValues(typeof(ConnectIndices)).Cast<ConnectIndices>()).ToArray();

        public static I[] Indices<I>()
        {
            if (typeof(I) == typeof(TacIndices))
            {
                return (I[])(object)Tac;
            }

            if (typeof(I) == typeof(ConnectIndices))
            {
                return (I[])(object)Conn;
            }

            return null;
        }

        public static IEnumerable<IMorphism<I>> AllMorphisms<I>()
        {
            yield return IdentityM<I>();
            yield return Rotate<I>();
            yield return HalfTurn<I>();
            yield return ThreeQuarterTurn<I>();
            yield return MirrorH<I>();
            yield return MirrorV<I>();
            yield return MirrorD<I>();
            yield return MirrorA<I>();
        }

        public static I Flip<I>(I index)
        {
            var indices = Indices<I>();

            return (I) (object) ((indices.Length - 1 - (int) (object) index) % indices.Length);
        }

        public static IMorphism<I> MirrorH<I>()
        {
            return new Morphism<I>(MirrorH, MirrorH);
        }

        public static IMorphism<I> MirrorV<I>()
        {
            return new Morphism<I>(MirrorV, MirrorV);
        }

        public static IMorphism<I> MirrorD<I>()
        {
            return new Morphism<I>(MirrorD, MirrorD);
        }

        public static IMorphism<I> MirrorA<I>()
        {
            return ComposeM(HalfTurn<I>(), MirrorD<I>());
        }

        public static IMorphism<I> Rotate<I>()
        {
            return new Morphism<I>(Clockwise, CounterClock);
        }

        public static IMorphism<I> HalfTurn<I>()
        {
            return ComposeM(Rotate<I>(), Rotate<I>());
        }

        public static IMorphism<I> ThreeQuarterTurn<I>()
        {
            return ComposeM(HalfTurn<I>(), Rotate<I>());
        }

        public static IMorphism<I> IdentityM<I>()
        {
            return new Morphism<I>(_ => _, _ => _);
        }

        public static IMorphism<I> Inverse<I>(IMorphism<I> morphism)
        {
            return new Morphism<I>(morphism.Backward, morphism.Forward);
        }

        public static IMorphism<I> ComposeM<I>(IMorphism<I> left, IMorphism<I> right)
        {
            return new Morphism<I>(
                forwardF: position => right.Forward(left.Forward(position)),
                backwardF: position => left.Backward(right.Backward(position)));
        }

        public static Position<I> MirrorD<I>(Position<I> position)
        {
            return new Position<I> { X = position.Y, Y = position.X };
        }

        public static Position<I> MirrorA<I>(Position<I> position)
        {
            return new Position<I> { X = Flip(position.Y), Y = Flip(position.X) };
        }

        public static Position<I> MirrorH<I>(Position<I> position)
        {
            return new Position<I> { X = Flip(position.X), Y = position.Y };
        }

        public static Position<I> MirrorV<I>(Position<I> position)
        {
            return new Position<I> { X = position.X, Y = Flip(position.Y) };
        }

        public static Position<I> CounterClock<I>(Position<I> position)
        {
            return new Position<I> { X = position.Y, Y = Flip(position.X) };
        }

        public static Position<I> Clockwise<I>(Position<I> position)
        {
            return new Position<I> { X = Flip(position.Y), Y = position.X };
        }

        public static int Number<I>(this Position<I> position)
        {
            var indices = Indices<I>();

            return indices.Length * (int)(object)position.Y + (int)(object)position.X;
        }

        public static IEnumerable<Position<I>> Positions<I>()
        {
            var indices = Indices<I>();

            foreach (var i in indices)
            {
                foreach (var j in indices)
                {
                    yield return new Position<I> { X = i, Y = j };
                }
            }
        }

    }
}
