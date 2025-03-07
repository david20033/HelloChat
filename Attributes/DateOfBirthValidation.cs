using System.ComponentModel.DataAnnotations;

namespace HelloChat.Attributes
{
    public class DateOfBirthValidation : ValidationAttribute
    {
        public DateOfBirthValidation()
        :base("You must be atleast 16 years old"){ }
        public override bool IsValid(object? value)
        {
            if (value is DateTime date)
            {
                DateTime now = DateTime.Now;
                return date < now && now.AddYears(-date.Year).Year >= 16;
            }
            return false;
        }
    }
}
