import apiClient from './api';
import type {
  AiProvider,
  AiProviderDashboard,
  AiProviderHealth,
  AiProviderKindMetadata,
  AiProviderModel,
  AiProviderValidationResult,
  AiRoutingPolicy,
  ApiResponse,
  CreateAiProviderRequest,
  CreateAiRoutingPolicyRequest,
  UpdateAiProviderRequest,
} from '../types';

export const aiProviderService = {
  list: async (): Promise<AiProvider[]> => {
    const response = await apiClient.get<ApiResponse<AiProvider[]>>('/ai/providers');
    if (!response.data.data) throw new Error(response.data.message || 'Failed to fetch AI providers');
    return response.data.data;
  },

  getById: async (id: string): Promise<AiProvider> => {
    const response = await apiClient.get<ApiResponse<AiProvider>>(`/ai/providers/${id}`);
    if (!response.data.data) throw new Error(response.data.message || 'Failed to fetch AI provider');
    return response.data.data;
  },

  create: async (data: CreateAiProviderRequest): Promise<AiProvider> => {
    const response = await apiClient.post<ApiResponse<AiProvider>>('/ai/providers', data);
    if (!response.data.data) throw new Error(response.data.message || 'Failed to create AI provider');
    return response.data.data;
  },

  update: async (id: string, data: UpdateAiProviderRequest): Promise<AiProvider> => {
    const response = await apiClient.put<ApiResponse<AiProvider>>(`/ai/providers/${id}`, data);
    if (!response.data.data) throw new Error(response.data.message || 'Failed to update AI provider');
    return response.data.data;
  },

  delete: async (id: string): Promise<void> => {
    await apiClient.delete(`/ai/providers/${id}`);
  },

  validate: async (
    id: string,
    data?: { apiKey?: string; baseUrl?: string },
  ): Promise<AiProviderValidationResult> => {
    const response = await apiClient.post<ApiResponse<AiProviderValidationResult>>(
      `/ai/providers/${id}/validate`,
      data ?? {},
    );
    if (!response.data.data) throw new Error(response.data.message || 'Failed to validate AI provider');
    return response.data.data;
  },

  getHealth: async (id: string): Promise<AiProviderHealth> => {
    const response = await apiClient.get<ApiResponse<AiProviderHealth>>(`/ai/providers/${id}/health`);
    if (!response.data.data) throw new Error(response.data.message || 'Failed to fetch AI provider health');
    return response.data.data;
  },

  listModels: async (providerId?: string): Promise<AiProviderModel[]> => {
    const response = await apiClient.get<ApiResponse<AiProviderModel[]>>('/ai/models', {
      params: providerId ? { providerId } : undefined,
    });
    if (!response.data.data) throw new Error(response.data.message || 'Failed to fetch AI models');
    return response.data.data;
  },

  listRoutingPolicies: async (): Promise<AiRoutingPolicy[]> => {
    const response = await apiClient.get<ApiResponse<AiRoutingPolicy[]>>('/ai/routing-policies');
    if (!response.data.data) throw new Error(response.data.message || 'Failed to fetch routing policies');
    return response.data.data;
  },

  createRoutingPolicy: async (data: CreateAiRoutingPolicyRequest): Promise<AiRoutingPolicy> => {
    const response = await apiClient.post<ApiResponse<AiRoutingPolicy>>('/ai/routing-policies', data);
    if (!response.data.data) throw new Error(response.data.message || 'Failed to create routing policy');
    return response.data.data;
  },

  updateRoutingPolicy: async (
    id: string,
    data: CreateAiRoutingPolicyRequest,
  ): Promise<AiRoutingPolicy> => {
    const response = await apiClient.put<ApiResponse<AiRoutingPolicy>>(`/ai/routing-policies/${id}`, data);
    if (!response.data.data) throw new Error(response.data.message || 'Failed to update routing policy');
    return response.data.data;
  },

  deleteRoutingPolicy: async (id: string): Promise<void> => {
    await apiClient.delete(`/ai/routing-policies/${id}`);
  },

  getDashboard: async (): Promise<AiProviderDashboard> => {
    const response = await apiClient.get<ApiResponse<AiProviderDashboard>>('/ai/dashboard');
    if (!response.data.data) throw new Error(response.data.message || 'Failed to fetch AI dashboard');
    return response.data.data;
  },

  listKinds: async (): Promise<AiProviderKindMetadata[]> => {
    const response = await apiClient.get<ApiResponse<AiProviderKindMetadata[]>>('/ai/provider-kinds');
    if (!response.data.data) throw new Error(response.data.message || 'Failed to fetch provider kinds');
    return response.data.data;
  },
};
