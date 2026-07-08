const trimTrailingSlash = (value: string) => value.replace(/\/$/, '');

export const getGatewayBaseUrl = (): string => {
  const fromEnv = import.meta.env.VITE_GATEWAY_URL?.trim();
  if (fromEnv) {
    return trimTrailingSlash(fromEnv);
  }

  const apiUrl = import.meta.env.VITE_API_URL?.trim();
  if (apiUrl?.startsWith('http')) {
    return trimTrailingSlash(apiUrl.replace(/\/api\/v1\/?$/, '/v1'));
  }

  return `${window.location.origin}/v1`;
};

export const GATEWAY_ENDPOINTS = [
  {
    method: 'POST',
    path: '/chat/completions',
    description: 'OpenAI-compatible chat completions',
  },
  {
    method: 'POST',
    path: '/responses',
    description: 'OpenAI-compatible responses API',
  },
  {
    method: 'POST',
    path: '/messages',
    description: 'Anthropic-compatible messages API',
  },
  {
    method: 'GET',
    path: '/models',
    description: 'List available models',
  },
] as const;

export const buildGatewayUrl = (path: string): string =>
  `${getGatewayBaseUrl()}${path.startsWith('/') ? path : `/${path}`}`;
