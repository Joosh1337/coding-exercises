import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    strictPort: true,
  },
  test: {
    environment: 'jsdom',
    setupFiles: ['./tests/setup.ts'],
    globals: true,
    coverage: {
      provider: 'v8',
      reporter: ['text', 'html'],
      exclude: ['src/main.tsx', 'src/types/**', 'tests/**', '*.config.*'],
      thresholds: { lines: 80, functions: 80, branches: 80, statements: 80 },
    },
  },
})
