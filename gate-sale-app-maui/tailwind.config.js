/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "./Components/**/*.{razor,html,js}",
    "./wwwroot/**/*.html"
  ],
  theme: {
    extend: {
      colors: {
        'gate-teal': '#4ECDC4',
        'sale-orange': '#FF6B6B',
        'soft-gray': '#666666',
        'light-blue': '#E8F4F8',
        'sky-blue': '#F0F8FF',
        'button-blue': '#00BFFF',
        'heart-green': '#4CAF50',
        'shield-blue': '#2196F3'
      },
      fontFamily: {
        'system': ['-apple-system', 'BlinkMacSystemFont', 'Segoe UI', 'Roboto', 'sans-serif']
      },
      boxShadow: {
        'soft': '0 8px 32px rgba(0, 0, 0, 0.08)',
        'button': '0 4px 16px rgba(0, 191, 255, 0.3)',
        'icon': '0 4px 12px rgba(0, 0, 0, 0.15)'
      }
    },
  },
  plugins: [],
}
