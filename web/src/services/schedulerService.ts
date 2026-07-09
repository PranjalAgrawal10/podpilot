import api from './api';
import type { ApiResponse } from '../types';

export interface SchedulerRequest {
  id: string;
  organizationId: string;
  podId: string;
  model?: string;
  path: string;
  status: string;
  priority: string;
  createdAt: string;
  startedAt: string;
  completedAt?: string;
  queueTimeMs?: number;
  executionTimeMs?: number;
  retryCount: number;
  isStreaming: boolean;
}

export interface QueueStatus {
  queueLength: number;
  runningRequests: number;
  streamingRequests: number;
  failedRequestsLastHour: number;
  averageWaitTimeMs: number;
  averageExecutionTimeMs: number;
}

export interface SchedulerStatus {
  isHealthy: boolean;
  activeTrackedRequests: number;
  totalQueuedRequests: number;
  totalRunningRequests: number;
  retriesLastHour: number;
  podUtilizationPercent: number;
}

export const schedulerService = {
  listRequests: async (status?: string): Promise<SchedulerRequest[]> => {
    const response = await api.get<ApiResponse<SchedulerRequest[]>>('/requests', {
      params: status ? { status } : undefined,
    });
    return response.data.data ?? [];
  },

  getRequest: async (id: string): Promise<SchedulerRequest> => {
    const response = await api.get<ApiResponse<SchedulerRequest>>(`/requests/${id}`);
    if (!response.data.data) {
      throw new Error('Request not found');
    }
    return response.data.data;
  },

  cancelRequest: async (id: string): Promise<boolean> => {
    const response = await api.post<ApiResponse<boolean>>(`/requests/${id}/cancel`);
    return response.data.data ?? false;
  },

  getQueue: async (): Promise<QueueStatus> => {
    const response = await api.get<ApiResponse<QueueStatus>>('/queue');
    if (!response.data.data) {
      throw new Error('Failed to load queue status');
    }
    return response.data.data;
  },

  getStatus: async (): Promise<SchedulerStatus> => {
    const response = await api.get<ApiResponse<SchedulerStatus>>('/scheduler/status');
    if (!response.data.data) {
      throw new Error('Failed to load scheduler status');
    }
    return response.data.data;
  },
};
