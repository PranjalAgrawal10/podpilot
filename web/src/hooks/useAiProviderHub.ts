import { useEffect } from 'react';
import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import { useQueryClient } from '@tanstack/react-query';
import { startSignalRHub } from '../utils/startSignalRHub';
import { tokenStorage } from '../utils/tokenStorage';

const hubUrl = import.meta.env.VITE_AI_PROVIDER_HUB_URL || '/hubs/ai-providers';

export const useAiProviderHub = (organizationId?: string | null) => {
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

    const invalidateProviders = () => {
      queryClient.invalidateQueries({ queryKey: ['ai-providers', organizationId] });
      queryClient.invalidateQueries({ queryKey: ['ai-dashboard', organizationId] });
    };

    const invalidateModels = () => {
      queryClient.invalidateQueries({ queryKey: ['ai-models', organizationId] });
    };

    const invalidateHealth = () => {
      queryClient.invalidateQueries({ queryKey: ['ai-provider-health', organizationId] });
      queryClient.invalidateQueries({ queryKey: ['ai-dashboard', organizationId] });
    };

    connection.on('AiProviderConnected', invalidateProviders);
    connection.on('AiProviderDisconnected', invalidateProviders);
    connection.on('AiProviderHealthChanged', invalidateHealth);
    connection.on('AiModelCatalogUpdated', invalidateModels);

    const abortController = new AbortController();
    void startSignalRHub(connection, abortController.signal);

    return () => {
      abortController.abort();
      connection.stop();
    };
  }, [organizationId, queryClient]);
};
