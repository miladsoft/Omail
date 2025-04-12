/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "./wwwroot/index.html",
    "./Pages/**/*.razor",
    "./Layout/**/*.razor",
    "./App.razor"
  ],
  darkMode: 'class',
  theme: {
    extend: {
      colors: {
        omail: {
          50: '#ecf5f7',
          100: '#cbdde1',
          200: '#adc7cc',
          300: '#8eb0b7',
          400: '#6e9aa3',
          500: '#4f838e',
          600: '#086c81',
          700: '#065a6b',
          800: '#044755',
          900: '#022229',
          light: '#cbdde1',    // Light shade
          primary: '#086c81',  // Primary color
          dark: '#022229',     // Dark shade
        }
      },
      boxShadow: {
        'omail': '0 4px 14px 0 rgba(8, 108, 129, 0.39)',
      },
      transitionProperty: {
        'height': 'height',
        'spacing': 'margin, padding',
      }
    },
  },
  plugins: []
}
