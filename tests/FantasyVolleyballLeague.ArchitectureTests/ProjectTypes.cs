using System.Reflection;
using FantasyVolleyballLeague.Api.Common.Endpoints;

namespace FantasyVolleyballLeague.ArchitectureTests
{
    internal static class ApiTypes
    {
        public const string RootNamespace = "FantasyVolleyballLeague.Api";

        private const BindingFlags AllDeclared =
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;

        public static Assembly Assembly { get; } = typeof(IEndpoint).Assembly;

        public static IEnumerable<Type> InNamespace(string relativeNamespace)
        {
            var fullNamespace = $"{RootNamespace}.{relativeNamespace}";
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
