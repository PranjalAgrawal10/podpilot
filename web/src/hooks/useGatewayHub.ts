import { useEffect } from 'react';
import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import { useQueryClient } from '@tanstack/react-query';
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

    connection.start().catch(() => {
      // SignalR is best-effort; dashboard still polls via React Query.
    });

    return () => {
      connection.stop();
    };
  }, [organizationId, queryClient]);
};
