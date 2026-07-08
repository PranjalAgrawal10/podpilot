import apiClient from './api';
import type {
  ApiResponse,
  AuthResponse,
  CreateOrganizationRequest,
  Organization,
  SwitchOrganizationRequest,
  UpdateOrganizationRequest,
} from '../types';

export const organizationService = {
  list: async (): Promise<Organization[]> => {
    const response = await apiClient.get<ApiResponse<Organization[]>>('/organizations');
    if (!response.data.data) {
      throw new Error(response.data.message || 'Failed to fetch organizations');
    }
    return response.data.data;
  },

  getById: async (id: string): Promise<Organization> => {
    const response = await apiClient.get<ApiResponse<Organization>>(`/organizations/${id}`);
    if (!response.data.data) {
      throw new Error(response.data.message || 'Failed to fetch organization');
    }
    return response.data.data;
  },

  create: async (data: CreateOrganizationRequest): Promise<Organization> => {
    const response = await apiClient.post<ApiResponse<Organization>>('/organizations', data);
    if (!response.data.data) {
      throw new Error(response.data.message || 'Failed to create organization');
    }
    return response.data.data;
  },

  update: async (id: string, data: UpdateOrganizationRequest): Promise<Organization> => {
    const response = await apiClient.put<ApiResponse<Organization>>(`/organizations/${id}`, data);
    if (!response.data.data) {
      throw new Error(response.data.message || 'Failed to update organization');
    }
    return response.data.data;
  },

  delete: async (id: string): Promise<void> => {
    await apiClient.delete(`/organizations/${id}`);
  },

  switch: async (data: SwitchOrganizationRequest): Promise<AuthResponse> => {
    const response = await apiClient.post<ApiResponse<AuthResponse>>('/organizations/switch', data);
    if (!response.data.data) {
      throw new Error(response.data.message || 'Failed to switch organization');
    }
    return response.data.data;
  },
};
