import { useEffect } from 'react';
import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import { useQueryClient } from '@tanstack/react-query';
import { startSignalRHub } from '../utils/startSignalRHub';
import { tokenStorage } from '../utils/tokenStorage';

const hubUrl = import.meta.env.VITE_PLUGIN_HUB_URL || '/hubs/plugins';

export const usePluginHub = (organizationId?: string | null) => {
  const queryClient = useQueryClient();

  useEffect(() => {
    if (!organizationId) {
      return;
    }

    const token = tokenStorage.getAccessToken();
    if (!token) {
      return;
    }

    const connection = new HubConnectionBuilder()
      .withUrl(`${hubUrl}?access_token=${encodeURIComponent(token)}`)
      .withAutomaticReconnect()
      .configureLogging(LogLevel.Warning)
      .build();

    const invalidatePlugins = () => {
      queryClient.invalidateQueries({ queryKey: ['plugins', organizationId] });
      queryClient.invalidateQueries({ queryKey: ['plugin-dashboard', organizationId] });
    };

    const invalidateMcp = () => {
      queryClient.invalidateQueries({ queryKey: ['mcp-servers', organizationId] });
      queryClient.invalidateQueries({ queryKey: ['mcp-tools', organizationId] });
      queryClient.invalidateQueries({ queryKey: ['mcp-resources', organizationId] });
      queryClient.invalidateQueries({ queryKey: ['plugin-dashboard', organizationId] });
    };

    connection.on('PluginInstalled', invalidatePlugins);
    connection.on('PluginRemoved', invalidatePlugins);
    connection.on('PluginUpdated', invalidatePlugins);
    connection.on('McpConnected', invalidateMcp);
    connection.on('McpDisconnected', invalidateMcp);
    connection.on('ToolExecuted', invalidateMcp);

    const abortController = new AbortController();
    void startSignalRHub(connection, abortController.signal);

    return () => {
      abortController.abort();
      connection.stop();
    };
  }, [organizationId, queryClient]);
};
