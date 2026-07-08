import { useEffect } from 'react';
import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import { useQueryClient } from '@tanstack/react-query';
import { startSignalRHub } from '../utils/startSignalRHub';
import { tokenStorage } from '../utils/tokenStorage';

const hubUrl = import.meta.env.VITE_MODEL_HUB_URL || '/hubs/models';

export const useModelHub = (organizationId?: string | null) => {
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
      queryClient.invalidateQueries({ queryKey: ['models', organizationId] });
      queryClient.invalidateQueries({ queryKey: ['model-dashboard', organizationId] });
      queryClient.invalidateQueries({ queryKey: ['model-downloads', organizationId] });
      queryClient.invalidateQueries({ queryKey: ['model-health', organizationId] });
    };

    [
      'ModelDownloadStarted',
      'ModelDownloadProgress',
      'ModelDownloadCompleted',
      'ModelDeleted',
      'HealthUpdated',
    ].forEach((eventName) => {
      connection.on(eventName, invalidate);
    });

    const abortController = new AbortController();
    void startSignalRHub(connection, abortController.signal);

    return () => {
      abortController.abort();
      connection.stop();
    };
  }, [organizationId, queryClient]);
};
