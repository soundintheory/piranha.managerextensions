using Microsoft.CodeAnalysis.CSharp.Syntax;
using Piranha.Extend.Fields;

namespace ExampleProject.Extensions
{
    public static class PiranhaFieldExtensions
    {
        public static bool IsNullOrWhiteSpace(this SimpleField<string> stringField)
        {
            return string.IsNullOrWhiteSpace(stringField?.Value);
        }

        public static bool IsNullOrEmpty(this SimpleField<string> stringField)
        {
            return string.IsNullOrEmpty(stringField?.Value);
        }

        public static bool IsNullOrEmpty(this ImageField imageField)
        {
            return imageField == null || !imageField.HasValue;
        }
    }
}
