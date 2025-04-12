/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    './**/*.{razor,html,cshtml}',
    './Pages/**/*.{razor,html,cshtml}',
    './Shared/**/*.{razor,html,cshtml}',
    './Components/**/*.{razor,html,cshtml}'
  ],
  darkMode: 'class',
  theme: {
    extend: {
      colors: {
        'omail': {
          50: '#f6f5ff',
          100: '#edebfe',
          200: '#dad7fc',
          300: '#c0bcf7',
          400: '#9c96f1',
          500: '#7e76ea',
          600: '#6555db',
          700: '#5344c4',
          800: '#4338a0',
          900: '#38337f',
          950: '#221e49'
        },
        'omail-dark': '#1e1b4b',
        'omail-light': '#f5f3ff',
        'omail-primary': '#6555db',
      }
    },
    boxShadow: {
      sm: '0 1px 2px 0 rgba(0, 0, 0, 0.05)',
      DEFAULT: '0 1px 3px 0 rgba(0, 0, 0, 0.1), 0 1px 2px 0 rgba(0, 0, 0, 0.06)',
      md: '0 4px 6px -1px rgba(0, 0, 0, 0.1), 0 2px 4px -1px rgba(0, 0, 0, 0.06)',
      lg: '0 10px 15px -3px rgba(0, 0, 0, 0.1), 0 4px 6px -2px rgba(0, 0, 0, 0.05)',
      xl: '0 20px 25px -5px rgba(0, 0, 0, 0.1), 0 10px 10px -5px rgba(0, 0, 0, 0.04)',
      '2xl': '0 25px 50px -12px rgba(0, 0, 0, 0.25)',
      '3xl': '0 35px 60px -15px rgba(0, 0, 0, 0.3)',
      inner: 'inset 0 2px 4px 0 rgba(0, 0, 0, 0.06)',
      none: 'none',
      'omail': '0 4px 14px 0 rgba(101, 85, 219, 0.39)',
    }
  },
  variants: {
    extend: {
      opacity: ['disabled'],
      cursor: ['disabled'],
      backgroundColor: ['disabled', 'active', 'group-hover'],
      textColor: ['disabled', 'active', 'group-hover'],
      borderColor: ['disabled', 'active', 'group-hover'],
      ringColor: ['hover', 'active'],
      ringWidth: ['hover', 'active'],
    },
  },
  plugins: [
    require('@tailwindcss/forms'),
    require('@tailwindcss/typography'),
    require('@tailwindcss/aspect-ratio'),
  ],
}
