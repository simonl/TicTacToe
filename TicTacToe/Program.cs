using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace TicTacToe
{
    public enum Spins
    {
        Left = -1,
        Right = +1,
    }

    public interface ISymmetry<I>
    {
        Players SMap(Players first);
        IBoard<I> Map(IBoard<I> board);
        Position<I> PMap(IBoard<I> board, Position<I> target);
    }

    public sealed class Symmetry<I> : ISymmetry<I>
    {
        private readonly Func<Players, Players> SMapF;
        private readonly Func<IBoard<I>, IBoard<I>> MapF;
        private readonly Func<IBoard<I>, Position<I>, Position<I>> PMapF;

        public Symmetry(Func<Players, Players> sMapF, Func<IBoard<I>, IBoard<I>> mapF, Func<IBoard<I>, Position<I>, Position<I>> pMapF)
        {
            SMapF = sMapF;
            MapF = mapF;
            PMapF = pMapF;
        }

        public Players SMap(Players first)
        {
            return this.SMapF(first);
        }

        public IBoard<I> Map(IBoard<I> board)
        {
            return this.MapF(board);
        }
        
        public Position<I> PMap(IBoard<I> board, Position<I> target)
        {
            return this.PMapF(board, target);
        }
    }

    public sealed class Struggle<I> : IEquatable<Struggle<I>>, IComparable<Struggle<I>>
    {
        private readonly Position<I>[] Moves;

        public Struggle(Position<I>[] moves)
        {
            Moves = moves;
        }

        public int CompareTo(Struggle<I> other)
        {
            var comparing = Moves.Length.CompareTo(other.Moves.Length);
            if (comparing != 0)
            {
                return comparing;
            }

            for (int index = 0; index < Moves.Length; index++)
            {
                comparing = Moves[index].CompareTo(other.Moves[index]);

                if (comparing != 0)
                {
                    return comparing;
                }
            }

            return comparing;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Struggle<I>);
        }

        public bool Equals(Struggle<I> other)
        {
            return other != null &&
                   Enumerable.SequenceEqual(Moves, other.Moves);
        }

        public override int GetHashCode()
        {
            var hash = 0;
            foreach (var move in Moves)
            {
                hash = hash * 367 ^ move.GetHashCode();
            }
            return hash;
        }
    }

    public sealed class Vector : IEquatable<Vector>, IComparable<Vector>
    {
        public readonly Spins Observation;
        public readonly Spins Action;

        public Vector(Spins observation, Spins action)
        {
            Observation = observation;
            Action = action;
        }

        public override string ToString()
        {
            return this.Observation + ", " + this.Action;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Vector);
        }

        public bool Equals(Vector other)
        {
            return this.GetHashCode() == other.GetHashCode();
        }

        public override int GetHashCode()
        {
            return this.Indexify();
        }

        public int CompareTo(Vector other)
        {
            return this.GetHashCode().CompareTo(other.GetHashCode());
        }
    }

    public sealed class Mixing : IEquatable<Mixing>, IComparable<Mixing>
    {
        public readonly bool Swap;
        public readonly bool FlipO;
        public readonly bool FlipA;

        public Mixing(bool swap, bool flipO, bool flipA)
        {
            Swap = swap;
            FlipO = flipO;
            FlipA = flipA;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Mixing);
        }

        public bool Equals(Mixing other)
        {
            return this.GetHashCode() == other.GetHashCode();
        }

        public override int GetHashCode()
        {
            return (this.Swap ? 1 << 3 : 0) | (this.FlipO ? 1 << 2 : 0) | (this.FlipA ? 1 : 0);
        }

        public int CompareTo(Mixing other)
        {
            return this.GetHashCode().CompareTo(other.GetHashCode());
        }
    }

    /*
     * 
     * macrostate : type
     * infer : macrostate -> (microstate -> probability)
     * 
     * microstate : type
     * predict : microstate -> stream (o, a)
     * utility : microstate -> real
     * 
     * 
     * 
     * symmetry : type
     * transport : symmetry -> 
     * reify : symmetry -> (macrostate <-> macrostate)
     * 
     * project : symmetry -> (-1 | +1)
     * 
     * utility (reify S m) == project S * utility m
     * 
     * 
     * utility : functor (space, utilons)
     *  : microstate -> real
     *  : [x, y:microstate] -> (x ~> y) -> (utility x ~> utility y)
     * 
     * invariant : (microstate -> real) -> [x, y, z:microstate] -> real
     * invariant U [x, y, z] = (U(x) - U(z)) / (U(y) - U(z))
     * 
     * (U == V) = [x, y, z:S] -> invariant U [x, y, z] == invariant V [x, y, z]
     * 
     * 
     * 
     */
    public static class Program
    {
        static void Main(string[] args)
        {
            var mixer = MixGroup().Closure(MixBasis().ToArray());

            foreach (var mix in mixer)
            {
                foreach (var vector in UnIndexify)
                {
                    if (Utility(mix.Mix(vector)) != Factor(mix) * Utility(vector))
                    {
                        throw new ArgumentException();
                    }
                }
            }

            var flips = Transforms.Group.Closure(new Transform(true, true, 2), new Transform(false, true, 0));

            Emulate<TacIndices>(new TicTacToe(simplify: true));

            //Emulate<ConnectIndices>(new ConnectFour());

            Interactive<TacIndices>(new TicTacToe(simplify: true));
        }

        public static bool Match(Func<Vector, Vector> left, Func<Vector, Vector> right)
        {
            foreach (var vector in UnIndexify)
            {
                if (!left(vector).Equals(right(vector)))
                {
                    return false;
                }
            }

            return true;
        }

        public static IEnumerable<Mixing> MixBasis()
        {
            yield return new Mixing(true, false, false);
            yield return new Mixing(false, true, true);
            yield return new Mixing(false, false, true);
        }

        public static IGroup<Mixing> MixGroup()
        {
            return new Group<Mixing>(
                identity: new Mixing(false, false, false),
                inverseF: mixing => mixing,
                composeF: (left, right) => new Mixing(left.Swap ^ right.Swap, left.FlipO ^ right.FlipO, left.FlipA ^ right.FlipA));
        }

        public static Vector Mix(this Mixing mixing, Vector vector)
        {
            if (mixing.Swap)
            {
                vector = new Vector(vector.Action, vector.Observation);
            }

            if (mixing.FlipO)
            {
                vector = new Vector(vector.Observation == Spins.Left ? Spins.Right : Spins.Left, vector.Action);
            }

            if (mixing.FlipA)
            {
                vector = new Vector(vector.Observation, vector.Action == Spins.Left ? Spins.Right : Spins.Left);
            }

            return vector;
        }

        public static int Factor(Mixing mixing)
        {
            return (mixing.FlipO ? -1 : +1) * (mixing.FlipA ? -1 : +1);
        }

        public static IGroup<int> UtilityGroup()
        {
            return new Group<int>(
                identity: +1,
                inverseF: n => -n,
                composeF: (n, m) => n * m);
        }

        public static IEnumerable<int> UtilityBasis()
        {
            yield return -1;
        }

        public static Func<Vector, Vector> Permutation(int[] shuffle)
        {
            return vector => UnIndexify[shuffle[Indexify(vector)]];
        }

        private static readonly Vector[] UnIndexify = new[]
        {
            new Vector(Spins.Left, Spins.Left),
            new Vector(Spins.Left, Spins.Right),
            new Vector(Spins.Right, Spins.Left),
            new Vector(Spins.Right, Spins.Right),
        };

        public static int Indexify(this Vector vector)
        {
            return (vector.Action == Spins.Left ? 0 : 1) + (vector.Observation == Spins.Left ? 0 : 2);
        }

        public static int Utility(Vector vector)
        {
            return (int) vector.Observation * (int) vector.Action;
        }

        public static IEnumerable<int[]> Generate(bool[] permuted, int index = 0)
        {
            if (index == permuted.Length)
            {
                yield return new int[permuted.Length];
                yield break;
            }

            for (int factor = 0; factor < permuted.Length; factor++)
            {
                if (permuted[factor])
                {
                    continue;
                }

                permuted[factor] = true;

                foreach (var generated in Generate(permuted, index + 1))
                {
                    generated[index] = factor;

                    yield return generated;
                }

                permuted[factor] = false;
            }
        }

        public static void Interactive<I>(IGame<I> game) where I : struct
        {
            var board = Domains.Board<I>().Create(Players.Cross);

            while (true)
            {
                Console.WriteLine(board);

                foreach (var move in game.Links.Events(board).Select(move => game.Class(board, move).First().Key).ToHashSet())
                {
                    Console.WriteLine(move);
                }

                Console.Write("> ");
                var line = Console.ReadLine();

                Position<I> position;
                if (TryParsePosition<I>(out position, line))
                {
                    board = game.Dynamic.Update(board, position);
                }

                Console.WriteLine();
            }
        }

        public static void Emulate<I>(IGame<I> game)
        {
            IDictionary<IBoard<I>, int> canonicalBoards = new Dictionary<IBoard<I>, int>();
            IDictionary<IBoard<I>, IDictionary<Position<I>, Tuple<Transform, IBoard<I>>>> canonicalGraph = new Dictionary<IBoard<I>, IDictionary<Position<I>, Tuple<Transform, IBoard<I>>>>();

            var candidates = new Queue<IBoard<I>>();

            candidates.Enqueue(Domains.Board<I>().Create(Players.Cross));

            while (candidates.Count != 0)
            {
                var board = candidates.Dequeue();

                if (canonicalGraph.ContainsKey(board))
                {
                    continue;
                }

                var targets = new Dictionary<Position<I>, Tuple<Transform, IBoard<I>>>();

                foreach (var target in game.Links.Events(board))
                {
                    var canonical = game.Class(board, target).First();

                    if (targets.ContainsKey(canonical.Key))
                    {
                        continue;
                    }
                    
                    var next = game.Class(game.Dynamic.Update(board, target)).First();

                    targets.Add(canonical.Key, Tuple.Create(next.Value, next.Key));
                }

                canonicalGraph.Add(board, targets);

                foreach (var target in targets)
                {
                    candidates.Enqueue(target.Value.Item2);
                }

                if (targets.Count == 0)
                {
                    var winner = Score(game.Winner(board));

                    canonicalBoards.Add(board, winner);
                }
            }

            var canonicalMoves = 0;
            foreach (var move in canonicalGraph)
            {
                canonicalMoves += move.Value.Count;
            }

            var canonicalGames = new int[3];
            Count(canonicalGames, canonicalBoards, canonicalGraph.Map(edges => edges.Map(tuple => tuple.Item2)), Domains.Board<I>().Create(Players.Cross));

            var scores = new Dictionary<IBoard<I>, decimal>();

            var expectation = game.Game(scores, Domains.Board<I>().Create(Players.Cross));

            IDictionary<Struggle<I>, int> struggles = new Dictionary<Struggle<I>, int>();
            Populate<I>(game, struggles, new Stack<Position<I>>(), Domains.Board<I>().Create(Players.Cross));
        }

        private static void Populate<I>(IGame<I> game, IDictionary<Struggle<I>, int> struggles, Stack<Position<I>> moves, IBoard<I> board)
        {
            board = game.Class(board).First().Key;

            var visited = new HashSet<Position<I>>();

            foreach (var move in game.Links.Events(board))
            {
                visited.Add(game.Class(board, move).First().Key);
            }

            foreach (var move in visited)
            {
                moves.Push(move);

                Populate(game, struggles, moves, game.Dynamic.Update(board, move));

                moves.Pop();
            }

            if (visited.Count == 0)
            {
                var score = Score(game.Winner(board));

                struggles.Add(new Struggle<I>(moves.Reverse().ToArray()), score);
            }
        }

        private static void PopulateF<I>(IGame<I> game, IDictionary<Struggle<I>, int> struggles, Stack<Position<I>> moves, IBoard<I> board)
        {
            bool finished = true;
            foreach (var move in game.Links.Events(board))
            {
                finished = false;

                moves.Push(move);

                PopulateF(game, struggles, moves, game.Dynamic.Update(board, move));

                moves.Pop();
            }

            if (finished)
            {
                var score = Score(game.Winner(board));

                struggles.Add(new Struggle<I>(moves.Reverse().ToArray()), score);
            }
        }

        private static bool TryParsePosition<I>(out Position<I> position, string line) where I : struct
        {
            var parts = line.Split(',');

            I x, y;
            if (Enum.TryParse<I>(parts[0], true, out x))
            {
                if (Enum.TryParse<I>(parts[1], true, out y))
                {
                    position = new Position<I> { X = x, Y = y };
                    return true;
                }
            }

            position = new Position<I>();
            return false;
        }

        public static IDictionary<K, B> Map<K, A, B>(this IDictionary<K, A> dictionary, Func<A, B> convert)
        {
            var converted = new Dictionary<K, B>(dictionary.Count);

            foreach (var entry in dictionary)
            {
                converted.Add(entry.Key, convert(entry.Value));
            }

            return converted;
        }

        public static void Count<I>(int[] scores, IDictionary<IBoard<I>, int> winners, IDictionary<IBoard<I>, IDictionary<Position<I>, IBoard<I>>> graph, IBoard<I> board)
        {
            var moves = graph[board];

            if (moves.Count == 0)
            {
                scores[winners[board] + 1]++;
                
                return;
            }

            foreach (var move in moves)
            {
                Count(scores, winners, graph, move.Value);
            }
        }

        public static decimal Game<I>(this IGame<I> game, IDictionary<IBoard<I>, decimal> games, IBoard<I> board)
        {
            var canonical = game.Class(board).First();

            board = canonical.Key;

            if (games.ContainsKey(board))
            {
                return games[board];
            }

            var finished = true;
            var expectation = 0m;
            foreach (var position in game.Links.Events(board))
            {
                finished = false;

                var subscore = -game.Game(games, game.Dynamic.Update(board, position));

                expectation = Math.Max(expectation, subscore);
            }

            if (finished)
            {
                var score = Score(game.Winner(board));

                games[board] = score * Score(board.Turn);

                return games[board];
            }
            else
            {
                games[board] = expectation;

                return games[board];
            }
        }

        public static int Multiple(Transform transform)
        {
            return transform.Oppose ? -1 : 1;
        }

        public static int Score(Players? cell)
        {
            switch (cell)
            {
                case null:
                    return 0;
                case Players.Cross:
                    return -1;
                case Players.Circle:
                    return +1;
                default:
                    throw new ArgumentException();
            }
        }

        public static Tuple<ISymmetry<I>, ISymmetry<I>> ReifyT<I>(Transform transform)
        {
            return Tuple.Create(Reify<I>(transform), Reify<I>(Transforms.InverseT(transform)));
        }

        public static ISymmetry<I> Reify<I>(Transform transform)
        {
            var symmetry = Identity<I>();
            
            foreach (var fragment in Reification<I>(transform))
            {
                symmetry = Compose<I>(symmetry, fragment);
            }

            return symmetry;
        }

        public static IEnumerable<ISymmetry<I>> Reification<I>(Transform transform)
        {
            if (transform.Oppose)
            {
                yield return Opposite<I>();
            }

            foreach (var morphism in ReificationP<I>(transform))
            {
                yield return Positioning(morphism);
            }
        }

        public static IEnumerable<IMorphism<I>> ReificationP<I>(Transform transform)
        {
            if (transform.Mirror)
            {
                yield return Morphisms.MirrorH<I>();
            }

            for (int turn = 0; turn < transform.Rotate; turn++)
            {
                yield return Morphisms.Rotate<I>();
            }
        }

        public static Transform Canonical<I>(this IGame<I> game, IBoard<I> board)
        {
            return game.Class(board).First().Value;
        }

        public static IDictionary<IBoard<I>, Transform> Class<I>(this IGame<I> game, IBoard<I> board)
        {
            var mapping = new SortedDictionary<IBoard<I>, Transform>();
            
            foreach (var symmetry in game.Symmetries)
            {
                var next = ReifyT<I>(symmetry).Item1.Map(board);

                if (mapping.ContainsKey(next))
                {
                    continue;
                }

                mapping.Add(next, Transforms.InverseT(symmetry));
            }

            return mapping;
        }

        public static Transform Canonical<I>(this IGame<I> game, IBoard<I> board, Position<I> move)
        {
            return game.Class(board, move).First().Value;
        }

        public static IDictionary<Position<I>, Transform> Class<I>(this IGame<I> game, IBoard<I> board, Position<I> move)
        {
            var mapping = new SortedDictionary<Position<I>, Transform>();

            var master = game.Class(game.Dynamic.Update(board, move));

            foreach (var symmetry in game.Symmetries)
            {
                var target = ReifyT<I>(symmetry).Item1.PMap(board, move);

                if (mapping.ContainsKey(target) || !game.Links.Events(board).Contains(target))
                {
                    continue;
                }

                var next = game.Dynamic.Update(board, target);

                if (master.ContainsKey(next))
                {
                    mapping.Add(target, Transforms.InverseT(symmetry));
                }
            }

            return mapping;
        }

        public static IDomain<Players, Tuple<IBoard<I>, Transform>, Position<I>> Canonical<I>(IGame<I> game)
        {
            return new Domain<Players, Tuple<IBoard<I>, Transform>, Position<I>>(
                eventsF: (board, position) =>
                {
                    return game.Dynamic.Events(board.Item1, ReifyT<I>(board.Item2).Item2.PMap(null, position));
                },
                createF: selector =>
                {
                    var canonical = game.Class(game.Dynamic.Create(selector)).First();

                    return Tuple.Create(canonical.Key, canonical.Value);
                },
                updateF: (board, position) =>
                {
                    position = ReifyT<I>(board.Item2).Item2.PMap(null, position);

                    var canonical = game.Class(game.Dynamic.Update(board.Item1, position)).First();

                    return Tuple.Create(canonical.Key, Transforms.ComposeT(canonical.Value, board.Item2));
                });
        }

        public static IDiscrete<Players, Tuple<IBoard<I>, Transform>, Position<I>> CanonicalLinks<I>(IGame<I> game)
        {
            return new Discrete<Players, Tuple<IBoard<I>, Transform>, Position<I>>(
                selectors: new Players[] { Players.Cross },
                eventsF: board =>
                {
                    var visited = new SortedSet<Position<I>>();

                    foreach (var move in game.Links.Events(board.Item1))
                    {
                        var canonical = game.Class(board.Item1, move).First();

                        visited.Add(ReifyT<I>(canonical.Value).Item1.PMap(null, canonical.Key));
                    }

                    return visited.ToArray();
                });
        }

        public static IEnumerable<Tuple<ISymmetry<I>, ISymmetry<I>>> Symmetries<I>()
        {
            foreach (var morhism in Morphisms.AllMorphisms<I>())
            {
                yield return Tuple.Create(Positioning(morhism), Positioning(Morphisms.Inverse(morhism)));
                yield return Tuple.Create(Compose(Opposite<I>(), Positioning(morhism)), Compose(Opposite<I>(), Positioning(Morphisms.Inverse(morhism))));
            }

            //foreach (var first in SimpleSymmetries())
            //{
            //    foreach (var second in SimpleSymmetries())
            //    {
            //        yield return Tuple.Create(Compose(first.Item1, second.Item1), Compose(second.Item2, first.Item1));
            //    }
            //}
        }

        public static IEnumerable<Tuple<ISymmetry<I>, ISymmetry<I>>> SimpleSymmetries<I>()
        {
            yield return Tuple.Create(Opposite<I>(), Opposite<I>());

            foreach (var morphism in Morphisms.AllMorphisms<I>())
            {
                yield return Tuple.Create(Positioning(morphism), Positioning(Morphisms.Inverse(morphism)));
            }

        }

        public static ISymmetry<I> Identity<I>()
        {
            return new Symmetry<I>(
                sMapF: first => first,
                mapF: board => board,
                pMapF: (board, target) => target);
        }

        public static ISymmetry<I> Compose<I>(ISymmetry<I> left, ISymmetry<I> right)
        {
            return new Symmetry<I>(
                sMapF: first => right.SMap(left.SMap(first)),
                mapF: board => Copy(right.Map(left.Map(board))),
                pMapF: (board, target) =>
                {
                    return right.PMap(left.Map(board), left.PMap(board, target));
                });
        }

        public static long Compress<I>(this IBoard<I> board)
        {
            var bitmap = (long) board.Turn;
            foreach (var position in Morphisms.Positions<I>())
            {
                bitmap = (bitmap << 2) | Compress(board[position]);
            }
            return bitmap;
        }

        public static long Compress(Players? cell)
        {
            switch (cell)
            {
                case null:
                    return 0;
                case Players.Cross:
                    return 1;
                case Players.Circle:
                    return 2;
                default:
                    throw new ArgumentException();
            }
        }

        public static ISymmetry<I> Opposite<I>()
        {
            return new Symmetry<I>(
                sMapF: first => Opposite(first),
                mapF: board => Copy(Opposite(board)),
                pMapF: (board, target) => target);
        }

        public static ISymmetry<I> Positioning<I>(IMorphism<I> morphism)
        {
            return new Symmetry<I>(
                sMapF: first => first,
                mapF: board => Copy(new Board<I>(board.Turn, position => board[morphism.Backward(position)])),
                pMapF: (board, target) => morphism.Forward(target));
        }

        public static Players Opposite(Players player)
        {
            return (Players) (1 - (int) player);
        }

        public static Players? Opposite(Players? cell)
        {
            if (cell.HasValue)
            {
                return Opposite(cell.Value);
            }

            return null;
        }

        public static IBoard<I> Opposite<I>(IBoard<I> board)
        {
            return new Board<I>(turn: Opposite(board.Turn),
                fetchF: position => Opposite(board[position]));
        }

        public static IBoard<I> Copy<I>(this IBoard<I> board)
        {
            var indices = Morphisms.Indices<I>();

            var cells = new Players?[indices.Length, indices.Length];

            foreach (var column in indices)
            {
                foreach (var row in indices)
                {
                    cells[(int) (object) column, (int) (object) row] = board[new Position<I> { X = column, Y = row }];
                }
            }

            return new Board<I>(board.Turn,
                fetchF: position => cells[(int) (object) position.X, (int) (object) position.Y]);
        }
    }
}
