using System.ComponentModel.DataAnnotations;

namespace HelloChat.Attributes
{
    public class DateOfBirthValidation : ValidationAttribute
    {
        public DateOfBirthValidation()
        : base("You must be atleast 16 years old") { }
        public override bool IsValid(object? value)
        {
            if (value is DateTime date)
            {
                DateTime now = DateTime.Now;
                int age = now.Year - date.Year;
                if (now.Month < date.Month || (now.Month == date.Month && now.Day < date.Day))
                    age--;
                return date < now && age >= 16;
            }
            return false;
        }
    }
}
