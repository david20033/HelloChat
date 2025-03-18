using System.ComponentModel.DataAnnotations;

namespace HelloChat.ViewModels
{
    public class EditProfileViewModel
    {
        public string Id { get; set; }
        public IFormFile? ProfileImage { get; set; }
        public string? ProfileImagePath { get; set; }
        [Required(ErrorMessage = "First Name is required.")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "First Name must be between 2 and 50 characters.")]
        [RegularExpression(@"^[a-zA-Z\s\-]+$", ErrorMessage = "First Name can only contain letters, spaces, and hyphens.")]
        public string FirstName { get; set; }
        [Required(ErrorMessage = "Last Name is required.")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Last Name must be between 2 and 50 characters.")]
        [RegularExpression(@"^[a-zA-Z\s\-]+$", ErrorMessage = "Last Name can only contain letters, spaces, and hyphens.")]
        public string LastName { get; set; }
        [Required(ErrorMessage = "Phone number is required.")]
        [RegularExpression(@"^(\+?[1-9]\d{1,14}|0\d{9,15})$", ErrorMessage = "Please enter a valid phone number.")]
        public string PhoneNumber { get; set; }
        [EmailAddress(ErrorMessage = "Email address is not in correct format")]
        public string Email { get; set; }

    }
}
