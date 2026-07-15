export interface ApiEnvelope<T> {
  success: boolean;
  data?: T;
  message?: string;
  correlationId?: string;
}

export interface AuthResult {
  accessToken: string;
  refreshToken: string;
  expiresIn: number;
  tokenType: string;
}

export interface PodSummary {
  id: string;
  name: string;
  status: string;
}

export interface HealthStatus {
  status: string;
  totalDuration: string;
}

export interface GatewayHealth {
  activeRequests: number;
  recentErrors: number;
  averageLatencyMs: number;
}

export interface PodPilotClientOptions {
  baseUrl?: string;
  accessToken?: string;
  fetchImpl?: typeof fetch;
}

export class PodPilotError extends Error {
  constructor(message: string) {
    super(message);
    this.name = 'PodPilotError';
  }
}

export class PodPilotClient {
  private readonly baseUrl: string;
  private accessToken?: string;
  private readonly fetchImpl: typeof fetch;

  constructor(options: PodPilotClientOptions = {}) {
    this.baseUrl = (options.baseUrl ?? process.env.PODPILOT_API_URL ?? 'http://localhost:5000').replace(/\/$/, '');
    this.accessToken = options.accessToken;
    this.fetchImpl = options.fetchImpl ?? fetch;
  }

  setAccessToken(token: string): void {
    this.accessToken = token;
  }

  async login(email: string, password: string): Promise<AuthResult> {
    const auth = await this.request<AuthResult>('POST', '/api/v1/auth/login', { email, password }, false);
    this.accessToken = auth.accessToken;
    return auth;
  }

  async listPods(): Promise<PodSummary[]> {
    return this.request<PodSummary[]>('GET', '/api/v1/pods');
  }

  async getHealth(): Promise<HealthStatus> {
    return this.request<HealthStatus>('GET', '/api/v1/health', undefined, false);
  }

  async getGatewayStats(): Promise<GatewayHealth> {
    return this.request<GatewayHealth>('GET', '/api/v1/gateway/stats');
  }

  private async request<T>(
    method: string,
    path: string,
    body?: unknown,
    auth = true,
  ): Promise<T> {
    const headers: Record<string, string> = {
      Accept: 'application/json',
    };
    if (body !== undefined) {
      headers['Content-Type'] = 'application/json';
    }
    if (auth) {
      if (!this.accessToken) {
        throw new PodPilotError('Not authenticated. Call login() or pass accessToken.');
      }
      headers.Authorization = `Bearer ${this.accessToken}`;
    }

    const response = await this.fetchImpl(`${this.baseUrl}${path}`, {
      method,
      headers,
      body: body === undefined ? undefined : JSON.stringify(body),
    });

    const text = await response.text();
    let envelope: ApiEnvelope<T>;
    try {
      envelope = JSON.parse(text) as ApiEnvelope<T>;
    } catch {
      throw new PodPilotError(`Invalid JSON (HTTP ${response.status}): ${text.slice(0, 200)}`);
    }

    if (!response.ok || !envelope.success || envelope.data === undefined) {
      throw new PodPilotError(envelope.message ?? `HTTP ${response.status}: ${text.slice(0, 200)}`);
    }

    return envelope.data;
  }
}
