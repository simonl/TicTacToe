using System;
using System.Collections.Generic;

namespace TicTacToe
{
    public interface IGroup<G>
    {
        G Identity { get; }
        G Inverse(G element);
        G Compose(G left, G right);
    }

    public sealed class Group<G> : IGroup<G>
    {
        private readonly Func<G, G> InverseF;
        private readonly Func<G, G, G> ComposeF;

        public Group(G identity, Func<G, G> inverseF, Func<G, G, G> composeF)
        {
            this.Identity = identity;
            this.InverseF = inverseF;
            this.ComposeF = composeF;
        }

        public G Identity { get; private set; }

        public G Inverse(G element)
        {
            return this.InverseF(element);
        }

        public G Compose(G left, G right)
        {
            return this.ComposeF(left, right);
        }
    }

    public interface IRelation<S>
    {
        S Forward(S element);
        S Backward(S element);
    }

    public sealed class Relation<S> : IRelation<S>
    {
        private readonly Func<S, S> ForwardF;
        private readonly Func<S, S> BackwardF;

        public Relation(Func<S, S> forwardF, Func<S, S> backwardF)
        {
            this.ForwardF = forwardF;
            this.BackwardF = backwardF;
        }

        public S Forward(S element)
        {
            return this.ForwardF(element);
        }

        public S Backward(S element)
        {
            return this.BackwardF(element);
        }
    }

    public static class Groups
    {
        public static IGroup<IRelation<S>> Relations<S>()
        {
            return new Group<IRelation<S>>(
                identity: new Relation<S>(_ => _, _ => _),
                inverseF: relation => new Relation<S>(relation.Backward, relation.Forward),
                composeF: (left, right) => new Relation<S>(e => right.Forward(left.Forward(e)), e => left.Backward(right.Backward(e))));
        }

        public static IEnumerable<G> Closure<G>(this IGroup<G> group, params G[] basis)
        {
            var transforms = new HashSet<G>();

            var queue = new Queue<G>(new[] { group.Identity });

            while (queue.Count != 0)
            {
                var element = queue.Dequeue();

                if (transforms.Contains(element))
                {
                    continue;
                }

                transforms.Add(element);

                foreach (var fragment in basis)
                {
                    queue.Enqueue(group.Compose(element, fragment));
                    queue.Enqueue(group.Compose(element, group.Inverse(fragment)));
                }
            }

            return transforms;
        }

        public static void CheckTransforms<G>(this IGroup<G> group, params G[] basis)
        {
            var transforms = group.Closure(basis);

            foreach (var transform in transforms)
            {
                if (!group.Compose(group.Identity, transform).Equals(transform))
                {
                    throw new ArgumentOutOfRangeException();
                }

                if (!group.Compose(transform, group.Identity).Equals(transform))
                {
                    throw new ArgumentOutOfRangeException();
                }

                if (!group.Compose(group.Inverse(transform), transform).Equals(group.Identity))
                {
                    throw new ArgumentOutOfRangeException();
                }
            }

            foreach (var left in transforms)
            {
                foreach (var right in transforms)
                {
                    if (!group.Compose(group.Inverse(left), group.Inverse(right)).Equals(group.Inverse(group.Compose(right, left))))
                    {
                        throw new ArgumentOutOfRangeException();
                    }
                }
            }

            foreach (var left in transforms)
            {
                foreach (var middle in transforms)
                {
                    foreach (var right in transforms)
                    {
                        if (!group.Compose(group.Compose(left, middle), right).Equals(group.Compose(left, group.Compose(middle, right))))
                        {
                            throw new ArgumentOutOfRangeException();
                        }
                    }
                }
            }
        }
    }
}
