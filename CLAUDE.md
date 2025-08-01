# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Development Commands

### Building and Running
- `dotnet build` - Build the solution
- `dotnet run` - Run the application (uses https://localhost:7147 and http://localhost:5281)
- `dotnet run --project SteadyGrowth.Web.csproj` - Run from solution root

### Database Operations
- `dotnet ef migrations add <MigrationName>` - Create new EF migration
- `dotnet ef database update` - Apply pending migrations
- `dotnet ef migrations remove` - Remove last migration
- Database migrations are auto-applied on startup in Program.cs:219

### Package Management
- `dotnet restore` - Restore NuGet packages
- `dotnet add package <PackageName>` - Add new package

## Architecture Overview

### Technology Stack
- **Framework**: ASP.NET Core 8.0 with Razor Pages
- **Database**: SQL Server with Entity Framework Core
- **Authentication**: ASP.NET Core Identity with custom User entity
- **Patterns**: CQRS with MediatR, Repository pattern via EF Core
- **Logging**: Serilog with file and console outputs
- **Frontend**: Bootstrap, jQuery, TinyMCE for rich text

### Project Structure

#### Core Areas
- **Areas/Admin/**: Administrative interface with analytics, property approval, user management
- **Areas/Identity/**: Custom authentication pages (Login, Register, Password Reset)
- **Areas/Membership/**: Member dashboard with academy, properties, wallet, referrals

#### Application Layer (CQRS)
- **Application/Commands/**: Write operations organized by entity (Academy, Properties, Users, Wallet)
- **Application/Queries/**: Read operations with pagination support
- **Application/ViewModels/**: DTOs for data transfer

#### Data Layer
- **Data/ApplicationDbContext.cs**: Main EF context with all entity configurations
- **Models/Entities/**: Domain entities (User, Property, Course, Wallet, etc.)
- **Models/Enums/**: Shared enumerations (KYCStatus, DocumentType, etc.)

#### Services
- **Services/Interfaces/**: Service contracts
- **Services/Implementations/**: Business logic implementations
- All services registered as Scoped in Program.cs

### Key Entities and Relationships
- **User**: Extended IdentityUser with referral system, KYC status, wallet
- **Property**: Real estate listings with approval workflow and image gallery
- **Academy**: Course system with progress tracking and packages
- **Wallet**: Digital wallet with transactions and withdrawal requests
- **Referral**: Multi-level referral system with commission tracking

### Authentication & Authorization
- Three main user areas: Public, Member (authenticated), Admin (role-based)
- KYC requirement system with custom authorization handlers
- Cookie-based authentication with 14-day expiration
- Role-based access control for admin functions

### Database Configuration
- Precision decimal fields for financial data (18,2)
- Indexes on frequently queried fields (Status, UserId, CreatedAt)
- Soft deletes via status fields rather than hard deletes
- Referential integrity with appropriate cascade behaviors

### File Handling
- Property images stored in wwwroot/images/properties/
- Profile pictures in wwwroot/images/profilepictures/
- KYC documents in wwwroot/kycdocuments/
- 50MB file upload limit configured

### Important Implementation Notes
- Auto-migration on startup - be careful with production deployments
- Referral codes auto-generated on user registration
- Default admin seeding via SeedData.InitializeAsync()
- NGN currency formatting throughout the application
- Serilog rotating daily log files in Logs/ directory

### When Working with This Codebase
- Always use MediatR pattern for new commands/queries
- Follow existing entity-specific folder organization
- Use the custom User entity, not default IdentityUser
- Financial calculations require decimal precision (18,2)
- Test database changes carefully due to auto-migration
- New entities need DbSet registration in ApplicationDbContext.cs