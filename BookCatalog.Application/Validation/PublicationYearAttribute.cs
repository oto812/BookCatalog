using System.ComponentModel.DataAnnotations;

namespace BookCatalog.Application.Validation
{
    // [Range(0, 2026)] is wrong the moment the year turns over: on 1 January it starts
    // rejecting books published this year. The upper bound has to be read when the request
    // arrives, not written into the attribute when the code is compiled.
    public class PublicationYearAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            if (value is not int year)
            {
                return false;
            }

            return year >= 0 && year <= DateTime.UtcNow.Year;
        }

        // Built here rather than passed as ErrorMessage, so the year in the message is the
        // same one IsValid compared against.
        public override string FormatErrorMessage(string name)
        {
            return $"{name} must be between 0 and {DateTime.UtcNow.Year}.";
        }
    }
}
