import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';
import type { ProxyOptions } from 'vite';

const apiTarget = 'http://localhost:5000';

function quietProxy(proxyOptions: ProxyOptions): ProxyOptions {
  return {
    ...proxyOptions,
    configure: (proxy) => {
      proxy.on('error', (error, _request, response) => {
        if (response && 'writeHead' in response && !response.headersSent) {
          response.writeHead(502, { 'Content-Type': 'text/plain' });
          response.end('PodPilot API is not running on localhost:5000');
        }

        const code = 'code' in error ? String(error.code) : '';
        if (code === 'ECONNREFUSED' || code === 'ECONNRESET') {
          return;
        }

        console.error('[vite] proxy error:', error.message);
      });
    },
  };
}

export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    proxy: {
      '/api': quietProxy({
        target: apiTarget,
        changeOrigin: true,
      }),
      '/v1': quietProxy({
        target: apiTarget,
        changeOrigin: true,
      }),
      '/hubs': quietProxy({
        target: apiTarget,
        changeOrigin: true,
        ws: true,
      }),
    },
  },
  preview: {
    port: 3000,
    host: true,
  },
});
