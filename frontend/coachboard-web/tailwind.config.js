/** @type {import('tailwindcss').Config} */
export default {
  content: [
  './index.html',
  './src/**/*.{ts,tsx}',
  ],
  theme: {
  extend: {
  colors: {
  primary: {
  50: '#eef7ff', 100: '#d9ecff', 200: '#bfe0ff', 300: '#93c8ff',
  400: '#5aa5ff', 500: '#2a7dff', 600: '#1f60d6', 700: '#184ab0',
  800: '#153d8f', 900: '#143675', 950: '#0e224a'
  }
  }
  },
  },
  plugins: [],
  }