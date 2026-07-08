import { apiClient } from './api';
import type { ApiResponse } from '../types';
import type {
  GatewayApiKey,
  GatewayRequestSummary,
  GatewayRoute,
  GatewayStats,
} from '../types';

export const gatewayService = {
  async listApiKeys(): Promise<GatewayApiKey[]> {
    const response = await apiClient.get<ApiResponse<GatewayApiKey[]>>('/gateway/api-keys');
    return response.data.data ?? [];
  },

  async createApiKey(payload: {
    name: string;
    isPersonal: boolean;
    expiresAt?: string;
    rateLimitPerMinute?: number;
    rateLimitPerDay?: number;
  }): Promise<GatewayApiKey> {
    const response = await apiClient.post<ApiResponse<GatewayApiKey>>('/gateway/api-keys', payload);
    return response.data.data!;
  },

  async revokeApiKey(keyId: string): Promise<void> {
    await apiClient.delete(`/gateway/api-keys/${keyId}`);
  },

  async rotateApiKey(keyId: string): Promise<GatewayApiKey> {
    const response = await apiClient.post<ApiResponse<GatewayApiKey>>(`/gateway/api-keys/${keyId}/rotate`);
    return response.data.data!;
  },

  async listRoutes(): Promise<GatewayRoute[]> {
    const response = await apiClient.get<ApiResponse<GatewayRoute[]>>('/gateway/routes');
    return response.data.data ?? [];
  },

  async createRoute(payload: {
    gpuPodId: string;
    modelName: string;
    isDefault: boolean;
  }): Promise<GatewayRoute> {
    const response = await apiClient.post<ApiResponse<GatewayRoute>>('/gateway/routes', payload);
    return response.data.data!;
  },

  async deleteRoute(routeId: string): Promise<void> {
    await apiClient.delete(`/gateway/routes/${routeId}`);
  },

  async getStats(): Promise<GatewayStats> {
    const response = await apiClient.get<ApiResponse<GatewayStats>>('/gateway/stats');
    return response.data.data!;
  },

  async listRequests(limit = 50): Promise<GatewayRequestSummary[]> {
    const response = await apiClient.get<ApiResponse<GatewayRequestSummary[]>>('/gateway/requests', {
      params: { limit },
    });
    return response.data.data ?? [];
  },
};
