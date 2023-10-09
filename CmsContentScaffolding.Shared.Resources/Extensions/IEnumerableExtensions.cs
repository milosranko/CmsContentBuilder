namespace CmsContentScaffolding.Shared.Resources.Extensions
{
    internal static class IEnumerableExtensions
    {
        private static Random rnd;

        static IEnumerableExtensions()
        {
            rnd = new Random();
        }

        public static T Random<T>(this IEnumerable<T> source) where T : class
        {
            if (source == null) throw new ArgumentNullException("source");

            return source.ElementAt(rnd.Next(source.Count()));
        }
    }
}
