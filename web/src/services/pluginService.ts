import apiClient from './api';
import type {
  ApiResponse,
  InstallPluginRequest,
  Plugin,
  PluginDashboard,
  PluginSetting,
  UpdatePluginRequest,
  UpdatePluginSettingsRequest,
} from '../types';

export const pluginService = {
  list: async (): Promise<Plugin[]> => {
    const response = await apiClient.get<ApiResponse<Plugin[]>>('/plugins');
    if (!response.data.data) throw new Error(response.data.message || 'Failed to fetch plugins');
    return response.data.data;
  },

  getDashboard: async (): Promise<PluginDashboard> => {
    const response = await apiClient.get<ApiResponse<PluginDashboard>>('/plugins/dashboard');
    if (!response.data.data) throw new Error(response.data.message || 'Failed to fetch plugin dashboard');
    return response.data.data;
  },

  getById: async (installationId: string): Promise<Plugin> => {
    const response = await apiClient.get<ApiResponse<Plugin>>(`/plugins/${installationId}`);
    if (!response.data.data) throw new Error(response.data.message || 'Failed to fetch plugin');
    return response.data.data;
  },

  install: async (data: InstallPluginRequest): Promise<Plugin> => {
    const response = await apiClient.post<ApiResponse<Plugin>>('/plugins', data);
    if (!response.data.data) throw new Error(response.data.message || 'Failed to install plugin');
    return response.data.data;
  },

  update: async (installationId: string, data: UpdatePluginRequest): Promise<Plugin> => {
    const response = await apiClient.put<ApiResponse<Plugin>>(`/plugins/${installationId}`, data);
    if (!response.data.data) throw new Error(response.data.message || 'Failed to update plugin');
    return response.data.data;
  },

  uninstall: async (installationId: string): Promise<void> => {
    await apiClient.delete(`/plugins/${installationId}`);
  },

  getSettings: async (installationId: string): Promise<PluginSetting[]> => {
    const response = await apiClient.get<ApiResponse<PluginSetting[]>>(`/plugins/${installationId}/settings`);
    if (!response.data.data) throw new Error(response.data.message || 'Failed to fetch plugin settings');
    return response.data.data;
  },

  updateSettings: async (installationId: string, data: UpdatePluginSettingsRequest): Promise<void> => {
    await apiClient.put(`/plugins/${installationId}/settings`, data);
  },

  enable: async (installationId: string): Promise<void> => {
    await apiClient.post(`/plugins/${installationId}/enable`);
  },

  disable: async (installationId: string): Promise<void> => {
    await apiClient.post(`/plugins/${installationId}/disable`);
  },
};
