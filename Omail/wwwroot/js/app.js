// Theme handling
window.themeManager = {
    setTheme: function (isDarkMode) {
        if (isDarkMode) {
            document.documentElement.classList.add('dark');
            localStorage.setItem('omail-theme', 'dark');
        } else {
            document.documentElement.classList.remove('dark');
            localStorage.setItem('omail-theme', 'light');
        }
    },
    
    getTheme: function () {
        return localStorage.getItem('omail-theme') === 'dark';
    },
    
    initializeTheme: function () {
        // Check local storage
        const isDarkMode = localStorage.getItem('omail-theme') === 'dark' ||
                          (!localStorage.getItem('omail-theme') && 
                           window.matchMedia('(prefers-color-scheme: dark)').matches);
        
        if (isDarkMode) {
            document.documentElement.classList.add('dark');
        } else {
            document.documentElement.classList.remove('dark');
        }
    }
};

// Icon handler to replace LordIcon with standard SVGs
window.iconHandler = {
    // Map of icon names to SVG paths
    iconMap: {
        'inbox': '<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 8l7.89 5.26a2 2 0 002.22 0L21 8M5 19h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v10a2 2 0 002 2z" />',
        'compose': '<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z" />',
        'sent': '<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 19l9 2-9-18-9 18 9-2zm0 0v-8" />',
        'drafts': '<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />',
        'trash': '<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />',
        'settings': '<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10.325 4.317c.426-1.756 2.924-1.756 3.35 0a1.724 1.724 0 002.573 1.066c1.543-.94 3.31.826 2.37 2.37a1.724 1.724 0 001.065 2.572c1.756.426 1.756 2.924 0 3.35a1.724 1.724 0 00-1.066 2.573c.94 1.543-.826 3.31-2.37 2.37a1.724 1.724 0 00-2.572 1.065c-.426 1.756-2.924 1.756-3.35 0a1.724 1.724 0 00-2.573-1.066c-1.543.94-3.31-.826-2.37-2.37a1.724 1.724 0 00-1.065-2.572c-1.756-.426-1.756-2.924 0-3.35a1.724 1.724 0 001.066-2.573c-.94-1.543.826-3.31 2.37-2.37.996.608 2.296.07 2.572-1.065z" /><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" />',
        'search': '<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />',
        'portfolio': '<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 13.255A23.931 23.931 0 0112 15c-3.183 0-6.22-.62-9-1.745M16 6V4a2 2 0 00-2-2h-4a2 2 0 00-2 2v2m4 6h.01M5 20h14a2 2 0 002-2V8a2 2 0 00-2-2H5a2 2 0 00-2 2v10a2 2 0 002 2z" />',
    },
    
    // Get SVG for a specific icon type
    getSvg: function(iconType, className = 'w-5 h-5') {
        const path = this.iconMap[iconType] || this.iconMap['inbox'];
        return `<svg xmlns="http://www.w3.org/2000/svg" class="${className}" fill="none" viewBox="0 0 24 24" stroke="currentColor">${path}</svg>`;
    },
    
    // Replace all lord-icon elements with SVGs
    replaceAllIcons: function() {
        document.querySelectorAll('lord-icon').forEach(icon => {
            this.replaceLordIcon(icon);
        });
    },
    
    // Replace a single lord-icon with SVG
    replaceLordIcon: function(icon) {
        // Extract icon type from src attribute
        const src = icon.getAttribute('src') || '';
        const parts = src.split('/');
        const fileName = parts[parts.length - 1].replace('.json', '');
        
        // Map to icon type
        let iconType = 'inbox';
        if (fileName.includes('tkaubbni')) iconType = 'inbox';
        else if (fileName.includes('wloilxuq')) iconType = 'compose';
        else if (fileName.includes('iltqorsz')) iconType = 'sent';
        else if (fileName.includes('puvaffet')) iconType = 'drafts';
        else if (fileName.includes('gsqxdxog')) iconType = 'trash';
        else if (fileName.includes('hwuyodym')) iconType = 'settings';
        else if (fileName.includes('msoeawqm')) iconType = 'search'; 
        else if (fileName.includes('fihkmkwt')) iconType = 'portfolio';
        
        // Get class names
        const className = icon.getAttribute('class') || 'w-5 h-5';
        
        // Create replacement element
        const span = document.createElement('span');
        span.className = className;
        span.innerHTML = this.getSvg(iconType, '');
        
        // Replace the original icon
        if (icon.parentNode) {
            icon.parentNode.replaceChild(span, icon);
        }
    }
};

// Loading handler
window.loadingHandler = {
    hideLoader: function() {
        var loadingEl = document.getElementById('app-loading');
        if (loadingEl) {
            loadingEl.style.display = 'none';
        }
    },
    
    showLoader: function() {
        var loadingEl = document.getElementById('app-loading');
        if (loadingEl) {
            loadingEl.style.display = 'flex';
        }
    }
};

// Initialize theme on page load
document.addEventListener('DOMContentLoaded', function () {
    window.themeManager.initializeTheme();
    
    // Replace icons after a short delay to give them a chance to load
    setTimeout(function() {
        window.iconHandler.replaceAllIcons();
        
        // Ensure loader is hidden
        window.loadingHandler.hideLoader();
    }, 1000);
    
    // Enable dismissing Blazor error UI
    const blazorErrorUI = document.getElementById('blazor-error-ui');
    if (blazorErrorUI) {
        const dismissBtn = blazorErrorUI.querySelector('.dismiss');
        if (dismissBtn) {
            dismissBtn.addEventListener('click', function(e) {
                e.preventDefault();
                blazorErrorUI.style.display = 'none';
            });
        }
    }
});

// Ensure loader is hidden when the window is fully loaded
window.addEventListener('load', function() {
    window.loadingHandler.hideLoader();
});

// File drag and drop handling for email attachments
window.fileUploadInterop = {
    initializeFileDropZone: function (dropZoneElement, inputElement) {
        // Prevent default drag behaviors
        ['dragenter', 'dragover', 'dragleave', 'drop'].forEach(eventName => {
            dropZoneElement.addEventListener(eventName, function (e) {
                e.preventDefault();
                e.stopPropagation();
            }, false);
        });

        // Highlight drop zone when item is dragged over it
        ['dragenter', 'dragover'].forEach(eventName => {
            dropZoneElement.addEventListener(eventName, function () {
                dropZoneElement.classList.add('bg-omail-50', 'dark:bg-omail-700/40', 'border-omail-400', 'dark:border-omail-500');
            }, false);
        });

        // Remove highlight when item is dragged out or dropped
        ['dragleave', 'drop'].forEach(eventName => {
            dropZoneElement.addEventListener(eventName, function () {
                dropZoneElement.classList.remove('bg-omail-50', 'dark:bg-omail-700/40', 'border-omail-400', 'dark:border-omail-500');
            }, false);
        });

        // Handle dropped files
        dropZoneElement.addEventListener('drop', function (e) {
            const files = e.dataTransfer.files;
            if (files.length > 0) {
                inputElement.files = files;
                const event = new Event('change', { bubbles: true });
                inputElement.dispatchEvent(event);
            }
        }, false);
    }
};

// Toast notifications
window.toastNotification = {
    show: function (message, type = 'info', duration = 3000) {
        const toast = document.createElement('div');
        
        // Set base styles
        toast.className = 'fixed bottom-4 right-4 px-4 py-2 rounded-lg shadow-lg transform transition-all duration-300 z-50 flex items-center';
        
        // Add type-specific styles
        switch (type) {
            case 'success':
                toast.classList.add('bg-green-500', 'text-white');
                break;
            case 'error':
                toast.classList.add('bg-red-500', 'text-white');
                break;
            case 'warning':
                toast.classList.add('bg-yellow-500', 'text-white');
                break;
            default:
                toast.classList.add('bg-omail-500', 'text-white');
        }
        
        // Add icon based on type
        let icon = '';
        switch (type) {
            case 'success':
                icon = '<svg class="w-5 h-5 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24" xmlns="http://www.w3.org/2000/svg"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 13l4 4L19 7"></path></svg>';
                break;
            case 'error':
                icon = '<svg class="w-5 h-5 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24" xmlns="http://www.w3.org/2000/svg"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"></path></svg>';
                break;
            case 'warning':
                icon = '<svg class="w-5 h-5 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24" xmlns="http://www.w3.org/2000/svg"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z"></path></svg>';
                break;
            default:
                icon = '<svg class="w-5 h-5 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24" xmlns="http://www.w3.org/2000/svg"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"></path></svg>';
        }
        
        // Set content with icon and message
        toast.innerHTML = icon + message;
        
        // Initial state (for animation)
        toast.style.opacity = '0';
        toast.style.transform = 'translateY(1rem)';
        
        // Append to body
        document.body.appendChild(toast);
        
        // Trigger animation
        setTimeout(() => {
            toast.style.opacity = '1';
            toast.style.transform = 'translateY(0)';
        }, 10);
        
        // Hide after duration
        setTimeout(() => {
            toast.style.opacity = '0';
            toast.style.transform = 'translateY(1rem)';
            
            // Remove from DOM after animation completes
            setTimeout(() => {
                document.body.removeChild(toast);
            }, 300);
        }, duration);
    }
};
