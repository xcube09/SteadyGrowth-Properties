using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SteadyGrowth.Web.Models.Entities;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SteadyGrowth.Web.Data
{
    public static class SeedData
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            context.Database.Migrate();

            var adminUser = await SeedUsersAndRolesAsync(context, userManager, roleManager);
            await SeedPropertiesAsync(context, adminUser);
            await SeedAcademyPackagesAndCoursesAsync(context);
        }

        private static async Task<User> SeedUsersAndRolesAsync(ApplicationDbContext context, UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
        {
            // Seed admin role
            var adminRole = "Admin";
            if (!await roleManager.RoleExistsAsync(adminRole))
            {
                await roleManager.CreateAsync(new IdentityRole(adminRole));
            }

            // Seed admin user
            var adminEmail = "admin@steadygrowth.com";
            var adminPassword = "Admin!2345678"; // <-- Set static password for initial admin
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new User
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    FirstName = "Admin",
                    LastName = "User"
                };
                var result = await userManager.CreateAsync(adminUser, adminPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, adminRole);
                    Console.WriteLine($"Seeded admin user: {adminEmail} with password: {adminPassword}");
                }
                else
                {
                    Console.WriteLine($"Failed to seed admin user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
            else
            {
                // Ensure user is in admin role
                if (!await userManager.IsInRoleAsync(adminUser, adminRole))
                {
                    await userManager.AddToRoleAsync(adminUser, adminRole);
                }
            }
            return adminUser;
        }

        private static async Task SeedPropertiesAsync(ApplicationDbContext context, User adminUser)
        {
            // Seed showcase properties for landing page
            if (!context.Properties.Any())
            {
                var showcaseProperties = new[]
                {
                    new Property
                    {
                        Title = "Modern Family Home in Lekki",
                        Description = "A beautiful, fully-furnished 4-bedroom family home with a private pool and garden. Perfect for modern living in the heart of Lekki.",
                        Price = 120_000_000,
                        Location = "Lekki, Lagos",
                        PropertyType = PropertyType.Residential,
                        Status = PropertyStatus.Approved,
                        UserId = adminUser.Id,
                        CreatedAt = DateTime.UtcNow.AddDays(-10),
                        ApprovedAt = DateTime.UtcNow.AddDays(-9),
                        Features = "[\"4 Bedrooms\",\"3 Bathrooms\",\"Swimming Pool\",\"Garden\",\"Parking\"]",
                        IsActive = true,
                        PropertyImages = new[]
                        {
                            new PropertyImage { FileName = "/images/properties/lekki-home-1.jpg", Caption = "Front View", ImageType = "Exterior", DisplayOrder = 1 },
                            new PropertyImage { FileName = "/images/properties/lekki-home-2.jpg", Caption = "Living Room", ImageType = "Interior", DisplayOrder = 2 }
                        }
                    },
                    new Property
                    {
                        Title = "Luxury Office Space in Victoria Island",
                        Description = "Premium commercial office space with ocean views, 24/7 security, and modern amenities. Ideal for growing businesses.",
                        Price = 350_000_000,
                        Location = "Victoria Island, Lagos",
                        PropertyType = PropertyType.Commercial,
                        Status = PropertyStatus.Approved,
                        UserId = adminUser.Id,
                        CreatedAt = DateTime.UtcNow.AddDays(-20),
                        ApprovedAt = DateTime.UtcNow.AddDays(-19),
                        Features = "[\"Open Plan\",\"Ocean View\",\"24/7 Security\",\"Parking\"]",
                        IsActive = true,
                        PropertyImages = new[]
                        {
                            new PropertyImage { FileName = "/images/properties/victoria-office-1.jpg", Caption = "Building Exterior", ImageType = "Exterior", DisplayOrder = 1 },
                            new PropertyImage { FileName = "/images/properties/victoria-office-2.jpg", Caption = "Office Interior", ImageType = "Interior", DisplayOrder = 2 }
                        }
                    },
                    new Property
                    {
                        Title = "Serviced Land Plot in Ajah",
                        Description = "A 600sqm serviced land plot in a gated estate, ready for development. Secure your investment today!",
                        Price = 30_000_000,
                        Location = "Ajah, Lagos",
                        PropertyType = PropertyType.Land,
                        Status = PropertyStatus.Approved,
                        UserId = adminUser.Id,
                        CreatedAt = DateTime.UtcNow.AddDays(-5),
                        ApprovedAt = DateTime.UtcNow.AddDays(-4),
                        Features = "[\"600sqm\",\"Gated Estate\",\"Title: C of O\"]",
                        IsActive = true,
                        PropertyImages = new[]
                        {
                            new PropertyImage { FileName = "/images/properties/ajah-land-1.jpg", Caption = "Land Plot", ImageType = "Land", DisplayOrder = 1 }
                        }
                    }
                };
                context.Properties.AddRange(showcaseProperties);
                await context.SaveChangesAsync();
                Console.WriteLine("Seeded showcase properties for landing page with images.");
            }
        }

        private static async Task SeedAcademyPackagesAndCoursesAsync(ApplicationDbContext context)
        {
            AcademyPackage basicPackage;
            AcademyPackage premiumPackage;

            // Seed Academy Packages
            if (!context.AcademyPackages.Any())
            {
                basicPackage = new AcademyPackage
                {
                    Name = "Basic Package",
                    Description = "Access to fundamental real estate courses.",
                    Price = 0.00m,
                    IsActive = true
                };
                premiumPackage = new AcademyPackage
                {
                    Name = "Premium Package",
                    Description = "Full access to all real estate academy courses and advanced materials.",
                    Price = 50000.00m,
                    IsActive = true
                };
                context.AcademyPackages.AddRange(basicPackage, premiumPackage);
                await context.SaveChangesAsync();
                Console.WriteLine("Seeded Academy Packages.");
            }
            else
            {
                basicPackage = await context.AcademyPackages.FirstAsync(p => p.Name == "Basic Package");
                premiumPackage = await context.AcademyPackages.FirstAsync(p => p.Name == "Premium Package");
            }

            // Seed Courses
            if (!context.Courses.Any())
            {
                var courses = new[]
                {
                    new Course
                    {
                        Title = "Introduction to Real Estate",
                        Description = "Learn the basics of real estate investment and market analysis.",
                        Content = "Course content for Introduction to Real Estate.",
                        Duration = 60,
                        Order = 1,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        TotalLessons = 5,
                        AcademyPackageId = basicPackage.Id
                    },
                    new Course
                    {
                        Title = "Advanced Property Valuation",
                        Description = "Deep dive into advanced valuation techniques and financial modeling.",
                        Content = "Course content for Advanced Property Valuation.",
                        Duration = 120,
                        Order = 2,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        TotalLessons = 10,
                        AcademyPackageId = premiumPackage.Id
                    },
                    new Course
                    {
                        Title = "Real Estate Law Fundamentals",
                        Description = "Understand the legal aspects of property ownership and transactions.",
                        Content = "Course content for Real Estate Law Fundamentals.",
                        Duration = 90,
                        Order = 3,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        TotalLessons = 7,
                        AcademyPackageId = basicPackage.Id
                    },
                    new Course
                    {
                        Title = "Real Estate Marketing Strategies",
                        Description = "Effective strategies for marketing properties in a competitive market.",
                        Content = "Course content for Real Estate Marketing Strategies.",
                        Duration = 75,
                        Order = 4,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        TotalLessons = 6,
                        AcademyPackageId = premiumPackage.Id
                    }
                };
                context.Courses.AddRange(courses);
                await context.SaveChangesAsync();
                Console.WriteLine("Seeded Courses.");
            }
        }
    }
}
