export interface ApiResponse<T> {
  success: boolean;
  data?: T;
  message?: string;
  errors?: Record<string, string[]>;
  correlationId?: string;
}

export interface UserSummary {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  roles: string[];
}

export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  expiresIn: number;
  tokenType: string;
  user: UserSummary;
}

export interface OrganizationSummary {
  id: string;
  name: string;
  slug: string;
  role: string;
}

export interface UserResponse {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  roles: string[];
  organizations: OrganizationSummary[];
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
  organizationName: string;
}

export interface HealthResponse {
  status: string;
  checks: Record<string, { status: string; description?: string; duration: string }>;
  totalDuration: string;
}

export type ThemeMode = 'light' | 'dark';
