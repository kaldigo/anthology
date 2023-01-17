namespace Anthology.Utils
{
    public static class ListUtils
    {
        public static bool CompareLists<T>(List<T> aList, List<T> bList)
        {
            var equal = true;
            if (!((aList == null || aList.Count == 0) && (bList == null || bList.Count == 0)))
            {
                if(((aList == null || aList.Count == 0) && !(bList == null || bList.Count == 0)) || (!(aList == null || aList.Count == 0) && (bList == null || bList.Count == 0))) return false;
                var aEqual = aList.All(a => bList.Any(b => EqualityComparer<T>.Default.Equals(a, b)));
                var bEqual = bList.All(b => aList.Any(a => EqualityComparer<T>.Default.Equals(a, b)));
                equal = aEqual && bEqual;
            }
            return equal;
        }
    }
}
