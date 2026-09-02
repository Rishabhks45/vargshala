/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    './**/*.{razor,html,cshtml}',
    '../Vargshala.Contracts/**/*.{cs,razor}'
  ],
  darkMode: 'class',
  theme: {
    extend: {
      colors: {
        // Theme 4: Teal & Coral (Fresh & Energetic)
        brand: {
          50: '#f0fdfa',   // --primary-bg-light
          100: '#ccfbf1',  // --primary-bg
          200: '#99f6e4',
          300: '#5eead4',
          400: '#2dd4bf',
          500: '#14b8a6',  // --primary-light
          600: '#009488',  // --primary (Base Vibrant Teal)
          700: '#0f766e',
          800: '#115e59',
          900: '#0f4f4b',  // Sidebar Deep Teal
          950: '#042f2e',
        },
        // Coral Accent
        coral: {
          50: '#fff1f2',
          100: '#ffe4e6',
          200: '#fecdd3',
          300: '#fda4af',
          400: '#fb7185',
          500: '#fb6868',  // --coral base
          600: '#e11d48',
          700: '#be123c',
        },
        // Orange / Amber Accent
        amber: {
          50: '#fffbeb',
          100: '#fef3c7',
          500: '#f59e0b',  // --orange
          600: '#d97706',
        },
        // Custom Neutral & Border
        tealborder: '#e2f1ef',
        slate: {
          50: '#f8fafc',
          100: '#f1f5f9',
          200: '#e2e8f0',
          300: '#cbd5e1',
          400: '#94a3b8',
          500: '#64748b',  // --text-muted
          600: '#475569',
          700: '#334155',
          800: '#1e293b',
          900: '#0f172a',
          950: '#020617',
        }
      },
      fontFamily: {
        sans: ['Inter', 'Plus Jakarta Sans', '-apple-system', 'BlinkMacSystemFont', 'Segoe UI', 'Roboto', 'sans-serif'],
        display: ['Plus Jakarta Sans', 'Inter', 'sans-serif'],
      },
      boxShadow: {
        'soft': '0 4px 12px rgba(13, 148, 136, 0.08)',
        'soft-lg': '0 10px 25px -3px rgba(13, 148, 136, 0.1), 0 4px 6px -2px rgba(13, 148, 136, 0.05)',
        'brand': '0 8px 20px -4px rgba(0, 148, 136, 0.35)',
        'coral': '0 8px 20px -4px rgba(251, 104, 104, 0.35)',
      },
      borderRadius: {
        'xl': '0.75rem',
        '2xl': '1rem',
        '3xl': '1.5rem',
      },
      animation: {
        'float-slow': 'float 6s ease-in-out infinite',
        'float-medium': 'float 4s ease-in-out infinite',
        'float-reverse': 'float-reverse 5s ease-in-out infinite',
        'pulse-glow': 'pulse-glow 3s cubic-bezier(0.4, 0, 0.6, 1) infinite',
        'plane-fly': 'plane-fly 4s ease-in-out infinite',
        'dash-flow': 'dash-flow 20s linear infinite',
      },
      keyframes: {
        float: {
          '0%, 100%': { transform: 'translateY(0px)' },
          '50%': { transform: 'translateY(-10px)' },
        },
        'float-reverse': {
          '0%, 100%': { transform: 'translateY(0px)' },
          '50%': { transform: 'translateY(10px)' },
        },
        'pulse-glow': {
          '0%, 100%': { opacity: '0.8', transform: 'scale(1)' },
          '50%': { opacity: '0.4', transform: 'scale(1.05)' },
        },
        'plane-fly': {
          '0%, 100%': { transform: 'translate(0, 0) rotate(0deg)' },
          '50%': { transform: 'translate(8px, -12px) rotate(2deg)' },
        },
        'dash-flow': {
          to: { strokeDashoffset: '-100' },
        },
      }
    },
  },
  plugins: [],
}
