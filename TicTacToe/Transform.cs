using System;
using System.Collections.Generic;

namespace TicTacToe
{
    public sealed class Transform : IEquatable<Transform>, IComparable<Transform>
    {
        public readonly bool Oppose;
        public readonly bool Mirror;
        public readonly int Rotate;

        public Transform(bool oppose, bool mirror, int rotate)
        {
            this.Oppose = oppose;
            this.Mirror = mirror;
            this.Rotate = rotate;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Transform);
        }

        public bool Equals(Transform other)
        {
            return this.Compress() == other.Compress();
        }

        public override int GetHashCode()
        {
            return this.Compress();
        }

        public int CompareTo(Transform other)
        {
            return this.Compress().CompareTo(other.Compress());
        }
    }

    public static class Transforms
    {
        public static IEnumerable<Transform> BaseTransforms()
        {
            yield return new Transform(true, false, 0);
            yield return new Transform(false, true, 0);
            yield return new Transform(false, false, 1);
        }

        public static int Compress(this Transform transform)
        {
            return (transform.Oppose ? 1 << 3 : 0) | (transform.Mirror ? 1 << 2 : 0) | transform.Rotate;
        }

        public static IGroup<Transform> Group
        {
            get { return new Group<Transform>(IdentityT(), InverseT, ComposeT); }
        }

        public static Transform IdentityT()
        {
            return new Transform(false, false, 0);
        }

        public static Transform ComposeT(Transform left, Transform right)
        {
            return new Transform(left.Oppose ^ right.Oppose, left.Mirror ^ right.Mirror, ((right.Mirror ? 4 - left.Rotate : left.Rotate) + right.Rotate) % 4);
        }

        public static Transform InverseT(Transform transform)
        {
            return new Transform(transform.Oppose, transform.Mirror, (transform.Mirror ? transform.Rotate : 4 - transform.Rotate) % 4);
        }
    }
}
