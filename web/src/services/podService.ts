import apiClient from './api';
import type {
  ApiResponse,
  CreatePodRequest,
  Pod,
  PodActivity,
  PodIdlePolicy,
  PodLifecycleEvent,
  PodLifecycleSummary,
  PodShutdownResult,
  PodWakeResult,
  UpdatePodIdlePolicyRequest,
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

  getActivity: async (id: string): Promise<PodActivity[]> => {
    const response = await apiClient.get<ApiResponse<PodActivity[]>>(`/pods/${id}/activity`);
    if (!response.data.data) {
      throw new Error(response.data.message || 'Failed to fetch pod activity');
    }
    return response.data.data;
  },

  getLifecycle: async (id: string): Promise<PodLifecycleSummary> => {
    const response = await apiClient.get<ApiResponse<PodLifecycleSummary>>(`/pods/${id}/lifecycle`);
    if (!response.data.data) {
      throw new Error(response.data.message || 'Failed to fetch pod lifecycle');
    }
    return response.data.data;
  },

  getLifecycleEvents: async (id: string): Promise<PodLifecycleEvent[]> => {
    const response = await apiClient.get<ApiResponse<PodLifecycleEvent[]>>(`/pods/${id}/lifecycle/events`);
    if (!response.data.data) {
      throw new Error(response.data.message || 'Failed to fetch lifecycle events');
    }
    return response.data.data;
  },

  wake: async (id: string): Promise<PodWakeResult> => {
    const response = await apiClient.post<ApiResponse<PodWakeResult>>(`/pods/${id}/wake`);
    if (!response.data.data) {
      throw new Error(response.data.message || 'Failed to wake pod');
    }
    return response.data.data;
  },

  shutdown: async (id: string): Promise<PodShutdownResult> => {
    const response = await apiClient.post<ApiResponse<PodShutdownResult>>(`/pods/${id}/shutdown`);
    if (!response.data.data) {
      throw new Error(response.data.message || 'Failed to shutdown pod');
    }
    return response.data.data;
  },

  updateIdlePolicy: async (id: string, data: UpdatePodIdlePolicyRequest): Promise<PodIdlePolicy> => {
    const response = await apiClient.put<ApiResponse<PodIdlePolicy>>(`/pods/${id}/idle-policy`, data);
    if (!response.data.data) {
      throw new Error(response.data.message || 'Failed to update idle policy');
    }
    return response.data.data;
  },
};
