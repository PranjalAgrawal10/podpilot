import axios, { type AxiosError, type InternalAxiosRequestConfig } from 'axios';
import { tokenStorage } from '../utils/tokenStorage';
import { organizationStorage } from '../utils/organizationStorage';
import type { ApiResponse, AuthResponse } from '../types';

const API_BASE_URL = import.meta.env.VITE_API_URL || '/api/v1';

export const apiClient = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

let isRefreshing = false;
let refreshSubscribers: Array<(token: string) => void> = [];

const subscribeTokenRefresh = (callback: (token: string) => void) => {
  refreshSubscribers.push(callback);
};

const onTokenRefreshed = (token: string) => {
  refreshSubscribers.forEach((callback) => callback(token));
  refreshSubscribers = [];
};

apiClient.interceptors.request.use((config: InternalAxiosRequestConfig) => {
  const token = tokenStorage.getAccessToken();
  if (token && config.headers) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

apiClient.interceptors.response.use(
  (response) => response,
  async (error: AxiosError) => {
    const originalRequest = error.config as InternalAxiosRequestConfig & { _retry?: boolean };

    if (error.response?.status === 401 && !originalRequest._retry) {
      const refreshToken = tokenStorage.getRefreshToken();

      if (!refreshToken) {
        tokenStorage.clear();
        window.location.href = '/login';
        return Promise.reject(error);
      }

      if (isRefreshing) {
        return new Promise((resolve) => {
          subscribeTokenRefresh((token: string) => {
            if (originalRequest.headers) {
              originalRequest.headers.Authorization = `Bearer ${token}`;
            }
            resolve(apiClient(originalRequest));
          });
        });
      }

      originalRequest._retry = true;
      isRefreshing = true;

      try {
        const response = await axios.post<ApiResponse<AuthResponse>>(
          `${API_BASE_URL}/auth/refresh`,
          { refreshToken },
        );

        const authData = response.data.data;
        if (authData) {
          tokenStorage.setAccessToken(authData.accessToken);
          tokenStorage.setRefreshToken(authData.refreshToken);
          if (authData.user.currentOrganizationId) {
            organizationStorage.setCurrentOrganizationId(authData.user.currentOrganizationId);
          }
          if (authData.user.currentOrganizationRole) {
            organizationStorage.setCurrentOrganizationRole(authData.user.currentOrganizationRole);
          }
          onTokenRefreshed(authData.accessToken);

          if (originalRequest.headers) {
            originalRequest.headers.Authorization = `Bearer ${authData.accessToken}`;
          }

          return apiClient(originalRequest);
        }
      } catch {
        tokenStorage.clear();
        window.location.href = '/login';
      } finally {
        isRefreshing = false;
      }
    }

    return Promise.reject(error);
  },
);

export default apiClient;
