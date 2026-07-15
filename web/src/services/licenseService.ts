import apiClient from './api';
import type {
  ActivateLicenseRequest,
  ApiResponse,
  IssuedLicense,
  IssueLicenseRequest,
  License,
} from '../types';

export const licenseService = {
  get: async (): Promise<License> => {
    const response = await apiClient.get<ApiResponse<License>>('/licenses');
    if (!response.data.data) throw new Error(response.data.message || 'Failed to fetch license');
    return response.data.data;
  },

  activate: async (data: ActivateLicenseRequest): Promise<License> => {
    const response = await apiClient.post<ApiResponse<License>>('/licenses/activate', data);
    if (!response.data.data) throw new Error(response.data.message || 'Failed to activate license');
    return response.data.data;
  },

  issue: async (data: IssueLicenseRequest): Promise<IssuedLicense> => {
    const response = await apiClient.post<ApiResponse<IssuedLicense>>('/licenses/issue', data);
    if (!response.data.data) throw new Error(response.data.message || 'Failed to issue license');
    return response.data.data;
  },
};
