// Add these functions if they're not already present

// This function can be triggered from C# to toggle password visibility
function togglePasswordVisibility(elementId) {
    const input = document.getElementById(elementId);
    if (input) {
        const type = input.getAttribute('type') === 'password' ? 'text' : 'password';
        input.setAttribute('type', type);
    }
}

// This function allows downloading of text files
function saveAsFile(filename, content) {
    const blob = new Blob([content], { type: 'text/plain' });
    const link = document.createElement('a');
    link.download = filename;
    link.href = window.URL.createObjectURL(blob);
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
}

// Simple toast notification system (can be replaced with a more robust solution)
function showToast(type, message) {
    console.log(`Toast [${type}]: ${message}`);
    // You could use a library like toastr here
    // toastr[type](message);
}

// These functions need to be accessible from C#
window.appFunctions = {
    togglePasswordVisibility,
    saveAsFile,
    showToast,
    
    // Theme functions
    setThemePreference: function(isDark) {
        if (isDark) {
            document.documentElement.classList.add('dark');
        } else {
            document.documentElement.classList.remove('dark');
        }
        localStorage.setItem('theme', isDark ? 'dark' : 'light');
    },
    
    prefersDarkMode: function() {
        return window.matchMedia('(prefers-color-scheme: dark)').matches;
    }
};
