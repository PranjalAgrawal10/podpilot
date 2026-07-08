import apiClient from './api';
import type {
  ApiResponse,
  CreateProviderRequest,
  Provider,
  ProviderGpu,
  ProviderHealth,
  ProviderRegion,
  ProviderTemplate,
  ProviderValidationResult,
  UpdateProviderRequest,
  ValidateProviderRequest,
} from '../types';

export const providerService = {
  list: async (): Promise<Provider[]> => {
    const response = await apiClient.get<ApiResponse<Provider[]>>('/providers');
    if (!response.data.data) {
      throw new Error(response.data.message || 'Failed to fetch providers');
    }
    return response.data.data;
  },

  getById: async (id: string): Promise<Provider> => {
    const response = await apiClient.get<ApiResponse<Provider>>(`/providers/${id}`);
    if (!response.data.data) {
      throw new Error(response.data.message || 'Failed to fetch provider');
    }
    return response.data.data;
  },

  create: async (data: CreateProviderRequest): Promise<Provider> => {
    const response = await apiClient.post<ApiResponse<Provider>>('/providers', data);
    if (!response.data.data) {
      throw new Error(response.data.message || 'Failed to create provider');
    }
    return response.data.data;
  },

  update: async (id: string, data: UpdateProviderRequest): Promise<Provider> => {
    const response = await apiClient.put<ApiResponse<Provider>>(`/providers/${id}`, data);
    if (!response.data.data) {
      throw new Error(response.data.message || 'Failed to update provider');
    }
    return response.data.data;
  },

  delete: async (id: string): Promise<void> => {
    await apiClient.delete(`/providers/${id}`);
  },

  validateNew: async (data: ValidateProviderRequest): Promise<ProviderValidationResult> => {
    const response = await apiClient.post<ApiResponse<ProviderValidationResult>>(
      '/providers/validate',
      data,
    );
    if (!response.data.data) {
      throw new Error(response.data.message || 'Failed to validate provider');
    }
    return response.data.data;
  },

  validate: async (
    id: string,
    data?: ValidateProviderRequest,
  ): Promise<ProviderValidationResult> => {
    const response = await apiClient.post<ApiResponse<ProviderValidationResult>>(
      `/providers/${id}/validate`,
      data ?? {},
    );
    if (!response.data.data) {
      throw new Error(response.data.message || 'Failed to validate provider');
    }
    return response.data.data;
  },

  getRegions: async (id: string): Promise<ProviderRegion[]> => {
    const response = await apiClient.get<ApiResponse<ProviderRegion[]>>(
      `/providers/${id}/regions`,
    );
    if (!response.data.data) {
      throw new Error(response.data.message || 'Failed to fetch regions');
    }
    return response.data.data;
  },

  getGpus: async (id: string): Promise<ProviderGpu[]> => {
    const response = await apiClient.get<ApiResponse<ProviderGpu[]>>(`/providers/${id}/gpus`);
    if (!response.data.data) {
      throw new Error(response.data.message || 'Failed to fetch GPUs');
    }
    return response.data.data;
  },

  getTemplates: async (id: string): Promise<ProviderTemplate[]> => {
    const response = await apiClient.get<ApiResponse<ProviderTemplate[]>>(
      `/providers/${id}/templates`,
    );
    if (!response.data.data) {
      throw new Error(response.data.message || 'Failed to fetch templates');
    }
    return response.data.data;
  },

  getHealth: async (id: string): Promise<ProviderHealth> => {
    const response = await apiClient.get<ApiResponse<ProviderHealth>>(`/providers/${id}/health`);
    if (!response.data.data) {
      throw new Error(response.data.message || 'Failed to fetch provider health');
    }
    return response.data.data;
  },
};
