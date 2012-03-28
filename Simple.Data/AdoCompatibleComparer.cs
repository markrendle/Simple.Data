namespace Simple.Data
{
    using System.Collections.Generic;
    using Extensions;

    public class AdoCompatibleComparer : IEqualityComparer<string>
    {
        public static readonly HomogenizedEqualityComparer DefaultInstance = new HomogenizedEqualityComparer();

        public bool Equals(string x, string y)
        {
            return ReferenceEquals(x, y.Homogenize())
                   || x.Homogenize() == y.Homogenize()
                   || x.Homogenize().Singularize() == y.Homogenize().Singularize();
        }

        public int GetHashCode(string obj)
        {
            return obj.Homogenize().Singularize().GetHashCode();
        }
    }
}