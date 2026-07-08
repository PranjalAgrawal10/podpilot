import { useEffect } from 'react';
import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import { useQueryClient } from '@tanstack/react-query';
import { startSignalRHub } from '../utils/startSignalRHub';
import { tokenStorage } from '../utils/tokenStorage';

const hubUrl = import.meta.env.VITE_GATEWAY_HUB_URL || '/hubs/gateway';

export const useGatewayHub = (organizationId?: string | null) => {
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
      'GatewayRequestStarted',
      'GatewayRequestFinished',
      'GatewayPodWake',
      'GatewayError',
    ];

    events.forEach((eventName) => {
      connection.on(eventName, () => {
        queryClient.invalidateQueries({ queryKey: ['gateway-stats', organizationId] });
        queryClient.invalidateQueries({ queryKey: ['gateway-requests', organizationId] });
      });
    });

    const abortController = new AbortController();
    void startSignalRHub(connection, abortController.signal);

    return () => {
      abortController.abort();
      connection.stop();
    };
  }, [organizationId, queryClient]);
};
