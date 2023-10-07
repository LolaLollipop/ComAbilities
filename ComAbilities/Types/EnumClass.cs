namespace ComAbilities.Types.RueTasks
{
    using Exiled.API.Features;
    using MEC;
    using System.Collections;
    using System.Diagnostics.CodeAnalysis;
    using System.Reflection;
    using UnityEngine;

    [AttributeUsage(AttributeTargets.Property)]
    public class IncludeECAttribute : Attribute { }

    public abstract class Enumeration<T> : IReadOnlyCollection<T>
    {
        private T[] properties;

        public Enumeration() {
            properties = this.GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.DeclaredOnly)
                .Where(x => x.PropertyType == typeof(T))
                .Where(x => Attribute.IsDefined(x, typeof(IncludeECAttribute)))
                .Select(f => f.GetValue(null))
                .Cast<T>()
                .ToArray();
        }

        public int Count => properties.Count();



        public IEnumerator<T> GetEnumerator() => properties.AsEnumerable().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    }
}
