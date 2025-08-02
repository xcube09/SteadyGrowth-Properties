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
            await SeedCourseSegmentsAsync(context);
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
                        TotalLessons = 3,
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
                        TotalLessons = 4,
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
                        TotalLessons = 3,
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
                        TotalLessons = 3,
                        AcademyPackageId = premiumPackage.Id
                    }
                };
                context.Courses.AddRange(courses);
                await context.SaveChangesAsync();
                Console.WriteLine("Seeded Courses.");
            }
        }

        private static async Task SeedCourseSegmentsAsync(ApplicationDbContext context)
        {
            // Only seed segments if courses exist and no segments exist yet
            if (!context.Courses.Any() || context.CourseSegments.Any())
            {
                return;
            }

            var courses = await context.Courses.ToListAsync();
            var allSegments = new List<CourseSegment>();

            foreach (var course in courses)
            {
                var segments = new List<CourseSegment>();

                switch (course.Title)
                {
                    case "Introduction to Real Estate":
                        segments = new List<CourseSegment>
                        {
                            new CourseSegment
                            {
                                CourseId = course.Id,
                                Title = "Real Estate Market Overview",
                                Content = "<h3>Understanding the Real Estate Market</h3><p>The real estate market is a complex ecosystem involving buyers, sellers, investors, and professionals. In this segment, we'll explore the fundamental concepts that drive real estate values and market dynamics.</p><ul><li>Supply and demand factors</li><li>Market cycles and trends</li><li>Economic indicators affecting real estate</li><li>Regional vs national market differences</li></ul><p>By understanding these core principles, you'll be better equipped to make informed decisions in your real estate journey.</p>",
                                VideoUrl = "https://www.youtube.com/watch?v=dQw4w9WgXcQ",
                                Order = 1,
                                IsActive = true,
                                CreatedAt = DateTime.UtcNow,
                                UpdatedAt = DateTime.UtcNow
                            },
                            new CourseSegment
                            {
                                CourseId = course.Id,
                                Title = "Property Types and Investment Strategies",
                                Content = "<h3>Different Property Types for Investment</h3><p>Real estate offers various investment opportunities, each with unique characteristics and potential returns.</p><h4>Residential Properties</h4><ul><li>Single-family homes</li><li>Apartments and condominiums</li><li>Multi-family properties</li></ul><h4>Commercial Properties</h4><ul><li>Office buildings</li><li>Retail spaces</li><li>Industrial properties</li></ul><p>We'll also cover investment strategies like buy-and-hold, fix-and-flip, and real estate investment trusts (REITs).</p>",
                                VideoUrl = "https://www.youtube.com/watch?v=abc123defgh",
                                Order = 2,
                                IsActive = true,
                                CreatedAt = DateTime.UtcNow,
                                UpdatedAt = DateTime.UtcNow
                            },
                            new CourseSegment
                            {
                                CourseId = course.Id,
                                Title = "Financing and Investment Analysis",
                                Content = "<h3>Understanding Real Estate Financing</h3><p>Learn about different financing options and how to analyze potential investments.</p><h4>Financing Options</h4><ul><li>Traditional mortgages</li><li>Hard money loans</li><li>Private funding</li><li>Partnership structures</li></ul><h4>Investment Analysis</h4><ul><li>Cash flow calculations</li><li>Return on investment (ROI)</li><li>Cap rates and cash-on-cash returns</li><li>Risk assessment</li></ul><p>These tools will help you evaluate whether a property is a good investment opportunity.</p>",
                                VideoUrl = "https://www.youtube.com/watch?v=xyz789uvwxy",
                                Order = 3,
                                IsActive = true,
                                CreatedAt = DateTime.UtcNow,
                                UpdatedAt = DateTime.UtcNow
                            }
                        };
                        break;

                    case "Advanced Property Valuation":
                        segments = new List<CourseSegment>
                        {
                            new CourseSegment
                            {
                                CourseId = course.Id,
                                Title = "Comparative Market Analysis (CMA)",
                                Content = "<h3>Mastering Comparative Market Analysis</h3><p>A Comparative Market Analysis is the foundation of accurate property valuation. Learn how to identify and analyze comparable properties to determine fair market value.</p><h4>Key Components of CMA</h4><ul><li>Selecting appropriate comparables</li><li>Adjusting for property differences</li><li>Market conditions and timing</li><li>Location factors and neighborhood analysis</li></ul><p>We'll walk through real examples and teach you to use professional tools and databases for comprehensive market research.</p>",
                                VideoUrl = "https://www.youtube.com/watch?v=cma123analysis",
                                Order = 1,
                                IsActive = true,
                                CreatedAt = DateTime.UtcNow,
                                UpdatedAt = DateTime.UtcNow
                            },
                            new CourseSegment
                            {
                                CourseId = course.Id,
                                Title = "Income Approach Valuation",
                                Content = "<h3>Income-Based Property Valuation</h3><p>For investment properties, the income approach provides crucial insights into a property's value based on its income-generating potential.</p><h4>Key Concepts</h4><ul><li>Gross Rent Multiplier (GRM)</li><li>Capitalization rates</li><li>Net Operating Income (NOI)</li><li>Discounted Cash Flow analysis</li></ul><h4>Practical Applications</h4><p>Learn to calculate these metrics and understand when to use each method. We'll cover how to project future income streams and account for vacancy rates, maintenance costs, and market fluctuations.</p>",
                                VideoUrl = "https://www.youtube.com/watch?v=income456valuation",
                                Order = 2,
                                IsActive = true,
                                CreatedAt = DateTime.UtcNow,
                                UpdatedAt = DateTime.UtcNow
                            },
                            new CourseSegment
                            {
                                CourseId = course.Id,
                                Title = "Cost Approach and Market Adjustments",
                                Content = "<h3>Cost Approach Method</h3><p>The cost approach estimates property value based on the cost to replace or reproduce the improvements, minus depreciation, plus land value.</p><h4>Components</h4><ul><li>Replacement cost vs reproduction cost</li><li>Physical, functional, and economic depreciation</li><li>Land valuation techniques</li><li>Construction cost estimation</li></ul><h4>Market Adjustments</h4><p>Learn advanced techniques for making accurate adjustments based on market conditions, property characteristics, and external factors that affect value.</p>",
                                VideoUrl = "https://www.youtube.com/watch?v=cost789approach",
                                Order = 3,
                                IsActive = true,
                                CreatedAt = DateTime.UtcNow,
                                UpdatedAt = DateTime.UtcNow
                            },
                            new CourseSegment
                            {
                                CourseId = course.Id,
                                Title = "Advanced Valuation Software and Technology",
                                Content = "<h3>Technology in Modern Valuation</h3><p>Explore cutting-edge tools and software that are revolutionizing property valuation in today's market.</p><h4>Valuation Software</h4><ul><li>Automated Valuation Models (AVMs)</li><li>Professional appraisal software</li><li>GIS mapping and analysis tools</li><li>Market data platforms</li></ul><h4>Emerging Technologies</h4><ul><li>AI and machine learning in valuation</li><li>Drone technology for property assessment</li><li>3D modeling and virtual tours</li><li>Big data analytics</li></ul><p>Stay ahead of the curve by understanding how technology is changing the valuation landscape.</p>",
                                VideoUrl = "https://www.youtube.com/watch?v=tech101valuation",
                                Order = 4,
                                IsActive = true,
                                CreatedAt = DateTime.UtcNow,
                                UpdatedAt = DateTime.UtcNow
                            }
                        };
                        break;

                    case "Real Estate Law Fundamentals":
                        segments = new List<CourseSegment>
                        {
                            new CourseSegment
                            {
                                CourseId = course.Id,
                                Title = "Property Rights and Ownership Types",
                                Content = "<h3>Understanding Property Rights</h3><p>Property ownership involves a bundle of rights that can be complex and varied. Understanding these rights is crucial for any real estate transaction.</p><h4>Types of Ownership</h4><ul><li>Fee Simple Absolute</li><li>Life Estate</li><li>Joint Tenancy</li><li>Tenancy in Common</li><li>Community Property</li></ul><h4>Property Rights</h4><ul><li>Right to use and occupy</li><li>Right to transfer or sell</li><li>Right to exclude others</li><li>Mineral and air rights</li></ul><p>We'll examine how these different ownership structures affect buying, selling, and investing in real estate.</p>",
                                VideoUrl = "https://www.youtube.com/watch?v=rights123property",
                                Order = 1,
                                IsActive = true,
                                CreatedAt = DateTime.UtcNow,
                                UpdatedAt = DateTime.UtcNow
                            },
                            new CourseSegment
                            {
                                CourseId = course.Id,
                                Title = "Contracts and Transaction Documentation",
                                Content = "<h3>Real Estate Contracts and Legal Documentation</h3><p>Every real estate transaction involves multiple contracts and legal documents. Understanding these is essential for protecting your interests.</p><h4>Key Documents</h4><ul><li>Purchase agreements</li><li>Listing agreements</li><li>Disclosure statements</li><li>Title documents</li><li>Lease agreements</li></ul><h4>Contract Elements</h4><ul><li>Offer and acceptance</li><li>Consideration</li><li>Legal capacity</li><li>Contingencies and conditions</li></ul><p>Learn to read, understand, and negotiate these critical documents effectively.</p>",
                                VideoUrl = "https://www.youtube.com/watch?v=contracts456legal",
                                Order = 2,
                                IsActive = true,
                                CreatedAt = DateTime.UtcNow,
                                UpdatedAt = DateTime.UtcNow
                            },
                            new CourseSegment
                            {
                                CourseId = course.Id,
                                Title = "Closing Process and Title Issues",
                                Content = "<h3>The Real Estate Closing Process</h3><p>The closing process is where ownership officially transfers. Understanding this process helps ensure smooth transactions and avoid common pitfalls.</p><h4>Closing Steps</h4><ul><li>Title search and insurance</li><li>Property inspections</li><li>Loan approval and documentation</li><li>Final walk-through</li><li>Settlement statement review</li></ul><h4>Common Title Issues</h4><ul><li>Liens and encumbrances</li><li>Easements and restrictions</li><li>Survey disputes</li><li>Title defects and cures</li></ul><p>We'll cover how to identify and resolve these issues before they become major problems.</p>",
                                VideoUrl = "https://www.youtube.com/watch?v=closing789title",
                                Order = 3,
                                IsActive = true,
                                CreatedAt = DateTime.UtcNow,
                                UpdatedAt = DateTime.UtcNow
                            }
                        };
                        break;

                    case "Real Estate Marketing Strategies":
                        segments = new List<CourseSegment>
                        {
                            new CourseSegment
                            {
                                CourseId = course.Id,
                                Title = "Digital Marketing for Real Estate",
                                Content = "<h3>Leveraging Digital Platforms</h3><p>In today's market, successful real estate marketing requires a strong digital presence across multiple platforms.</p><h4>Online Platforms</h4><ul><li>MLS systems and syndication</li><li>Real estate websites and portals</li><li>Social media marketing</li><li>Google Ads and SEO</li><li>Email marketing campaigns</li></ul><h4>Content Creation</h4><ul><li>Professional photography</li><li>Virtual tours and video</li><li>Property descriptions that sell</li><li>Blog content and market updates</li></ul><p>Learn to create compelling digital marketing campaigns that attract qualified buyers and sellers.</p>",
                                VideoUrl = "https://www.youtube.com/watch?v=digital123marketing",
                                Order = 1,
                                IsActive = true,
                                CreatedAt = DateTime.UtcNow,
                                UpdatedAt = DateTime.UtcNow
                            },
                            new CourseSegment
                            {
                                CourseId = course.Id,
                                Title = "Traditional Marketing and Networking",
                                Content = "<h3>Traditional Marketing That Still Works</h3><p>While digital marketing is crucial, traditional methods remain important for building lasting relationships and local market presence.</p><h4>Traditional Methods</h4><ul><li>Print advertising and direct mail</li><li>Open houses and broker tours</li><li>Yard signs and property flyers</li><li>Referral programs</li></ul><h4>Networking Strategies</h4><ul><li>Building professional relationships</li><li>Community involvement</li><li>Industry associations and events</li><li>Past client cultivation</li></ul><p>Discover how to integrate traditional and digital approaches for maximum market impact.</p>",
                                VideoUrl = "https://www.youtube.com/watch?v=traditional456network",
                                Order = 2,
                                IsActive = true,
                                CreatedAt = DateTime.UtcNow,
                                UpdatedAt = DateTime.UtcNow
                            },
                            new CourseSegment
                            {
                                CourseId = course.Id,
                                Title = "Pricing Strategies and Market Positioning",
                                Content = "<h3>Strategic Pricing for Maximum Results</h3><p>Proper pricing strategy can make the difference between a property that sells quickly and one that sits on the market.</p><h4>Pricing Strategies</h4><ul><li>Competitive market pricing</li><li>Strategic overpricing and underpricing</li><li>Price reduction strategies</li><li>Market timing considerations</li></ul><h4>Market Positioning</h4><ul><li>Target audience identification</li><li>Unique selling propositions</li><li>Competition analysis</li><li>Value-added services</li></ul><p>Learn to position properties effectively in their respective markets for optimal results.</p>",
                                VideoUrl = "https://www.youtube.com/watch?v=pricing789strategy",
                                Order = 3,
                                IsActive = true,
                                CreatedAt = DateTime.UtcNow,
                                UpdatedAt = DateTime.UtcNow
                            }
                        };
                        break;
                }

                allSegments.AddRange(segments);
            }

            if (allSegments.Any())
            {
                context.CourseSegments.AddRange(allSegments);
                await context.SaveChangesAsync();
                Console.WriteLine($"Seeded {allSegments.Count} course segments across {courses.Count} courses.");
            }
        }
    }
}
