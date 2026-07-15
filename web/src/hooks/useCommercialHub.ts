import { useEffect } from 'react';
import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import { useQueryClient } from '@tanstack/react-query';
import { startSignalRHub } from '../utils/startSignalRHub';
import { tokenStorage } from '../utils/tokenStorage';

const hubUrl = import.meta.env.VITE_COMMERCIAL_HUB_URL || '/hubs/commercial';

export const useCommercialHub = (organizationId?: string | null) => {
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

    const invalidateCommercial = () => {
      queryClient.invalidateQueries({ queryKey: ['commercial-dashboard', organizationId] });
      queryClient.invalidateQueries({ queryKey: ['billing-subscription', organizationId] });
      queryClient.invalidateQueries({ queryKey: ['billing-usage', organizationId] });
      queryClient.invalidateQueries({ queryKey: ['billing-invoices', organizationId] });
      queryClient.invalidateQueries({ queryKey: ['billing-plans', organizationId] });
      queryClient.invalidateQueries({ queryKey: ['license', organizationId] });
    };

    connection.on('SubscriptionChanged', invalidateCommercial);
    connection.on('UsageThreshold', invalidateCommercial);
    connection.on('InvoiceGenerated', invalidateCommercial);
    connection.on('LicenseUpdated', invalidateCommercial);

    const abortController = new AbortController();
    void startSignalRHub(connection, abortController.signal);

    return () => {
      abortController.abort();
      connection.stop();
    };
  }, [organizationId, queryClient]);
};
