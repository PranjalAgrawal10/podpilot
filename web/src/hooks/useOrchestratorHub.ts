import { useEffect } from 'react';
import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import { useQueryClient } from '@tanstack/react-query';
import { startSignalRHub } from '../utils/startSignalRHub';
import { tokenStorage } from '../utils/tokenStorage';

const hubUrl = import.meta.env.VITE_ORCHESTRATOR_HUB_URL || '/hubs/orchestrator';

export const useOrchestratorHub = (organizationId?: string | null) => {
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

    const events = [
      'PodAdded',
      'PodRemoved',
      'ScalingStarted',
      'ScalingCompleted',
      'PodFailed',
      'FailoverTriggered',
      'PoolUpdated',
    ];

    const invalidateAll = () => {
      queryClient.invalidateQueries({ queryKey: ['pod-pools', organizationId] });
      queryClient.invalidateQueries({ queryKey: ['orchestrator-status', organizationId] });
      queryClient.invalidateQueries({ queryKey: ['autoscaler-status', organizationId] });
      queryClient.invalidateQueries({ queryKey: ['capacity', organizationId] });
      queryClient.invalidateQueries({ queryKey: ['pod-health-metrics', organizationId] });
      queryClient.invalidateQueries({ queryKey: ['scaling-events', organizationId] });
    };

    events.forEach((eventName) => {
      connection.on(eventName, invalidateAll);
    });

    const abortController = new AbortController();
    void startSignalRHub(connection, abortController.signal);

    return () => {
      abortController.abort();
      connection.stop();
    };
  }, [organizationId, queryClient]);
};
