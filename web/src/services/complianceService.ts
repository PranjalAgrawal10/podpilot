import apiClient from './api';
import type { ApiResponse, ComplianceExportResult, ComplianceStatus } from '../types';

export const complianceService = {
  getStatus: async (): Promise<ComplianceStatus> => {
    const response = await apiClient.get<ApiResponse<ComplianceStatus>>('/compliance');
    if (!response.data.data) throw new Error(response.data.message || 'Failed to fetch compliance status');
    return response.data.data;
  },

  export: async (): Promise<ComplianceExportResult> => {
    const response = await apiClient.post<ApiResponse<ComplianceExportResult>>('/compliance/export');
    if (!response.data.data) throw new Error(response.data.message || 'Failed to export compliance data');
    return response.data.data;
  },

  erase: async (targetUserId: string): Promise<void> => {
    await apiClient.post('/compliance/erasure', { targetUserId });
  },
};
