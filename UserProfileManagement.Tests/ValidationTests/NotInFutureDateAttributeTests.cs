using System;
using System.ComponentModel.DataAnnotations;
using UserProfileManagement.Models;
using Xunit;

namespace UserProfileManagement.Tests.ValidationTests
{
    public class NotInFutureDateAttributeTests
    {
        private readonly NotInFutureDateAttribute _attribute;

        public NotInFutureDateAttributeTests()
        {
            _attribute = new NotInFutureDateAttribute
            {
                ErrorMessage = "Date of birth cannot be in the future."
            };
        }

        [Fact]
        public void IsValid_PastDate_ReturnsSuccess()
        {
            // Arrange
            var pastDate = DateTime.Now.AddYears(-20);
            var validationContext = new ValidationContext(new object());

            #pragma warning disable CS8602 // Dereference of a possibly null reference.
            // Act
            var result = _attribute.GetValidationResult(pastDate, validationContext);
            #pragma warning restore CS8602

            // Assert
            Assert.Equal(ValidationResult.Success, result);
        }

        [Fact]
        public void IsValid_TodayDate_ReturnsSuccess()
        {
            // Arrange
            var today = DateTime.Now.Date;
            var validationContext = new ValidationContext(new object());

            #pragma warning disable CS8602
            // Act
            var result = _attribute.GetValidationResult(today, validationContext);
            #pragma warning restore CS8602

            // Assert
            Assert.Equal(ValidationResult.Success, result);
        }

        [Fact]
        public void IsValid_NullValue_ReturnsSuccess()
        {
            // Arrange
            DateTime? nullDate = null;
            var validationContext = new ValidationContext(new object());

            #pragma warning disable CS8602
            // Act
            var result = _attribute.GetValidationResult(nullDate, validationContext);
            #pragma warning restore CS8602

            // Assert
            Assert.Equal(ValidationResult.Success, result);
        }

        [Fact]
        public void IsValid_FutureDate_ReturnsValidationError()
        {
            // Arrange
            var futureDate = DateTime.Now.AddDays(5);
            var validationContext = new ValidationContext(new object());

            #pragma warning disable CS8602
            // Act
            var result = _attribute.GetValidationResult(futureDate, validationContext);
            #pragma warning restore CS8602

            // Assert
            Assert.NotNull(result);
            Assert.NotEqual(ValidationResult.Success, result);
            Assert.Equal("Date of birth cannot be in the future.", result.ErrorMessage);
        }
    }
}
