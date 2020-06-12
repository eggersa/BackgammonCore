using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Backgammon.Game
{
    public class PlyEqualityComparer : EqualityComparer<Ply>
    {
        public static PlyEqualityComparer Instance { get; } = new PlyEqualityComparer();

        public override bool Equals([AllowNull] Ply x, [AllowNull] Ply y)
        {
            if (x == null && y == null)
            {
                return true;
            }

            return x.Same(y);
        }

        public override int GetHashCode([DisallowNull] Ply obj)
        {
            return obj.GetHashCode();
        }
    }
}
