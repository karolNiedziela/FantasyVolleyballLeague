using System.Reflection;
using FantasyVolleyballLeague.DataApi.Common.Endpoints;
using FantasyVolleyballLeague.VolleyballData.Domain;

namespace FantasyVolleyballLeague.ArchitectureTests
{
    /// <summary>
    /// Types of one production project, used by the architecture tests.
    /// </summary>
    internal sealed class ProjectTypes
    {
        private const BindingFlags AllDeclared =
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;

        private readonly string rootNamespace;

        private ProjectTypes(Assembly assembly, string rootNamespace)
        {
            Assembly = assembly;
            this.rootNamespace = rootNamespace;
        }

        public static ProjectTypes DataApi { get; } = new ProjectTypes(typeof(IEndpoint).Assembly, "FantasyVolleyballLeague.DataApi");

        public static ProjectTypes VolleyballData { get; } = new ProjectTypes(typeof(Entity).Assembly, "FantasyVolleyballLeague.VolleyballData");

        public Assembly Assembly { get; }

        public IEnumerable<Type> InNamespace(string relativeNamespace)
        {
            var fullNamespace = $"{rootNamespace}.{relativeNamespace}";
            return Assembly.GetTypes()
                .Where(t => t.Namespace is not null
                    && (t.Namespace == fullNamespace || t.Namespace.StartsWith($"{fullNamespace}.", StringComparison.Ordinal)));
        }

        public static IEnumerable<Type> SignatureDependencies(Type type)
        {
            var direct = new List<Type>();
            if (type.BaseType is not null)
            {
                direct.Add(type.BaseType);
            }

            direct.AddRange(type.GetInterfaces());
            direct.AddRange(type.GetFields(AllDeclared).Select(f => f.FieldType));
            direct.AddRange(type.GetProperties(AllDeclared).Select(p => p.PropertyType));
            direct.AddRange(type.GetConstructors(AllDeclared).SelectMany(c => c.GetParameters()).Select(p => p.ParameterType));

            foreach (var method in type.GetMethods(AllDeclared))
            {
                direct.Add(method.ReturnType);
                direct.AddRange(method.GetParameters().Select(p => p.ParameterType));
            }

            return direct.SelectMany(Flatten).Distinct();
        }

        private static IEnumerable<Type> Flatten(Type type)
        {
            if (type.HasElementType)
            {
                return Flatten(type.GetElementType()!);
            }

            return type.IsGenericType
                ? new[] { type }.Concat(type.GetGenericArguments().SelectMany(Flatten))
                : [type];
        }
    }
}
