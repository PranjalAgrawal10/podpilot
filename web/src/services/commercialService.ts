import apiClient from './api';
import type {
  ApiResponse,
  BackupJob,
  CommercialDashboard,
  ReleaseStatus,
  StartBackupRequest,
  SystemStatus,
  TelemetryPreference,
} from '../types';

export const commercialService = {
  getDashboard: async (): Promise<CommercialDashboard> => {
    const response = await apiClient.get<ApiResponse<CommercialDashboard>>('/commercial/dashboard');
    if (!response.data.data) throw new Error(response.data.message || 'Failed to fetch commercial dashboard');
    return response.data.data;
  },

  getTelemetry: async (): Promise<TelemetryPreference> => {
    const response = await apiClient.get<ApiResponse<TelemetryPreference>>('/telemetry');
    if (!response.data.data) throw new Error(response.data.message || 'Failed to fetch telemetry preference');
    return response.data.data;
  },

  updateTelemetry: async (preference: TelemetryPreference): Promise<TelemetryPreference> => {
    const response = await apiClient.put<ApiResponse<TelemetryPreference>>('/telemetry', preference);
    if (!response.data.data) throw new Error(response.data.message || 'Failed to update telemetry preference');
    return response.data.data;
  },

  listBackups: async (): Promise<BackupJob[]> => {
    const response = await apiClient.get<ApiResponse<BackupJob[]>>('/backups');
    if (!response.data.data) throw new Error(response.data.message || 'Failed to fetch backups');
    return response.data.data;
  },

  startBackup: async (data?: StartBackupRequest): Promise<BackupJob> => {
    const response = await apiClient.post<ApiResponse<BackupJob>>('/backups', data ?? {});
    if (!response.data.data) throw new Error(response.data.message || 'Failed to start backup');
    return response.data.data;
  },

  restoreBackup: async (id: string): Promise<void> => {
    await apiClient.post(`/backups/${id}/restore`);
  },

  getReleaseStatus: async (): Promise<ReleaseStatus> => {
    const response = await apiClient.get<ApiResponse<ReleaseStatus>>('/releases/status');
    if (!response.data.data) throw new Error(response.data.message || 'Failed to fetch release status');
    return response.data.data;
  },

  getSystemStatus: async (): Promise<SystemStatus> => {
    const response = await apiClient.get<ApiResponse<SystemStatus>>('/status');
    if (!response.data.data) throw new Error(response.data.message || 'Failed to fetch system status');
    return response.data.data;
  },
};
