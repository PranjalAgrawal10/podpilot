import apiClient from './api';
import type {
  ApiResponse,
  AuthResponse,
  LoginRequest,
  RegisterRequest,
  UserResponse,
  HealthResponse,
} from '../types';

export const authService = {
  register: async (data: RegisterRequest): Promise<AuthResponse> => {
    const response = await apiClient.post<ApiResponse<AuthResponse>>('/auth/register', data);
    if (!response.data.data) {
      throw new Error(response.data.message || 'Registration failed');
    }
    return response.data.data;
  },

  login: async (data: LoginRequest): Promise<AuthResponse> => {
    const response = await apiClient.post<ApiResponse<AuthResponse>>('/auth/login', data);
    if (!response.data.data) {
      throw new Error(response.data.message || 'Login failed');
    }
    return response.data.data;
  },

  logout: async (refreshToken: string): Promise<void> => {
    await apiClient.post('/auth/logout', { refreshToken });
  },
};

export const userService = {
  getCurrentUser: async (): Promise<UserResponse> => {
    const response = await apiClient.get<ApiResponse<UserResponse>>('/users/me');
    if (!response.data.data) {
      throw new Error(response.data.message || 'Failed to fetch user');
    }
    return response.data.data;
  },
};

export const healthService = {
  getHealth: async (): Promise<HealthResponse> => {
    const response = await apiClient.get<ApiResponse<HealthResponse>>('/health');
    if (!response.data.data) {
      throw new Error(response.data.message || 'Health check failed');
    }
    return response.data.data;
  },
};
