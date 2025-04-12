using Microsoft.EntityFrameworkCore;
using Omail.Data;
using Omail.Data.Models;

namespace Omail.Services
{
    public class ApprovalService
    {
        private readonly AppDbContext _context;
        private readonly AuthService _authService;
        private readonly EmailService _emailService;

        public ApprovalService(AppDbContext context, AuthService authService, ServiceFactory serviceFactory)
        {
            _context = context;
            _authService = authService;
            _emailService = serviceFactory.GetService<EmailService>();
        }

        public async Task<List<EmailApproval>> GetPendingApprovalsAsync()
        {
            var currentUser = await _authService.GetCurrentUserAsync();
            if (currentUser == null)
                return new List<EmailApproval>();

            return await _context.EmailApprovals
                .Include(a => a.Email)
                .ThenInclude(e => e.Sender)
                .Include(a => a.Email.Recipients)
                .ThenInclude(r => r.Recipient)
                .Where(a => a.ApproverId == currentUser.Id && a.Status == ApprovalStatus.Pending)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<EmailApproval> CreateApprovalAsync(int emailId, int approverId)
        {
            var approval = new EmailApproval
            {
                EmailId = emailId,
                ApproverId = approverId,
                Status = ApprovalStatus.Pending
            };

            _context.EmailApprovals.Add(approval);
            await _context.SaveChangesAsync();
            return approval;
        }

        public async Task<EmailApproval> ApproveEmailAsync(int approvalId, string comments = null)
        {
            var currentUser = await _authService.GetCurrentUserAsync();
            if (currentUser == null)
                throw new UnauthorizedAccessException("User not authenticated");

            var approval = await _context.EmailApprovals
                .Include(a => a.Email)
                .FirstOrDefaultAsync(a => a.Id == approvalId && a.ApproverId == currentUser.Id);

            if (approval == null)
                throw new KeyNotFoundException("Approval not found or you don't have permission");

            approval.Status = ApprovalStatus.Approved;
            approval.Comments = comments;
            approval.CompletedAt = DateTime.UtcNow;

            // Update the email status
            approval.Email.Status = EmailStatus.Sent;
            approval.Email.SentAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return approval;
        }

        public async Task<EmailApproval> RejectEmailAsync(int approvalId, string comments = null)
        {
            var currentUser = await _authService.GetCurrentUserAsync();
            if (currentUser == null)
                throw new UnauthorizedAccessException("User not authenticated");

            var approval = await _context.EmailApprovals
                .Include(a => a.Email)
                .FirstOrDefaultAsync(a => a.Id == approvalId && a.ApproverId == currentUser.Id);

            if (approval == null)
                throw new KeyNotFoundException("Approval not found or you don't have permission");

            approval.Status = ApprovalStatus.Rejected;
            approval.Comments = comments;
            approval.CompletedAt = DateTime.UtcNow;

            // Update the email status
            approval.Email.Status = EmailStatus.Rejected;

            await _context.SaveChangesAsync();
            return approval;
        }
    }
}
