import apiClient from './api';
import type {
  ApiResponse,
  CreateSecretRequest,
  Secret,
  UpdateSecretRequest,
} from '../types';

export const secretsService = {
  list: async (): Promise<Secret[]> => {
    const response = await apiClient.get<ApiResponse<Secret[]>>('/secrets');
    if (!response.data.data) throw new Error(response.data.message || 'Failed to fetch secrets');
    return response.data.data;
  },

  create: async (data: CreateSecretRequest): Promise<Secret> => {
    const response = await apiClient.post<ApiResponse<Secret>>('/secrets', data);
    if (!response.data.data) throw new Error(response.data.message || 'Failed to create secret');
    return response.data.data;
  },

  update: async (id: string, data: UpdateSecretRequest): Promise<Secret> => {
    const response = await apiClient.put<ApiResponse<Secret>>(`/secrets/${id}`, data);
    if (!response.data.data) throw new Error(response.data.message || 'Failed to update secret');
    return response.data.data;
  },

  delete: async (id: string): Promise<void> => {
    await apiClient.delete(`/secrets/${id}`);
  },
};
