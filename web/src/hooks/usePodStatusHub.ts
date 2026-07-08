import { useEffect } from 'react';
import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import { useQueryClient } from '@tanstack/react-query';
import { tokenStorage } from '../utils/tokenStorage';

const hubUrl = import.meta.env.VITE_HUB_URL || '/hubs/pods';

export const usePodStatusHub = (organizationId?: string | null) => {
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

    const lifecycleEvents = [
      'PodStatusChanged',
      'PodStarted',
      'PodStopped',
      'PodSleeping',
      'PodWaking',
      'IdleDetected',
      'WakeCompleted',
      'ShutdownCompleted',
      'PolicyUpdated',
    ];

    lifecycleEvents.forEach((eventName) => {
      connection.on(eventName, () => {
        queryClient.invalidateQueries({ queryKey: ['pods', organizationId] });
        queryClient.invalidateQueries({ queryKey: ['pod'] });
        queryClient.invalidateQueries({ queryKey: ['pod-lifecycle'] });
        queryClient.invalidateQueries({ queryKey: ['pod-activity'] });
      });
    });

    connection.start().catch(() => {
      // SignalR is best-effort; polling still works via React Query.
    });

    return () => {
      connection.stop();
    };
  }, [organizationId, queryClient]);
};
