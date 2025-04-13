using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using Timer = System.Timers.Timer;

namespace Omail.Services
{
    public enum ToastType
    {
        Info,
        Success,
        Warning,
        Error
    }

    public class ToastMessage
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Content { get; set; }
        public ToastType Type { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsVisible { get; set; } = true;
    }

    public interface IToastService
    {
        event Action OnToastsChanged;
        void ShowInfo(string message);
        void ShowSuccess(string message);
        void ShowWarning(string message);
        void ShowError(string message);
        List<ToastMessage> GetToasts();
        void RemoveToast(Guid id);
    }

    public class ToastService : IToastService, IDisposable
    {
        private readonly List<ToastMessage> _toasts = new List<ToastMessage>();
        private readonly Timer _timer;
        
        public event Action OnToastsChanged;

        public ToastService()
        {
            _timer = new Timer(3000); // Auto-hide toasts after 3 seconds
            _timer.Elapsed += OnTimerElapsed;
            _timer.AutoReset = true;
            _timer.Start();
        }

        public void ShowInfo(string message)
        {
            ShowToast(ToastType.Info, message);
        }

        public void ShowSuccess(string message)
        {
            ShowToast(ToastType.Success, message);
        }

        public void ShowWarning(string message)
        {
            ShowToast(ToastType.Warning, message);
        }

        public void ShowError(string message)
        {
            ShowToast(ToastType.Error, message);
        }

        public List<ToastMessage> GetToasts()
        {
            // Return only visible toasts
            return _toasts.Where(t => t.IsVisible).ToList();
        }

        public void RemoveToast(Guid id)
        {
            var toast = _toasts.FirstOrDefault(t => t.Id == id);
            if (toast != null)
            {
                toast.IsVisible = false;
                OnToastsChanged?.Invoke();
            }
        }

        private void ShowToast(ToastType type, string message)
        {
            var toast = new ToastMessage
            {
                Content = message,
                Type = type,
                CreatedAt = DateTime.Now
            };
            
            _toasts.Add(toast);
            OnToastsChanged?.Invoke();
        }

        private void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            bool anyRemoved = false;
            foreach (var toast in _toasts)
            {
                if (toast.IsVisible && (DateTime.Now - toast.CreatedAt).TotalSeconds >= 5)
                {
                    toast.IsVisible = false;
                    anyRemoved = true;
                }
            }

            if (anyRemoved)
            {
                OnToastsChanged?.Invoke();
            }
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
