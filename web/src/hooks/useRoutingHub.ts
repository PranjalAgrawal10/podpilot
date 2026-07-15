import { useEffect } from 'react';
import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import { useQueryClient } from '@tanstack/react-query';
import { startSignalRHub } from '../utils/startSignalRHub';
import { tokenStorage } from '../utils/tokenStorage';

const hubUrl = import.meta.env.VITE_ROUTING_HUB_URL || '/hubs/routing';

export const useRoutingHub = (organizationId?: string | null) => {
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

    const invalidate = () => {
      queryClient.invalidateQueries({ queryKey: ['routing', organizationId] });
      queryClient.invalidateQueries({ queryKey: ['routing-models', organizationId] });
      queryClient.invalidateQueries({ queryKey: ['routing-history', organizationId] });
      queryClient.invalidateQueries({ queryKey: ['routing-policy', organizationId] });
    };

    connection.on('RoutingDecision', invalidate);
    connection.on('ProviderChanged', invalidate);
    connection.on('FallbackOccurred', invalidate);
    connection.on('PolicyUpdated', invalidate);

    const abortController = new AbortController();
    void startSignalRHub(connection, abortController.signal);

    return () => {
      abortController.abort();
      connection.stop();
    };
  }, [organizationId, queryClient]);
};
