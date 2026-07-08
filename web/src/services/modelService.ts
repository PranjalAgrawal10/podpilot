import { apiClient } from './api';
import type {
  AiModel,
  AiModelDetail,
  ApiResponse,
  ModelDashboard,
  ModelDownload,
  ModelHealthRecord,
} from '../types';

export const modelService = {
  async list(podId?: string): Promise<AiModel[]> {
    const response = await apiClient.get<ApiResponse<AiModel[]>>('/models', {
      params: podId ? { podId } : undefined,
    });
    return response.data.data ?? [];
  },

  async getDashboard(podId?: string): Promise<ModelDashboard> {
    const response = await apiClient.get<ApiResponse<ModelDashboard>>('/models/dashboard', {
      params: podId ? { podId } : undefined,
    });
    return response.data.data!;
  },

  async getById(modelId: string): Promise<AiModelDetail> {
    const response = await apiClient.get<ApiResponse<AiModelDetail>>(`/models/${modelId}`);
    return response.data.data!;
  },

  async pull(payload: { podId: string; model: string }): Promise<ModelDownload> {
    const response = await apiClient.post<ApiResponse<ModelDownload>>('/models/pull', payload);
    return response.data.data!;
  },

  async delete(modelId: string, forceDefault = false): Promise<void> {
    await apiClient.delete(`/models/${modelId}`, { params: { forceDefault } });
  },

  async setDefault(modelId: string): Promise<AiModel> {
    const response = await apiClient.post<ApiResponse<AiModel>>(`/models/${modelId}/default`);
    return response.data.data!;
  },

  async refresh(podId: string): Promise<AiModel[]> {
    const response = await apiClient.post<ApiResponse<AiModel[]>>('/models/refresh', { podId });
    return response.data.data ?? [];
  },

  async listDownloads(activeOnly = true): Promise<ModelDownload[]> {
    const response = await apiClient.get<ApiResponse<ModelDownload[]>>('/models/downloads', {
      params: { activeOnly },
    });
    return response.data.data ?? [];
  },

  async listHealth(modelId?: string): Promise<ModelHealthRecord[]> {
    const response = await apiClient.get<ApiResponse<ModelHealthRecord[]>>('/models/health', {
      params: modelId ? { modelId } : undefined,
    });
    return response.data.data ?? [];
  },
};
