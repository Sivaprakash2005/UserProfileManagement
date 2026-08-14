using System;
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

namespace UserProfileManagement.Tests.IntegrationTests
{
    public class ProfileIntegrationTests
    {
        private ApplicationDbContext GetDatabaseContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;

            var context = new ApplicationDbContext(options);
            context.Database.EnsureCreated();
            return context;
        }

        private ProfileController CreateController(ApplicationDbContext context)
        {
            var mockLogger = new Mock<ILogger<ProfileController>>();
            var controller = new ProfileController(context, mockLogger.Object);
            var httpContext = new DefaultHttpContext();
            var tempDataProvider = new Mock<ITempDataProvider>();
            controller.TempData = new TempDataDictionary(httpContext, tempDataProvider.Object);
            return controller;
        }

        [Fact]
        public async Task IntegrationTest_FullUserProfileLifecycle_CreateReadUpdateVerifyTimestamps()
        {
            // Arrange Database & Controller
            var dbName = Guid.NewGuid().ToString();
            using (var context = GetDatabaseContext(dbName))
            {
                var controller = CreateController(context);

                // Step 1: Create a new user profile
                var newProfile = new UserProfile
                {
                    FullName = "Integration User",
                    Email = "integration@test.com",
                    PhoneNumber = "9988776655",
                    DateOfBirth = new DateTime(1995, 6, 15),
                    Address = "Initial Address"
                };

                var createResult = await controller.Create(newProfile);
                var redirectResult = Assert.IsType<RedirectToActionResult>(createResult);
                Assert.Equal("Index", redirectResult.ActionName);
            }

            // Step 2: Read profile from database in a fresh scope
            int createdId;
            using (var context = GetDatabaseContext(dbName))
            {
                var createdUser = await context.UserProfiles.FirstOrDefaultAsync(u => u.Email == "integration@test.com");
                Assert.NotNull(createdUser);
                createdId = createdUser.UserId;
                Assert.Equal("Integration User", createdUser.FullName);

                var controller = CreateController(context);
                var indexResult = await controller.Index(createdId);
                var viewResult = Assert.IsType<ViewResult>(indexResult);
                var model = Assert.IsType<UserProfile>(viewResult.Model);
                Assert.Equal("Initial Address", model.Address);
            }

            // Step 3: Update profile details and verify timestamp changes
            using (var context = GetDatabaseContext(dbName))
            {
                var controller = CreateController(context);
                var updateForm = new UserProfile
                {
                    UserId = createdId,
                    FullName = "Integration User Updated",
                    Email = "integration.updated@test.com",
                    PhoneNumber = "9988776655",
                    DateOfBirth = new DateTime(1995, 6, 15),
                    Address = "Updated Street Address, Chennai"
                };

                var editPostResult = await controller.Edit(createdId, updateForm);
                Assert.IsType<RedirectToActionResult>(editPostResult);
            }

            // Step 4: Verify persistence in database
            using (var context = GetDatabaseContext(dbName))
            {
                var finalUser = await context.UserProfiles.FindAsync(createdId);
                Assert.NotNull(finalUser);
                Assert.Equal("Integration User Updated", finalUser.FullName);
                Assert.Equal("integration.updated@test.com", finalUser.Email);
                Assert.Equal("Updated Street Address, Chennai", finalUser.Address);
                Assert.True(finalUser.UpdatedAt >= finalUser.CreatedAt);
            }
        }

        [Fact]
        public async Task IntegrationTest_MultipleUsersSeeded_ProfileSwitcherFunctionality()
        {
            // Arrange & Seed Multiple Users
            var dbName = Guid.NewGuid().ToString();
            using (var context = GetDatabaseContext(dbName))
            {
                context.UserProfiles.AddRange(
                    new UserProfile { UserId = 1, FullName = "Siva Prakash", Email = "siva@example.com", PhoneNumber = "9876543210" },
                    new UserProfile { UserId = 2, FullName = "Priya Sharma", Email = "priya@example.com", PhoneNumber = "9876543211" },
                    new UserProfile { UserId = 3, FullName = "Rahul Kumar", Email = "rahul@example.com", PhoneNumber = "9876543212" },
                    new UserProfile { UserId = 4, FullName = "Ananya Roy", Email = "ananya@example.com", PhoneNumber = "9876543213" },
                    new UserProfile { UserId = 5, FullName = "Karthik Raja", Email = "karthik@example.com", PhoneNumber = "9876543214" }
                );
                await context.SaveChangesAsync();
            }

            // Act & Assert Switching to User 4
            using (var context = GetDatabaseContext(dbName))
            {
                var controller = CreateController(context);
                var result = await controller.Index(4);
                var viewResult = Assert.IsType<ViewResult>(result);
                var model = Assert.IsType<UserProfile>(viewResult.Model);

                Assert.Equal(4, model.UserId);
                Assert.Equal("Ananya Roy", model.FullName);
                Assert.Equal("ananya@example.com", model.Email);
            }
        }
    }
}
