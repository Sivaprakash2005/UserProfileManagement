using Microsoft.EntityFrameworkCore;
using UserProfileManagement.Data;
using UserProfileManagement.Models;

var builder = WebApplication.CreateBuilder(args);

// Add MVC Services
builder.Services.AddControllersWithViews();

// Configure ApplicationDbContext with SQL Server / LocalDB
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);

var app = builder.Build();

// Ensure Database Creation & Initial Seed Data (5 Default Profiles)
using (var scope = app.Services.CreateScope())
{
    try
    {
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        context.Database.EnsureCreated();

        var existingEmails = context.UserProfiles
                                    .Select(u => u.Email.ToLower())
                                    .ToHashSet();

        var defaultProfiles = new List<UserProfile>
        {
            new UserProfile
            {
                FullName = "Siva Prakash",
                Email = "siva@example.com",
                PhoneNumber = "9876543210",
                DateOfBirth = new DateTime(2003, 5, 10),
                Address = "Madurai, Tamil Nadu",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            },
            new UserProfile
            {
                FullName = "Priya Sharma",
                Email = "priya@example.com",
                PhoneNumber = "9876543211",
                DateOfBirth = new DateTime(1998, 8, 15),
                Address = "Chennai, Tamil Nadu",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            },
            new UserProfile
            {
                FullName = "Rahul Kumar",
                Email = "rahul@example.com",
                PhoneNumber = "9876543212",
                DateOfBirth = new DateTime(2001, 11, 22),
                Address = "Bangalore, Karnataka",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            },
            new UserProfile
            {
                FullName = "Ananya Roy",
                Email = "ananya@example.com",
                PhoneNumber = "9876543213",
                DateOfBirth = new DateTime(2000, 3, 5),
                Address = "Mumbai, Maharashtra",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            },
            new UserProfile
            {
                FullName = "Karthik Raja",
                Email = "karthik@example.com",
                PhoneNumber = "9876543214",
                DateOfBirth = new DateTime(1999, 7, 12),
                Address = "Coimbatore, Tamil Nadu",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            }
        };

        foreach (var profile in defaultProfiles)
        {
            if (!existingEmails.Contains(profile.Email.ToLower()))
            {
                context.UserProfiles.Add(profile);
            }
        }

        context.SaveChanges();
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while initializing or seeding the database.");
    }
}

// Configure Middleware Pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

// Default Route: ProfileController -> Index Action
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Profile}/{action=Index}/{id?}");

app.Run();