namespace ComAbilities.Types.RueTasks
{
    using System.Collections;
    using System.Reflection;

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class IncludeECAttribute : Attribute { }

    public abstract class Enumeration<T> : IReadOnlyCollection<T>
    {
        protected IReadOnlyCollection<T> properties { get; private set; }

        public Enumeration(Type type) {
            properties = type
                .GetProperties(BindingFlags.Public | BindingFlags.DeclaredOnly)
                .Where(x => x.PropertyType == typeof(T))
                .Where(x => Attribute.IsDefined(x, typeof(IncludeECAttribute)))
                .Select(f => f.GetValue(this))
                .Cast<T>()
                .ToArray();
        }

        public int Count => properties.Count();

        public IEnumerator<T> GetEnumerator() => properties.AsEnumerable().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    }
}
