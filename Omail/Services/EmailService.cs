using Microsoft.EntityFrameworkCore;
using Omail.Data;
using Omail.Data.Models;

namespace Omail.Services
{
    public class EmailService
    {
        private readonly AppDbContext _context;
        private readonly AuthService _authService;
        private readonly ServiceFactory _serviceFactory;

        public EmailService(
            AppDbContext context, 
            AuthService authService,
            ServiceFactory serviceFactory)
        {
            _context = context;
            _authService = authService;
            _serviceFactory = serviceFactory;
        }

        public async Task<List<EmailMessage>> GetInboxAsync(int? limit = null)
        {
            var currentUser = await _authService.GetCurrentUserAsync();
            if (currentUser == null)
                return new List<EmailMessage>();

            IQueryable<EmailMessage> query = _context.Emails
                .Include(e => e.Sender)
                .Include(e => e.Recipients)
                .ThenInclude(r => r.Recipient)
                .Include(e => e.Attachments)
                .Where(e => e.Recipients.Any(r => r.RecipientId == currentUser.Id && r.Type == RecipientType.To) &&
                           !e.IsDeleted &&
                           e.Status == EmailStatus.Sent)
                .OrderByDescending(e => e.SentAt);

            if (limit.HasValue)
                query = query.Take(limit.Value);

            return await query.ToListAsync();
        }

        public async Task<List<EmailMessage>> GetSentEmailsAsync(int? limit = null)
        {
            var currentUser = await _authService.GetCurrentUserAsync();
            if (currentUser == null)
                return new List<EmailMessage>();

            IQueryable<EmailMessage> query = _context.Emails
                .Include(e => e.Recipients)
                .ThenInclude(r => r.Recipient)
                .Include(e => e.Attachments)
                .Where(e => e.SenderId == currentUser.Id &&
                           !e.IsDeleted &&
                           e.Status == EmailStatus.Sent)
                .OrderByDescending(e => e.SentAt);

            if (limit.HasValue)
                query = query.Take(limit.Value);

            return await query.ToListAsync();
        }

        public async Task<List<EmailMessage>> GetDraftsAsync(int? limit = null)
        {
            var currentUser = await _authService.GetCurrentUserAsync();
            if (currentUser == null)
                return new List<EmailMessage>();

            IQueryable<EmailMessage> query = _context.Emails
                .Include(e => e.Recipients)
                .ThenInclude(r => r.Recipient)
                .Include(e => e.Attachments)
                .Where(e => e.SenderId == currentUser.Id &&
                           !e.IsDeleted &&
                           e.Status == EmailStatus.Draft)
                .OrderByDescending(e => e.CreatedAt);

            if (limit.HasValue)
                query = query.Take(limit.Value);

            return await query.ToListAsync();
        }

        public async Task<List<EmailMessage>> GetTrashAsync(int? limit = null)
        {
            var currentUser = await _authService.GetCurrentUserAsync();
            if (currentUser == null)
                return new List<EmailMessage>();

            IQueryable<EmailMessage> query = _context.Emails
                .Include(e => e.Sender)
                .Include(e => e.Recipients)
                .ThenInclude(r => r.Recipient)
                .Include(e => e.Attachments)
                .Where(e => (e.SenderId == currentUser.Id || 
                           e.Recipients.Any(r => r.RecipientId == currentUser.Id)) &&
                           e.IsDeleted)
                .OrderByDescending(e => e.DeletedAt);

            if (limit.HasValue)
                query = query.Take(limit.Value);

            return await query.ToListAsync();
        }

        public async Task<EmailMessage> GetEmailByIdAsync(int id)
        {
            var currentUser = await _authService.GetCurrentUserAsync();
            if (currentUser == null)
                return null;

            var email = await _context.Emails
                .Include(e => e.Sender)
                .Include(e => e.Recipients)
                .ThenInclude(r => r.Recipient)
                .Include(e => e.Attachments)
                .Include(e => e.Approvals)
                .ThenInclude(a => a.Approver)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (email == null)
                return null;

            // Mark as read if current user is a recipient
            var recipient = email.Recipients.FirstOrDefault(r => r.RecipientId == currentUser.Id);
            if (recipient != null && !recipient.IsRead)
            {
                recipient.IsRead = true;
                recipient.ReadAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            return email;
        }

        public async Task<EmailMessage> SaveDraftAsync(EmailMessage email, List<int> recipientIds, List<int> ccIds = null, List<int> bccIds = null)
        {
            var currentUser = await _authService.GetCurrentUserAsync();
            if (currentUser == null)
                throw new UnauthorizedAccessException("User not authenticated");

            email.Status = EmailStatus.Draft;
            email.SenderId = currentUser.Id;

            if (email.Id == 0) // New draft
            {
                _context.Emails.Add(email);
            }
            else // Update existing draft
            {
                // Remove existing recipients
                var existingRecipients = await _context.Set<EmailRecipient>()
                    .Where(r => r.EmailId == email.Id)
                    .ToListAsync();
                
                _context.Set<EmailRecipient>().RemoveRange(existingRecipients);
            }

            await _context.SaveChangesAsync();

            // Add recipients
            await AddRecipientsAsync(email.Id, recipientIds, RecipientType.To);
            
            if (ccIds != null && ccIds.Any())
                await AddRecipientsAsync(email.Id, ccIds, RecipientType.Cc);
            
            if (bccIds != null && bccIds.Any())
                await AddRecipientsAsync(email.Id, bccIds, RecipientType.Bcc);

            await _context.SaveChangesAsync();
            return email;
        }

        public async Task<EmailMessage> SendEmailAsync(EmailMessage email, List<int> recipientIds, List<int> ccIds = null, List<int> bccIds = null)
        {
            var currentUser = await _authService.GetCurrentUserAsync();
            if (currentUser == null)
                throw new UnauthorizedAccessException("User not authenticated");

            // Check if any recipients are in a different department and require approval
            bool requiresApproval = false;
            var allRecipientIds = new List<int>(recipientIds);
            if (ccIds != null) allRecipientIds.AddRange(ccIds);
            if (bccIds != null) allRecipientIds.AddRange(bccIds);
            
            var recipients = await _context.Employees
                .Include(e => e.Section)
                .ThenInclude(s => s.Department)
                .Where(e => allRecipientIds.Contains(e.Id))
                .ToListAsync();
            
            var senderDepartment = currentUser.Section.DepartmentId;
            foreach (var recipient in recipients)
            {
                if (recipient.Section.DepartmentId != senderDepartment)
                {
                    requiresApproval = true;
                    break;
                }
            }

            email.SenderId = currentUser.Id;
            
            if (requiresApproval)
            {
                email.RequiresDepartmentalApproval = true;
                email.Status = EmailStatus.PendingApproval;
            }
            else
            {
                email.Status = EmailStatus.Sent;
                email.SentAt = DateTime.UtcNow;
            }

            if (email.Id == 0) // New email
            {
                _context.Emails.Add(email);
            }
            else // Update existing draft
            {
                // Remove existing recipients
                var existingRecipients = await _context.Set<EmailRecipient>()
                    .Where(r => r.EmailId == email.Id)
                    .ToListAsync();
                
                _context.Set<EmailRecipient>().RemoveRange(existingRecipients);
            }

            await _context.SaveChangesAsync();

            // Add recipients
            await AddRecipientsAsync(email.Id, recipientIds, RecipientType.To);
            
            if (ccIds != null && ccIds.Any())
                await AddRecipientsAsync(email.Id, ccIds, RecipientType.Cc);
            
            if (bccIds != null && bccIds.Any())
                await AddRecipientsAsync(email.Id, bccIds, RecipientType.Bcc);

            // If approval is required, create an approval request
            if (requiresApproval)
            {
                // Find department manager
                var manager = await _context.Employees
                    .FirstOrDefaultAsync(e => e.SectionId == currentUser.SectionId && e.IsManager);
                
                if (manager != null)
                {
                    // Get ApprovalService through the factory to avoid circular dependency
                    var approvalService = _serviceFactory.GetService<ApprovalService>();
                    await approvalService.CreateApprovalAsync(email.Id, manager.Id);
                }
            }

            await _context.SaveChangesAsync();
            return email;
        }

        public async Task MoveToTrashAsync(int emailId)
        {
            var currentUser = await _authService.GetCurrentUserAsync();
            if (currentUser == null)
                throw new UnauthorizedAccessException("User not authenticated");

            var email = await _context.Emails.FindAsync(emailId);
            if (email == null)
                throw new KeyNotFoundException("Email not found");

            // Check if user has access to this email
            if (email.SenderId != currentUser.Id && 
                !await _context.Set<EmailRecipient>().AnyAsync(r => r.EmailId == emailId && r.RecipientId == currentUser.Id))
            {
                throw new UnauthorizedAccessException("You don't have access to this email");
            }

            email.IsDeleted = true;
            email.DeletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        public async Task RestoreFromTrashAsync(int emailId)
        {
            var currentUser = await _authService.GetCurrentUserAsync();
            if (currentUser == null)
                throw new UnauthorizedAccessException("User not authenticated");

            var email = await _context.Emails.FindAsync(emailId);
            if (email == null)
                throw new KeyNotFoundException("Email not found");

            // Check if user has access to this email
            if (email.SenderId != currentUser.Id && 
                !await _context.Set<EmailRecipient>().AnyAsync(r => r.EmailId == emailId && r.RecipientId == currentUser.Id))
            {
                throw new UnauthorizedAccessException("You don't have access to this email");
            }

            email.IsDeleted = false;
            email.DeletedAt = null;
            await _context.SaveChangesAsync();
        }

        public async Task PermanentlyDeleteAsync(int emailId)
        {
            var currentUser = await _authService.GetCurrentUserAsync();
            if (currentUser == null)
                throw new UnauthorizedAccessException("User not authenticated");

            var email = await _context.Emails.FindAsync(emailId);
            if (email == null)
                throw new KeyNotFoundException("Email not found");

            // Check if user has access to this email
            if (email.SenderId != currentUser.Id)
            {
                throw new UnauthorizedAccessException("You don't have access to this email");
            }

            _context.Emails.Remove(email);
            await _context.SaveChangesAsync();
        }

        private async Task AddRecipientsAsync(int emailId, List<int> recipientIds, RecipientType type)
        {
            foreach (var recipientId in recipientIds)
            {
                var recipient = new EmailRecipient
                {
                    EmailId = emailId,
                    RecipientId = recipientId,
                    Type = type
                };
                
                _context.Set<EmailRecipient>().Add(recipient);
            }
            
            await _context.SaveChangesAsync();
        }
    }
}
