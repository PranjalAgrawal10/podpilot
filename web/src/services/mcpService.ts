import apiClient from './api';
import type {
  ApiResponse,
  CreateMcpServerRequest,
  ExecuteMcpToolRequest,
  ExecuteMcpToolResponse,
  McpResource,
  McpServer,
  McpServerKind,
  McpTool,
} from '../types';

export const mcpService = {
  listServers: async (): Promise<McpServer[]> => {
    const response = await apiClient.get<ApiResponse<McpServer[]>>('/mcp/servers');
    if (!response.data.data) throw new Error(response.data.message || 'Failed to fetch MCP servers');
    return response.data.data;
  },

  createServer: async (data: CreateMcpServerRequest): Promise<McpServer> => {
    const response = await apiClient.post<ApiResponse<McpServer>>('/mcp/servers', data);
    if (!response.data.data) throw new Error(response.data.message || 'Failed to create MCP server');
    return response.data.data;
  },

  deleteServer: async (id: string): Promise<void> => {
    await apiClient.delete(`/mcp/servers/${id}`);
  },

  listKinds: async (): Promise<McpServerKind[]> => {
    const response = await apiClient.get<ApiResponse<McpServerKind[]>>('/mcp/kinds');
    if (!response.data.data) throw new Error(response.data.message || 'Failed to fetch MCP kinds');
    return response.data.data;
  },

  listTools: async (): Promise<McpTool[]> => {
    const response = await apiClient.get<ApiResponse<McpTool[]>>('/mcp/tools');
    if (!response.data.data) throw new Error(response.data.message || 'Failed to fetch MCP tools');
    return response.data.data;
  },

  listResources: async (): Promise<McpResource[]> => {
    const response = await apiClient.get<ApiResponse<McpResource[]>>('/mcp/resources');
    if (!response.data.data) throw new Error(response.data.message || 'Failed to fetch MCP resources');
    return response.data.data;
  },

  executeTool: async (data: ExecuteMcpToolRequest): Promise<ExecuteMcpToolResponse> => {
    const response = await apiClient.post<ApiResponse<ExecuteMcpToolResponse>>('/mcp/tools/execute', data);
    if (!response.data.data) throw new Error(response.data.message || 'Failed to execute MCP tool');
    return response.data.data;
  },
};
