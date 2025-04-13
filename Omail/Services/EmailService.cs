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
        private ILogger<EmailService> _logger;

        public EmailService(
            AppDbContext context, 
            AuthService authService,
            ServiceFactory serviceFactory,
            ILogger<EmailService> logger)
        {
            _context = context;
            _authService = authService;
            _serviceFactory = serviceFactory;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
            try
            {
                return await _context.Emails
                    .Include(e => e.Sender)
                    .Include(e => e.Recipients)
                        .ThenInclude(r => r.Recipient)
                    .Include(e => e.Attachments)
                    .Include(e => e.Approvals)
                        .ThenInclude(a => a.Approver)
                    .FirstOrDefaultAsync(e => e.Id == id);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error retrieving email with ID {EmailId}", id);
                throw;
            }
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
                var existingRecipients = await _context.Set<EmailRecipient>()
                    .Where(r => r.EmailId == email.Id)
                    .ToListAsync();
                
                _context.Set<EmailRecipient>().RemoveRange(existingRecipients);
            }

            await _context.SaveChangesAsync();

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
                var existingRecipients = await _context.Set<EmailRecipient>()
                    .Where(r => r.EmailId == email.Id)
                    .ToListAsync();
                
                _context.Set<EmailRecipient>().RemoveRange(existingRecipients);
            }

            await _context.SaveChangesAsync();

            await AddRecipientsAsync(email.Id, recipientIds, RecipientType.To);
            
            if (ccIds != null && ccIds.Any())
                await AddRecipientsAsync(email.Id, ccIds, RecipientType.Cc);
            
            if (bccIds != null && bccIds.Any())
                await AddRecipientsAsync(email.Id, bccIds, RecipientType.Bcc);

            if (requiresApproval)
            {
                var manager = await _context.Employees
                    .FirstOrDefaultAsync(e => e.SectionId == currentUser.SectionId && e.IsManager);
                
                if (manager != null)
                {
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

            if (email.SenderId != currentUser.Id)
            {
                throw new UnauthorizedAccessException("You don't have access to this email");
            }

            _context.Emails.Remove(email);
            await _context.SaveChangesAsync();
        }

        public async Task<int> GetTotalEmailCountAsync()
        {
            return await _context.Emails.CountAsync();
        }

        public async Task<List<EmailMessage>> GetRecentEmailsAsync(int count = 5)
        {
            return await _context.Emails
                .Include(e => e.Sender)
                .Include(e => e.Recipients)
                    .ThenInclude(r => r.Recipient)
                .OrderByDescending(e => e.CreatedAt)
                .Take(count)
                .ToListAsync();
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

        /// <summary>
        /// Gets email statistics for a specific user
        /// </summary>
        /// <param name="userId">The user ID to retrieve stats for</param>
        /// <returns>Email statistics for the user</returns>
        public async Task<EmailStatsModel> GetUserEmailStatsAsync(int userId)
        {
            try
            {
                var result = new EmailStatsModel();
                
                var sentEmails = await _context.Emails
                    .Where(e => e.SenderId == userId && !e.IsDeleted && e.Status == EmailStatus.Sent)
                    .CountAsync();
                
                var receivedEmails = await _context.Emails
                    .Include(e => e.Recipients)
                    .Where(e => e.Recipients.Any(r => r.RecipientId == userId) && !e.IsDeleted)
                    .CountAsync();
                
                result.SentEmails = sentEmails;
                result.ReceivedEmails = receivedEmails;
                result.TotalEmails = sentEmails + receivedEmails;
                
                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting user email statistics");
                throw;
            }
        }

        /// <summary>
        /// Gets recent emails associated with a user
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <param name="count">Maximum number of emails to retrieve</param>
        /// <returns>List of recent emails</returns>
        public async Task<List<EmailItemModel>> GetRecentUserEmailsAsync(int userId, int count = 5)
        {
            try
            {
                var result = new List<EmailItemModel>();
                
                var emails = await _context.Emails
                    .Include(e => e.Sender)
                    .Include(e => e.Recipients)
                        .ThenInclude(r => r.Recipient)
                    .Include(e => e.Attachments)
                    .Where(e => 
                        (e.SenderId == userId || e.Recipients.Any(r => r.RecipientId == userId)) 
                        && !e.IsDeleted)
                    .OrderByDescending(e => e.SentAt ?? e.CreatedAt)
                    .Take(count)
                    .ToListAsync();
                
                foreach (var email in emails)
                {
                    var recipient = email.Recipients.FirstOrDefault()?.Recipient;
                    bool isSender = email.SenderId == userId;
                    
                    var item = new EmailItemModel
                    {
                        Id = email.Id,
                        Subject = email.Subject,
                        Preview = TruncateBody(email.Body, 100),
                        Sender = isSender ? "Me" : email.Sender?.FullName ?? "Unknown",
                        Recipient = isSender ? recipient?.FullName ?? "Unknown" : "Me",
                        IsSender = isSender,
                        Date = email.SentAt ?? email.CreatedAt,
                        HasAttachments = email.Attachments.Any()
                    };
                    
                    result.Add(item);
                }
                
                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting recent user emails");
                throw;
            }
        }

        private string TruncateBody(string body, int maxLength)
        {
            var text = System.Text.RegularExpressions.Regex.Replace(body ?? "", "<[^>]*>", "");
            
            if (text.Length <= maxLength)
                return text;
                
            return text.Substring(0, maxLength) + "...";
        }

        public class EmailStatsModel
        {
            public int TotalEmails { get; set; }
            public int SentEmails { get; set; }
            public int ReceivedEmails { get; set; }
        }

        public class EmailItemModel
        {
            public int Id { get; set; }
            public string Subject { get; set; }
            public string Preview { get; set; }
            public string Sender { get; set; }
            public string Recipient { get; set; }
            public bool IsSender { get; set; }
            public DateTime Date { get; set; }
            public bool HasAttachments { get; set; }
        }
    }
}
