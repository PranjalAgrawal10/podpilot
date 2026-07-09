import { useEffect } from 'react';
import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import { useQueryClient } from '@tanstack/react-query';
import { startSignalRHub } from '../utils/startSignalRHub';
import { tokenStorage } from '../utils/tokenStorage';

const hubUrl = import.meta.env.VITE_OBSERVABILITY_HUB_URL || '/hubs/observability';

export const useObservabilityHub = (organizationId?: string | null) => {
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

    const invalidateMetrics = () => {
      queryClient.invalidateQueries({ queryKey: ['live-metrics', organizationId] });
      queryClient.invalidateQueries({ queryKey: ['metrics', organizationId] });
    };

    const invalidateCost = () => {
      queryClient.invalidateQueries({ queryKey: ['cost', organizationId] });
    };

    const invalidateAlerts = () => {
      queryClient.invalidateQueries({ queryKey: ['alerts', organizationId] });
    };

    const invalidatePodHealth = () => {
      queryClient.invalidateQueries({ queryKey: ['pod-health-overview', organizationId] });
    };

    const invalidateProviderHealth = () => {
      queryClient.invalidateQueries({ queryKey: ['provider-health-overview', organizationId] });
    };

    const invalidateQueue = () => {
      queryClient.invalidateQueries({ queryKey: ['live-metrics', organizationId] });
      queryClient.invalidateQueries({ queryKey: ['analytics', organizationId] });
    };

    connection.on('MetricsUpdated', invalidateMetrics);
    connection.on('CostUpdated', invalidateCost);
    connection.on('AlertRaised', invalidateAlerts);
    connection.on('PodHealthChanged', invalidatePodHealth);
    connection.on('ProviderHealthChanged', invalidateProviderHealth);
    connection.on('QueueUpdated', invalidateQueue);

    const abortController = new AbortController();
    void startSignalRHub(connection, abortController.signal);

    return () => {
      abortController.abort();
      connection.stop();
    };
  }, [organizationId, queryClient]);
};
