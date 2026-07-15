import apiClient from './api';
import type { ApiResponse, AuditEvent, ListAuditEventsParams } from '../types';

export const auditService = {
  list: async (params?: ListAuditEventsParams): Promise<AuditEvent[]> => {
    const response = await apiClient.get<ApiResponse<AuditEvent[]>>('/audit', { params });
    if (!response.data.data) throw new Error(response.data.message || 'Failed to fetch audit events');
    return response.data.data;
  },

  getById: async (id: string): Promise<AuditEvent> => {
    const response = await apiClient.get<ApiResponse<AuditEvent>>(`/audit/${id}`);
    if (!response.data.data) throw new Error(response.data.message || 'Failed to fetch audit event');
    return response.data.data;
  },
};
