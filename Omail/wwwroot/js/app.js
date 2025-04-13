/**
 * Omail App JavaScript Utilities
 */
window.appFunctions = {
    // Toggle password visibility in forms
    togglePasswordVisibility: function(elementId) {
        const input = document.getElementById(elementId);
        if (input) {
            const type = input.getAttribute('type') === 'password' ? 'text' : 'password';
            input.setAttribute('type', type);
            // Optionally toggle icon if needed
            // const icon = input.nextElementSibling?.querySelector('svg'); // Adjust selector if needed
            // if (icon) { ... toggle icon appearance ... }
        }
    },

    // Save text to a file and download it
    saveAsFile: function(filename, content) {
        const blob = new Blob([content], { type: 'text/plain' });
        const link = document.createElement('a');
        link.download = filename;
        link.href = window.URL.createObjectURL(blob);
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
        window.URL.revokeObjectURL(link.href); // Clean up blob URL
    },

    // Set theme preference
    setThemePreference: function(isDark) {
        if (isDark) {
            document.documentElement.classList.add('dark');
        } else {
            document.documentElement.classList.remove('dark');
        }
        // Store preference only if it's not set to follow system
        // The ThemeService in Blazor should decide whether to store 'light', 'dark', or remove the item for 'system'
        // localStorage.setItem('theme', isDark ? 'dark' : 'light'); // Let Blazor service handle storage logic
    },

    // Check browser-level preference for dark mode
    prefersDarkMode: function() {
        return window.matchMedia('(prefers-color-scheme: dark)').matches;
    },

    // Apply Accent Color using CSS Variables
    applyAccentColor: function(colorName) {
        const root = document.documentElement;
        // Map color names to HSL values or specific hex codes
        // Example using Tailwind's default palette structure (adjust as needed)
        // This requires defining these CSS variables in your main CSS file.
        let color500 = '';
        let color600 = '';
        let color400 = ''; // For rings/focus in dark mode

        // Define your color mappings here. Ensure these match your Tailwind config or CSS.
        const colorMap = {
            omail: { 500: '63, 98, 184', 600: '49, 46, 129', 400: '99, 102, 241' }, // Example: Indigo
            blue: { 500: '59, 130, 246', 600: '37, 99, 235', 400: '96, 165, 250' },
            indigo: { 500: '99, 102, 241', 600: '79, 70, 229', 400: '129, 140, 248' },
            purple: { 500: '139, 92, 246', 600: '124, 58, 237', 400: '167, 139, 250' },
            pink: { 500: '236, 72, 153', 600: '219, 39, 119', 400: '244, 114, 182' },
            red: { 500: '239, 68, 68', 600: '220, 38, 38', 400: '248, 113, 113' },
            orange: { 500: '249, 115, 22', 600: '234, 88, 12', 400: '251, 146, 60' },
            amber: { 500: '245, 158, 11', 600: '217, 119, 6', 400: '252, 182, 3' },
            yellow: { 500: '234, 179, 8', 600: '202, 138, 4', 400: '250, 204, 21' },
            green: { 500: '34, 197, 94', 600: '22, 163, 74', 400: '74, 222, 128' },
        };

        const selectedColor = colorMap[colorName] || colorMap['omail']; // Fallback to default

        root.style.setProperty('--color-accent-500-hsl', selectedColor[500]);
        root.style.setProperty('--color-accent-600-hsl', selectedColor[600]);
        root.style.setProperty('--color-accent-400-hsl', selectedColor[400]);

        console.log(`Applied accent color: ${colorName}`);
    },

    // Placeholder for applying other layout settings
    applyLayoutSettings: function(density, fontFamily, fontSize, sidebarPosition) {
        console.log("Applying layout settings:", { density, fontFamily, fontSize, sidebarPosition });
        // Add/remove classes on body or html element based on settings
        // Example: document.body.classList.add(`density-${density}`);
        // Example: document.documentElement.style.setProperty('--font-family-base', fontFamily);
    }
};

// Initialize theme on page load
function initializeTheme() {
    const storedTheme = localStorage.getItem('theme');
    const systemPrefersDark = window.matchMedia('(prefers-color-scheme: dark)').matches;

    if (storedTheme === 'dark' || (storedTheme === null && systemPrefersDark)) {
        document.documentElement.classList.add('dark');
    } else {
        document.documentElement.classList.remove('dark');
    }
}

// Run theme initialization
initializeTheme();

// Listen for system theme changes and update if theme is set to 'system' (null in localStorage)
window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', e => {
    if (localStorage.getItem('theme') === null) { // Only react if following system preference
        if (e.matches) {
            document.documentElement.classList.add('dark');
        } else {
            document.documentElement.classList.remove('dark');
        }
    }
});

// Initialize accent color on load
document.addEventListener('DOMContentLoaded', () => {
    const storedAccentColor = localStorage.getItem('accentColor');
    if (storedAccentColor) {
        window.appFunctions.applyAccentColor(storedAccentColor);
    } else {
         window.appFunctions.applyAccentColor('omail'); // Apply default
    }
    // Initialize other appearance settings if needed
});

// Store commonly used icons to avoid network requests for lord-icon
window.iconStore = {
    outline: {
        'inbox': '<svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 8l7.89 5.26a2 2 0 002.22 0L21 8M5 19h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v10a2 2 0 002 2z" /></svg>',
        'compose': '<svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z" /></svg>',
        'sent': '<svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 19l9 2-9-18-9 18 9-2zm0 0v-8" /></svg>',
        'drafts': '<svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z" /></svg>',
        'trash': '<svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" /></svg>',
        'settings': '<svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10.325 4.317c.426-1.756 2.924-1.756 3.35 0a1.724 1.724 0 002.573 1.066c1.543-.94 3.31.826 2.37 2.37a1.724 1.724 0 001.065 2.572c1.756.426 1.756 2.924 0 3.35a1.724 1.724 0 00-1.066 2.573c.94 1.543-.826 3.31-2.37 2.37a1.724 1.724 0 00-2.572 1.065c-.426 1.756-2.924 1.756-3.35 0a1.724 1.724 0 00-2.573-1.066c-1.543.94-3.31-.826-2.37-2.37a1.724 1.724 0 00-1.065-2.572c-1.756-.426-1.756-2.924 0-3.35a1.724 1.724 0 001.066-2.573c-.94-1.543.826-3.31 2.37-2.37.996.608 2.296.07 2.572-1.065zM10 13a3 3 0 100-6 3 3 0 000 6z" /></svg>'
    }
};

// File input click functionality
window.clickFileInputElement = (dotNetHelper) => {
    // Make the method available for .NET code to call
};

// Function to trigger a click on a file input by its ID
window.clickFileInput = (inputId) => {
    const element = document.getElementById(inputId);
    if (element) {
        element.click();
    } else {
        console.error(`File input with id '${inputId}' not found`);
    }
};