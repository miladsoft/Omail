using Microsoft.EntityFrameworkCore;
using Omail.Data;
using Microsoft.Extensions.Logging;

namespace Omail.Services
{
    public abstract class ServiceBase
    {
        protected readonly AppDbContext _context;
        protected readonly ILogger _logger;

        protected ServiceBase(AppDbContext context, ILogger logger = null)
        {
            _context = context;
            _logger = logger ?? NullLogger.Instance; // Use a null object pattern for logger
        }
    }

    // NullLogger implementation to avoid null reference exceptions
    public class NullLogger : ILogger
    {
        public static readonly NullLogger Instance = new NullLogger();

        private NullLogger() { }

        public IDisposable BeginScope<TState>(TState state) => null;

        public bool IsEnabled(LogLevel logLevel) => false;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter) { }
    }
}
