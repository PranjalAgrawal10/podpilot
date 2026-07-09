import apiClient from './api';
import type {
  ApiResponse,
  AutoScalerStatus,
  CapacityInfo,
  CreatePodPoolRequest,
  LoadBalancerConfig,
  ManualScaleRequest,
  OrchestratorStatus,
  PodHealthMetric,
  PodPool,
  ScalingActionResult,
  ScalingEvent,
  UpdateLoadBalancerConfigRequest,
  UpdatePodPoolRequest,
} from '../types';

export const orchestratorService = {
  listPodPools: async (): Promise<PodPool[]> => {
    const response = await apiClient.get<ApiResponse<PodPool[]>>('/pod-pools');
    if (!response.data.data) {
      throw new Error(response.data.message || 'Failed to fetch pod pools');
    }
    return response.data.data;
  },

  getPodPool: async (poolId: string): Promise<PodPool> => {
    const response = await apiClient.get<ApiResponse<PodPool>>(`/pod-pools/${poolId}`);
    if (!response.data.data) {
      throw new Error(response.data.message || 'Failed to fetch pod pool');
    }
    return response.data.data;
  },

  createPodPool: async (data: CreatePodPoolRequest): Promise<PodPool> => {
    const response = await apiClient.post<ApiResponse<PodPool>>('/pod-pools', data);
    if (!response.data.data) {
      throw new Error(response.data.message || 'Failed to create pod pool');
    }
    return response.data.data;
  },

  updatePodPool: async (poolId: string, data: UpdatePodPoolRequest): Promise<PodPool> => {
    const response = await apiClient.put<ApiResponse<PodPool>>(`/pod-pools/${poolId}`, data);
    if (!response.data.data) {
      throw new Error(response.data.message || 'Failed to update pod pool');
    }
    return response.data.data;
  },

  deletePodPool: async (poolId: string): Promise<void> => {
    await apiClient.delete(`/pod-pools/${poolId}`);
  },

  getOrchestratorStatus: async (): Promise<OrchestratorStatus> => {
    const response = await apiClient.get<ApiResponse<OrchestratorStatus>>('/orchestrator');
    if (!response.data.data) {
      throw new Error(response.data.message || 'Failed to fetch orchestrator status');
    }
    return response.data.data;
  },

  getAutoScalerStatus: async (): Promise<AutoScalerStatus> => {
    const response = await apiClient.get<ApiResponse<AutoScalerStatus>>('/autoscaler');
    if (!response.data.data) {
      throw new Error(response.data.message || 'Failed to fetch auto-scaler status');
    }
    return response.data.data;
  },

  getCapacity: async (poolId?: string): Promise<CapacityInfo> => {
    const response = await apiClient.get<ApiResponse<CapacityInfo>>('/capacity', {
      params: poolId ? { poolId } : undefined,
    });
    if (!response.data.data) {
      throw new Error(response.data.message || 'Failed to fetch capacity');
    }
    return response.data.data;
  },

  scaleUp: async (data: ManualScaleRequest): Promise<ScalingActionResult> => {
    const response = await apiClient.post<ApiResponse<ScalingActionResult>>('/autoscaler/scale-up', data);
    if (!response.data.data) {
      throw new Error(response.data.message || 'Failed to scale up');
    }
    return response.data.data;
  },

  scaleDown: async (data: ManualScaleRequest): Promise<ScalingActionResult> => {
    const response = await apiClient.post<ApiResponse<ScalingActionResult>>('/autoscaler/scale-down', data);
    if (!response.data.data) {
      throw new Error(response.data.message || 'Failed to scale down');
    }
    return response.data.data;
  },

  getLoadBalancerConfig: async (): Promise<LoadBalancerConfig> => {
    const response = await apiClient.get<ApiResponse<LoadBalancerConfig>>('/load-balancer');
    if (!response.data.data) {
      throw new Error(response.data.message || 'Failed to fetch load balancer config');
    }
    return response.data.data;
  },

  updateLoadBalancerConfig: async (data: UpdateLoadBalancerConfigRequest): Promise<LoadBalancerConfig> => {
    const response = await apiClient.put<ApiResponse<LoadBalancerConfig>>('/load-balancer', data);
    if (!response.data.data) {
      throw new Error(response.data.message || 'Failed to update load balancer config');
    }
    return response.data.data;
  },

  listPodHealthMetrics: async (podId?: string, limit = 100): Promise<PodHealthMetric[]> => {
    const response = await apiClient.get<ApiResponse<PodHealthMetric[]>>('/orchestrator/health', {
      params: { podId, limit },
    });
    if (!response.data.data) {
      throw new Error(response.data.message || 'Failed to fetch pod health metrics');
    }
    return response.data.data;
  },

  listScalingEvents: async (poolId?: string, limit = 50): Promise<ScalingEvent[]> => {
    const response = await apiClient.get<ApiResponse<ScalingEvent[]>>('/orchestrator/scaling-events', {
      params: { poolId, limit },
    });
    if (!response.data.data) {
      throw new Error(response.data.message || 'Failed to fetch scaling events');
    }
    return response.data.data;
  },
};
