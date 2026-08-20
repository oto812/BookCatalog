using System.ComponentModel.DataAnnotations;

namespace BookCatalog.Application.Validation
{
    public class EnumValueAttribute<T> : ValidationAttribute
        where T : Enum
    {
        public override bool IsValid(object? value)
        {
            if (value is T)
            {
                return Enum.IsDefined(typeof(T), (T)value);
            }
            return false;

        }
    }
}
