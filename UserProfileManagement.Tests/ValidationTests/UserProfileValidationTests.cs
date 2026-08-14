using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using UserProfileManagement.Models;
using Xunit;

namespace UserProfileManagement.Tests.ValidationTests
{
    public class UserProfileValidationTests
    {
        private List<ValidationResult> ValidateModel(object model)
        {
            var results = new List<ValidationResult>();
            var context = new ValidationContext(model, serviceProvider: null, items: null);
            Validator.TryValidateObject(model, context, results, validateAllProperties: true);
            return results;
        }

        [Fact]
        public void ValidateModel_ValidProfile_ReturnsNoValidationErrors()
        {
            // Arrange
            var user = new UserProfile
            {
                UserId = 1,
                FullName = "Siva Prakash",
                Email = "siva@example.com",
                PhoneNumber = "9876543210",
                DateOfBirth = new DateTime(2003, 5, 10),
                Address = "Madurai"
            };

            // Act
            var errors = ValidateModel(user);

            // Assert
            Assert.Empty(errors);
        }

        [Fact]
        public void ValidateModel_MissingFullName_ReturnsRequiredError()
        {
            // Arrange
            var user = new UserProfile
            {
                FullName = "", // Missing required field
                Email = "siva@example.com",
                PhoneNumber = "9876543210"
            };

            // Act
            var errors = ValidateModel(user);

            // Assert
            Assert.Contains(errors, e => e.MemberNames.Contains("FullName") && e.ErrorMessage == "Full Name is required.");
        }

        [Fact]
        public void ValidateModel_InvalidEmailFormat_ReturnsEmailAddressError()
        {
            // Arrange
            var user = new UserProfile
            {
                FullName = "Siva Prakash",
                Email = "invalid-email-format",
                PhoneNumber = "9876543210"
            };

            // Act
            var errors = ValidateModel(user);

            // Assert
            Assert.Contains(errors, e => e.MemberNames.Contains("Email") && e.ErrorMessage == "Please enter a valid email address.");
        }

        [Fact]
        public void ValidateModel_InvalidPhoneNumberLength_ReturnsRegexError()
        {
            // Arrange
            var user = new UserProfile
            {
                FullName = "Siva Prakash",
                Email = "siva@example.com",
                PhoneNumber = "12345" // Less than 10 digits
            };

            // Act
            var errors = ValidateModel(user);

            // Assert
            Assert.Contains(errors, e => e.MemberNames.Contains("PhoneNumber") && e.ErrorMessage == "Phone number must be 10 digits.");
        }

        [Fact]
        public void ValidateModel_AddressExceedsMaxLength_ReturnsStringLengthError()
        {
            // Arrange
            var user = new UserProfile
            {
                FullName = "Siva Prakash",
                Email = "siva@example.com",
                PhoneNumber = "9876543210",
                Address = new string('A', 251) // Exceeds 250 characters
            };

            // Act
            var errors = ValidateModel(user);

            // Assert
            Assert.Contains(errors, e => e.MemberNames.Contains("Address") && e.ErrorMessage == "Address cannot exceed 250 characters.");
        }
    }
}
