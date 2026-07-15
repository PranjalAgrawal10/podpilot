import { useEffect } from 'react';
import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import { useQueryClient } from '@tanstack/react-query';
import { startSignalRHub } from '../utils/startSignalRHub';
import { tokenStorage } from '../utils/tokenStorage';

const hubUrl = import.meta.env.VITE_SECURITY_HUB_URL || '/hubs/security';

export const useSecurityHub = (organizationId?: string | null) => {
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

    const invalidateSecurity = () => {
      queryClient.invalidateQueries({ queryKey: ['security-dashboard', organizationId] });
      queryClient.invalidateQueries({ queryKey: ['audit', organizationId] });
      queryClient.invalidateQueries({ queryKey: ['security-sessions', organizationId] });
      queryClient.invalidateQueries({ queryKey: ['security-devices', organizationId] });
    };

    connection.on('SecurityAlert', invalidateSecurity);
    connection.on('AuditEvent', invalidateSecurity);
    connection.on('PolicyViolation', invalidateSecurity);
    connection.on('NewLogin', invalidateSecurity);
    connection.on('ProviderCredentialChange', invalidateSecurity);

    const abortController = new AbortController();
    void startSignalRHub(connection, abortController.signal);

    return () => {
      abortController.abort();
      connection.stop();
    };
  }, [organizationId, queryClient]);
};
