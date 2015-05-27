namespace LauncherZLib.Utils
{
    public class TaggedObject<T> where T : class
    {
        private readonly string _tag;
        private readonly T _object;

        public string Tag { get { return _tag; } }

        public T Object { get { return _object; } }

        public TaggedObject(string tag, T o)
        {
            _tag = tag;
            _object = o;
        }
    }
}
