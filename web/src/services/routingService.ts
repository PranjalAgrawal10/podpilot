import apiClient from './api';
import type {
  ApiResponse,
  RankedModel,
  RoutingDashboard,
  RoutingHistoryItem,
  RoutingPolicySettings,
  SimulateRoutingRequest,
  SimulateRoutingResponse,
  UpdateRoutingPolicySettingsRequest,
} from '../types';

export const routingService = {
  getDashboard: async (): Promise<RoutingDashboard> => {
    const response = await apiClient.get<ApiResponse<RoutingDashboard>>('/routing');
    if (!response.data.data) throw new Error(response.data.message || 'Failed to load routing dashboard');
    return response.data.data;
  },

  getPolicy: async (): Promise<RoutingPolicySettings> => {
    const response = await apiClient.get<ApiResponse<RoutingPolicySettings>>('/routing/policy');
    if (!response.data.data) throw new Error(response.data.message || 'Failed to load routing policy');
    return response.data.data;
  },

  updatePolicy: async (data: UpdateRoutingPolicySettingsRequest): Promise<RoutingPolicySettings> => {
    const response = await apiClient.put<ApiResponse<RoutingPolicySettings>>('/routing/policy', data);
    if (!response.data.data) throw new Error(response.data.message || 'Failed to update routing policy');
    return response.data.data;
  },

  listModels: async (): Promise<RankedModel[]> => {
    const response = await apiClient.get<ApiResponse<RankedModel[]>>('/routing/models');
    if (!response.data.data) throw new Error(response.data.message || 'Failed to load ranked models');
    return response.data.data;
  },

  listHistory: async (take = 50): Promise<RoutingHistoryItem[]> => {
    const response = await apiClient.get<ApiResponse<RoutingHistoryItem[]>>('/routing/history', {
      params: { take },
    });
    if (!response.data.data) throw new Error(response.data.message || 'Failed to load routing history');
    return response.data.data;
  },

  simulate: async (data: SimulateRoutingRequest): Promise<SimulateRoutingResponse> => {
    const response = await apiClient.post<ApiResponse<SimulateRoutingResponse>>('/routing/simulate', data);
    if (!response.data.data) throw new Error(response.data.message || 'Failed to simulate routing');
    return response.data.data;
  },
};
