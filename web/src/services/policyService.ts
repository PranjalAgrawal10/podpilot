import apiClient from './api';
import type { ApiResponse, OrganizationPolicies, UpdatePoliciesRequest } from '../types';

export const policyService = {
  get: async (): Promise<OrganizationPolicies> => {
    const response = await apiClient.get<ApiResponse<OrganizationPolicies>>('/policies');
    if (!response.data.data) throw new Error(response.data.message || 'Failed to fetch policies');
    return response.data.data;
  },

  update: async (data: UpdatePoliciesRequest): Promise<OrganizationPolicies> => {
    const response = await apiClient.put<ApiResponse<OrganizationPolicies>>('/policies', data);
    if (!response.data.data) throw new Error(response.data.message || 'Failed to update policies');
    return response.data.data;
  },
};
