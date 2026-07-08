import apiClient from './api';
import type {
  AddMemberRequest,
  ApiResponse,
  Member,
  UpdateMemberRoleRequest,
} from '../types';

export const memberService = {
  list: async (organizationId: string): Promise<Member[]> => {
    const response = await apiClient.get<ApiResponse<Member[]>>(
      `/organizations/${organizationId}/members`,
    );
    if (!response.data.data) {
      throw new Error(response.data.message || 'Failed to fetch members');
    }
    return response.data.data;
  },

  add: async (organizationId: string, data: AddMemberRequest): Promise<Member> => {
    const response = await apiClient.post<ApiResponse<Member>>(
      `/organizations/${organizationId}/members`,
      data,
    );
    if (!response.data.data) {
      throw new Error(response.data.message || 'Failed to add member');
    }
    return response.data.data;
  },

  remove: async (organizationId: string, memberId: string): Promise<void> => {
    await apiClient.delete(`/organizations/${organizationId}/members/${memberId}`);
  },

  updateRole: async (
    organizationId: string,
    memberId: string,
    data: UpdateMemberRoleRequest,
  ): Promise<Member> => {
    const response = await apiClient.put<ApiResponse<Member>>(
      `/organizations/${organizationId}/members/${memberId}/role`,
      data,
    );
    if (!response.data.data) {
      throw new Error(response.data.message || 'Failed to update member role');
    }
    return response.data.data;
  },
};
