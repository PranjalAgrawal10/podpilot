import apiClient from './api';
import type {
  ApiResponse,
  CreateDeploymentRequest,
  Deployment,
  DeploymentDashboard,
  DeploymentHealth,
  DeploymentRegion,
  DeploymentTemplate,
  GpuCatalogEntry,
  GpuRecommendation,
  ModelCatalogEntry,
} from '../types';

export const deploymentService = {
  list: async (): Promise<Deployment[]> => {
    const response = await apiClient.get<ApiResponse<Deployment[]>>('/deployments');
    if (!response.data.data) throw new Error(response.data.message || 'Failed to fetch deployments');
    return response.data.data;
  },

  getDashboard: async (): Promise<DeploymentDashboard> => {
    const response = await apiClient.get<ApiResponse<DeploymentDashboard>>('/deployments/dashboard');
    if (!response.data.data) throw new Error(response.data.message || 'Failed to fetch deployment dashboard');
    return response.data.data;
  },

  getById: async (id: string): Promise<Deployment> => {
    const response = await apiClient.get<ApiResponse<Deployment>>(`/deployments/${id}`);
    if (!response.data.data) throw new Error(response.data.message || 'Failed to fetch deployment');
    return response.data.data;
  },

  create: async (data: CreateDeploymentRequest): Promise<Deployment> => {
    const response = await apiClient.post<ApiResponse<Deployment>>('/deployments', data);
    if (!response.data.data) throw new Error(response.data.message || 'Failed to create deployment');
    return response.data.data;
  },

  delete: async (id: string): Promise<void> => {
    await apiClient.delete(`/deployments/${id}`);
  },

  restart: async (id: string): Promise<Deployment> => {
    const response = await apiClient.post<ApiResponse<Deployment>>(`/deployments/${id}/restart`);
    if (!response.data.data) throw new Error(response.data.message || 'Failed to restart deployment');
    return response.data.data;
  },

  runHealthCheck: async (id: string): Promise<DeploymentHealth> => {
    const response = await apiClient.post<ApiResponse<DeploymentHealth>>(`/deployments/${id}/health`);
    if (!response.data.data) throw new Error(response.data.message || 'Failed to run health check');
    return response.data.data;
  },

  listGpus: async (): Promise<GpuCatalogEntry[]> => {
    const response = await apiClient.get<ApiResponse<GpuCatalogEntry[]>>('/gpus');
    if (!response.data.data) throw new Error(response.data.message || 'Failed to fetch GPU catalog');
    return response.data.data;
  },

  recommendGpu: async (models: string[]): Promise<GpuRecommendation> => {
    const response = await apiClient.post<ApiResponse<GpuRecommendation>>('/gpus/recommend', { models });
    if (!response.data.data) throw new Error(response.data.message || 'Failed to recommend GPU');
    return response.data.data;
  },

  listModelCatalog: async (): Promise<ModelCatalogEntry[]> => {
    const response = await apiClient.get<ApiResponse<ModelCatalogEntry[]>>('/models/catalog');
    if (!response.data.data) throw new Error(response.data.message || 'Failed to fetch model catalog');
    return response.data.data;
  },

  listRegions: async (providerId: string, sortBy?: string): Promise<DeploymentRegion[]> => {
    const response = await apiClient.get<ApiResponse<DeploymentRegion[]>>('/regions', {
      params: { providerId, sortBy },
    });
    if (!response.data.data) throw new Error(response.data.message || 'Failed to fetch regions');
    return response.data.data;
  },

  listTemplates: async (): Promise<DeploymentTemplate[]> => {
    const response = await apiClient.get<ApiResponse<DeploymentTemplate[]>>('/templates');
    if (!response.data.data) throw new Error(response.data.message || 'Failed to fetch templates');
    return response.data.data;
  },
};
