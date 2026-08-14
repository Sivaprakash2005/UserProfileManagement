using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using UserProfileManagement.Controllers;
using UserProfileManagement.Data;
using UserProfileManagement.Models;
using Xunit;

namespace UserProfileManagement.Tests.ControllerTests
{
    public class ProfileControllerUnitTests
    {
        private ApplicationDbContext GetInMemoryDbContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;

            return new ApplicationDbContext(options);
        }

        private ProfileController GetController(ApplicationDbContext context)
        {
            var mockLogger = new Mock<ILogger<ProfileController>>();
            var controller = new ProfileController(context, mockLogger.Object);
            var httpContext = new DefaultHttpContext();
            var tempDataProvider = new Mock<ITempDataProvider>();
            controller.TempData = new TempDataDictionary(httpContext, tempDataProvider.Object);
            return controller;
        }

        [Fact]
        public async Task Index_ReturnsViewWithProfiles_WhenProfilesExist()
        {
            // Arrange
            using var context = GetInMemoryDbContext(Guid.NewGuid().ToString());
            context.UserProfiles.Add(new UserProfile
            {
                UserId = 1,
                FullName = "Siva Prakash",
                Email = "siva@example.com",
                PhoneNumber = "9876543210"
            });
            await context.SaveChangesAsync();

            var controller = GetController(context);

            // Act
            var result = await controller.Index(null);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<UserProfile>(viewResult.Model);
            Assert.Equal("Siva Prakash", model.FullName);
        }

        [Fact]
        public async Task Index_ReturnsNoProfileView_WhenDatabaseIsEmpty()
        {
            // Arrange
            using var context = GetInMemoryDbContext(Guid.NewGuid().ToString());
            var controller = GetController(context);

            // Act
            var result = await controller.Index(null);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("NoProfile", viewResult.ViewName);
        }

        [Fact]
        public async Task Index_SpecifiedId_ReturnsRequestedProfile()
        {
            // Arrange
            using var context = GetInMemoryDbContext(Guid.NewGuid().ToString());
            context.UserProfiles.AddRange(
                new UserProfile { UserId = 1, FullName = "Siva Prakash", Email = "siva@example.com", PhoneNumber = "9876543210" },
                new UserProfile { UserId = 2, FullName = "Priya Sharma", Email = "priya@example.com", PhoneNumber = "9876543211" }
            );
            await context.SaveChangesAsync();

            var controller = GetController(context);

            // Act
            var result = await controller.Index(2);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<UserProfile>(viewResult.Model);
            Assert.Equal("Priya Sharma", model.FullName);
        }

        [Fact]
        public async Task EditGet_InvalidId_ReturnsNotFound()
        {
            // Arrange
            using var context = GetInMemoryDbContext(Guid.NewGuid().ToString());
            var controller = GetController(context);

            // Act
            var result = await controller.Edit(0);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task EditGet_NonExistentUser_ReturnsNotFound()
        {
            // Arrange
            using var context = GetInMemoryDbContext(Guid.NewGuid().ToString());
            var controller = GetController(context);

            // Act
            var result = await controller.Edit(99);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task EditGet_ValidId_ReturnsEditViewWithModel()
        {
            // Arrange
            using var context = GetInMemoryDbContext(Guid.NewGuid().ToString());
            context.UserProfiles.Add(new UserProfile
            {
                UserId = 1,
                FullName = "Siva Prakash",
                Email = "siva@example.com",
                PhoneNumber = "9876543210"
            });
            await context.SaveChangesAsync();

            var controller = GetController(context);

            // Act
            var result = await controller.Edit(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<UserProfile>(viewResult.Model);
            Assert.Equal("Siva Prakash", model.FullName);
        }

        [Fact]
        public async Task EditPost_IdMismatch_ReturnsNotFound()
        {
            // Arrange
            using var context = GetInMemoryDbContext(Guid.NewGuid().ToString());
            var controller = GetController(context);
            var form = new UserProfile { UserId = 2, FullName = "Test" };

            // Act
            var result = await controller.Edit(1, form);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task EditPost_InvalidModelState_ReturnsViewWithFormModel()
        {
            // Arrange
            using var context = GetInMemoryDbContext(Guid.NewGuid().ToString());
            var controller = GetController(context);
            controller.ModelState.AddModelError("Email", "Email is required.");
            var form = new UserProfile { UserId = 1, FullName = "Siva Prakash", Email = "", PhoneNumber = "9876543210" };

            // Act
            var result = await controller.Edit(1, form);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(form, viewResult.Model);
        }

        [Fact]
        public async Task EditPost_ValidModel_UpdatesUserAndRedirectsToIndex()
        {
            // Arrange
            using var context = GetInMemoryDbContext(Guid.NewGuid().ToString());
            context.UserProfiles.Add(new UserProfile
            {
                UserId = 1,
                FullName = "Siva Prakash",
                Email = "siva@example.com",
                PhoneNumber = "9876543210",
                Address = "Madurai"
            });
            await context.SaveChangesAsync();

            var controller = GetController(context);
            var form = new UserProfile
            {
                UserId = 1,
                FullName = "Siva Prakash Updated",
                Email = "siva.updated@example.com",
                PhoneNumber = "9876543210",
                Address = "Chennai"
            };

            // Act
            var result = await controller.Edit(1, form);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal(1, redirectResult.RouteValues?["id"]);

            var updatedUser = await context.UserProfiles.FindAsync(1);
            Assert.NotNull(updatedUser);
            Assert.Equal("Siva Prakash Updated", updatedUser.FullName);
            Assert.Equal("siva.updated@example.com", updatedUser.Email);
            Assert.Equal("Chennai", updatedUser.Address);
        }

        [Fact]
        public void CreateGet_ReturnsCreateView()
        {
            // Arrange
            using var context = GetInMemoryDbContext(Guid.NewGuid().ToString());
            var controller = GetController(context);

            // Act
            var result = controller.Create();

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task CreatePost_DuplicateEmail_AddsModelErrorAndReturnsView()
        {
            // Arrange
            using var context = GetInMemoryDbContext(Guid.NewGuid().ToString());
            context.UserProfiles.Add(new UserProfile
            {
                UserId = 1,
                FullName = "Existing User",
                Email = "duplicate@example.com",
                PhoneNumber = "9876543210"
            });
            await context.SaveChangesAsync();

            var controller = GetController(context);
            var form = new UserProfile
            {
                FullName = "New User",
                Email = "duplicate@example.com",
                PhoneNumber = "9876543211"
            };

            // Act
            var result = await controller.Create(form);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.True(controller.ModelState.ContainsKey("Email"));
        }

        [Fact]
        public async Task CreatePost_ValidModel_CreatesUserAndRedirectsToIndex()
        {
            // Arrange
            using var context = GetInMemoryDbContext(Guid.NewGuid().ToString());
            var controller = GetController(context);
            var form = new UserProfile
            {
                FullName = "New Developer",
                Email = "new.dev@example.com",
                PhoneNumber = "9876543299",
                Address = "Bangalore"
            };

            // Act
            var result = await controller.Create(form);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);

            var createdUser = await context.UserProfiles.FirstOrDefaultAsync(u => u.Email == "new.dev@example.com");
            Assert.NotNull(createdUser);
            Assert.Equal("New Developer", createdUser.FullName);
        }
    }
}
