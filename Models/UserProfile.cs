using System;
using System.ComponentModel.DataAnnotations;

namespace UserProfileManagement.Models
{
    /// <summary>
    /// Custom Validation Attribute to prevent selecting future dates for Date of Birth.
    /// </summary>
    public class NotInFutureDateAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is DateTime dateValue)
            {
                if (dateValue.Date > DateTime.Now.Date)
                {
                    return new ValidationResult(ErrorMessage ?? "Date of birth cannot be in the future.");
                }
            }
            return ValidationResult.Success;
        }
    }

    /// <summary>
    /// Represents a user profile in the application.
    /// Mapped to the UserProfiles database table.
    /// </summary>
    public class UserProfile
    {
        /// <summary>
        /// Primary Key
        /// </summary>
        [Key]
        public int UserId { get; set; }

        /// <summary>
        /// User Full Name
        /// </summary>
        [Required(ErrorMessage = "Full Name is required.")]
        [StringLength(100, ErrorMessage = "Full Name cannot exceed 100 characters.")]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        /// <summary>
        /// User Email Address
        /// </summary>
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        [StringLength(150, ErrorMessage = "Email address cannot exceed 150 characters.")]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// User Phone Number
        /// </summary>
        [Required(ErrorMessage = "Phone Number is required.")]
        [RegularExpression(@"^[0-9]{10}$", ErrorMessage = "Phone number must be 10 digits.")]
        [StringLength(10, ErrorMessage = "Phone number must be exactly 10 digits.")]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; } = string.Empty;

        /// <summary>
        /// User Date of Birth
        /// </summary>
        [DataType(DataType.Date)]
        [NotInFutureDate(ErrorMessage = "Date of birth cannot be in the future.")]
        [Display(Name = "Date of Birth")]
        public DateTime? DateOfBirth { get; set; }

        /// <summary>
        /// User Address
        /// </summary>
        [StringLength(250, ErrorMessage = "Address cannot exceed 250 characters.")]
        [Display(Name = "Address")]
        public string? Address { get; set; }

        /// <summary>
        /// Profile Picture Path
        /// </summary>
        [StringLength(255)]
        [Display(Name = "Profile Picture")]
        public string? ProfilePicture { get; set; }

        /// <summary>
        /// Record Created Date
        /// </summary>
        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// Record Last Updated Date
        /// </summary>
        [Display(Name = "Updated At")]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}