using Microsoft.EntityFrameworkCore;
using Omail.Data.Models;

namespace Omail.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Organization> Organizations { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Section> Sections { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<EmailMessage> Emails { get; set; }
        public DbSet<EmailApproval> EmailApprovals { get; set; }
        public DbSet<EmailAttachment> EmailAttachments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Organization-Department relationship
            modelBuilder.Entity<Department>()
                .HasOne(d => d.Organization)
                .WithMany(o => o.Departments)
                .HasForeignKey(d => d.OrganizationId);

            // Department-Section relationship
            modelBuilder.Entity<Section>()
                .HasOne(s => s.Department)
                .WithMany(d => d.Sections)
                .HasForeignKey(s => s.DepartmentId);

            // Section-Employee relationship
            modelBuilder.Entity<Employee>()
                .HasOne(e => e.Section)
                .WithMany(s => s.Employees)
                .HasForeignKey(e => e.SectionId);

            // EmailMessage-Employee (sender) relationship
            modelBuilder.Entity<EmailMessage>()
                .HasOne(e => e.Sender)
                .WithMany(s => s.SentEmails)
                .HasForeignKey(e => e.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            // EmailRecipient relationships - explicitly define column names
            modelBuilder.Entity<EmailRecipient>()
                .HasKey(er => new { er.EmailId, er.RecipientId });

            modelBuilder.Entity<EmailRecipient>()
                .HasOne(er => er.Email)
                .WithMany(e => e.Recipients)
                .HasForeignKey(er => er.EmailId);

            modelBuilder.Entity<EmailRecipient>()
                .HasOne(er => er.Recipient)
                .WithMany(e => e.ReceivedEmails)
                .HasForeignKey(er => er.RecipientId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascade delete

            // EmailApproval relationships
            modelBuilder.Entity<EmailApproval>()
                .HasOne(a => a.Email)
                .WithMany(e => e.Approvals)
                .HasForeignKey(a => a.EmailId);

            modelBuilder.Entity<EmailApproval>()
                .HasOne(a => a.Approver)
                .WithMany()  // Don't use the PendingApprovals navigation property here
                .HasForeignKey(a => a.ApproverId)
                .OnDelete(DeleteBehavior.Restrict);

            // EmailAttachment relationship
            modelBuilder.Entity<EmailAttachment>()
                .HasOne(a => a.Email)
                .WithMany(e => e.Attachments)
                .HasForeignKey(a => a.EmailId);

            // Add seed data
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Seed organizations
            modelBuilder.Entity<Organization>().HasData(
                new Organization { 
                    Id = 1, 
                    Name = "Omail Corporation", 
                    Description = "Main organization" 
                }
            );

            // Seed departments
            modelBuilder.Entity<Department>().HasData(
                new Department { 
                    Id = 1, 
                    Name = "Information Technology", 
                    Description = "IT department responsible for software and hardware management",
                    OrganizationId = 1 
                },
                new Department { 
                    Id = 2, 
                    Name = "Human Resources", 
                    Description = "HR department responsible for employee management",
                    OrganizationId = 1 
                },
                new Department { 
                    Id = 3, 
                    Name = "Finance", 
                    Description = "Finance department responsible for financial management",
                    OrganizationId = 1 
                }
            );

            // Seed sections
            modelBuilder.Entity<Section>().HasData(
                new Section { 
                    Id = 1, 
                    Name = "Software Development", 
                    Description = "Develops and maintains software applications",
                    DepartmentId = 1 
                },
                new Section { 
                    Id = 2, 
                    Name = "Network Operations", 
                    Description = "Manages network infrastructure",
                    DepartmentId = 1 
                },
                new Section { 
                    Id = 3, 
                    Name = "Recruitment", 
                    Description = "Handles recruitment process",
                    DepartmentId = 2 
                },
                new Section { 
                    Id = 4, 
                    Name = "Accounting", 
                    Description = "Manages accounts and financial reporting",
                    DepartmentId = 3 
                }
            );

            // Add more seed data here if needed
        }
    }
}
