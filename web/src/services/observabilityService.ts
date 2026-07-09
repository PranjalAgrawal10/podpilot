import { apiClient } from './api';
import type {
  AnalyticsSummary,
  ApiResponse,
  CostSummary,
  LiveMetrics,
  MetricsPeriod,
  MetricsSnapshot,
  ObservabilityAlert,
  ObservabilityExportFormat,
  ObservabilityExportType,
  PodHealthOverview,
  ProviderHealthOverview,
  SystemHealth,
} from '../types';

export interface MetricsQueryParams {
  from?: string;
  to?: string;
  providerId?: string;
  podId?: string;
  model?: string;
  limit?: number;
}

export interface CostQueryParams {
  period?: MetricsPeriod;
  providerId?: string;
  podId?: string;
  model?: string;
}

export interface AnalyticsQueryParams extends CostQueryParams {
  from?: string;
  to?: string;
}

export interface ExportQueryParams {
  format?: ObservabilityExportFormat;
  type?: ObservabilityExportType;
  from?: string;
  to?: string;
  providerId?: string;
  podId?: string;
  model?: string;
}

export const observabilityService = {
  async getMetrics(params?: MetricsQueryParams): Promise<MetricsSnapshot[]> {
    const response = await apiClient.get<ApiResponse<MetricsSnapshot[]>>('/metrics', { params });
    return response.data.data ?? [];
  },

  async getLiveMetrics(): Promise<LiveMetrics> {
    const response = await apiClient.get<ApiResponse<LiveMetrics>>('/metrics/live');
    if (!response.data.data) {
      throw new Error(response.data.message || 'Failed to fetch live metrics');
    }
    return response.data.data;
  },

  async getCost(params?: CostQueryParams): Promise<CostSummary> {
    const response = await apiClient.get<ApiResponse<CostSummary>>('/cost', { params });
    if (!response.data.data) {
      throw new Error(response.data.message || 'Failed to fetch cost summary');
    }
    return response.data.data;
  },

  async getAnalytics(params?: AnalyticsQueryParams): Promise<AnalyticsSummary> {
    const response = await apiClient.get<ApiResponse<AnalyticsSummary>>('/analytics', { params });
    if (!response.data.data) {
      throw new Error(response.data.message || 'Failed to fetch analytics');
    }
    return response.data.data;
  },

  async getSystemHealth(): Promise<SystemHealth> {
    const response = await apiClient.get<ApiResponse<SystemHealth>>('/health/system');
    if (!response.data.data) {
      throw new Error(response.data.message || 'Failed to fetch system health');
    }
    return response.data.data;
  },

  async getPodHealth(): Promise<PodHealthOverview> {
    const response = await apiClient.get<ApiResponse<PodHealthOverview>>('/health/pods');
    if (!response.data.data) {
      throw new Error(response.data.message || 'Failed to fetch pod health');
    }
    return response.data.data;
  },

  async getProviderHealth(): Promise<ProviderHealthOverview> {
    const response = await apiClient.get<ApiResponse<ProviderHealthOverview>>('/health/providers');
    if (!response.data.data) {
      throw new Error(response.data.message || 'Failed to fetch provider health');
    }
    return response.data.data;
  },

  async listAlerts(activeOnly = false, limit = 50): Promise<ObservabilityAlert[]> {
    const response = await apiClient.get<ApiResponse<ObservabilityAlert[]>>('/alerts', {
      params: { activeOnly, limit },
    });
    return response.data.data ?? [];
  },

  async exportData(params: ExportQueryParams): Promise<Blob> {
    const response = await apiClient.get<Blob>('/observability/export', {
      params,
      responseType: 'blob',
    });
    return response.data;
  },
};
