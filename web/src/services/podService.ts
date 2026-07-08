import apiClient from './api';
import type {
  ApiResponse,
  CreatePodRequest,
  Pod,
  UpdatePodRequest,
} from '../types';

export const podService = {
  list: async (): Promise<Pod[]> => {
    const response = await apiClient.get<ApiResponse<Pod[]>>('/pods');
    if (!response.data.data) {
      throw new Error(response.data.message || 'Failed to fetch pods');
    }
    return response.data.data;
  },

  getById: async (id: string): Promise<Pod> => {
    const response = await apiClient.get<ApiResponse<Pod>>(`/pods/${id}`);
    if (!response.data.data) {
      throw new Error(response.data.message || 'Failed to fetch pod');
    }
    return response.data.data;
  },

  create: async (data: CreatePodRequest): Promise<Pod> => {
    const response = await apiClient.post<ApiResponse<Pod>>('/pods', data);
    if (!response.data.data) {
      throw new Error(response.data.message || 'Failed to create pod');
    }
    return response.data.data;
  },

  update: async (id: string, data: UpdatePodRequest): Promise<Pod> => {
    const response = await apiClient.put<ApiResponse<Pod>>(`/pods/${id}`, data);
    if (!response.data.data) {
      throw new Error(response.data.message || 'Failed to update pod');
    }
    return response.data.data;
  },

  delete: async (id: string, force = false): Promise<void> => {
    await apiClient.delete(`/pods/${id}`, { data: { force } });
  },

  start: async (id: string): Promise<Pod> => {
    const response = await apiClient.post<ApiResponse<Pod>>(`/pods/${id}/start`);
    if (!response.data.data) {
      throw new Error(response.data.message || 'Failed to start pod');
    }
    return response.data.data;
  },

  stop: async (id: string): Promise<Pod> => {
    const response = await apiClient.post<ApiResponse<Pod>>(`/pods/${id}/stop`);
    if (!response.data.data) {
      throw new Error(response.data.message || 'Failed to stop pod');
    }
    return response.data.data;
  },

  restart: async (id: string): Promise<Pod> => {
    const response = await apiClient.post<ApiResponse<Pod>>(`/pods/${id}/restart`);
    if (!response.data.data) {
      throw new Error(response.data.message || 'Failed to restart pod');
    }
    return response.data.data;
  },

  sync: async (id: string): Promise<Pod> => {
    const response = await apiClient.post<ApiResponse<Pod>>(`/pods/${id}/sync`);
    if (!response.data.data) {
      throw new Error(response.data.message || 'Failed to sync pod');
    }
    return response.data.data;
  },
};
