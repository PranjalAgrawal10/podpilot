import { useEffect } from 'react';
import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import { useQueryClient } from '@tanstack/react-query';
import { startSignalRHub } from '../utils/startSignalRHub';
import { tokenStorage } from '../utils/tokenStorage';

const hubUrl = import.meta.env.VITE_DEPLOYMENT_HUB_URL || '/hubs/deployments';

const DEPLOYMENT_EVENTS = [
  'DeploymentStarted',
  'DeploymentProgress',
  'ModelDownloadProgress',
  'DeploymentModelProgress',
  'HealthUpdated',
  'DeploymentHealth',
  'DeploymentReady',
  'DeploymentFailed',
] as const;

export const useDeploymentHub = (organizationId?: string | null, deploymentId?: string | null) => {
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

    const invalidateDeployments = () => {
      queryClient.invalidateQueries({ queryKey: ['deployments', organizationId] });
      queryClient.invalidateQueries({ queryKey: ['deployment-dashboard', organizationId] });
      if (deploymentId) {
        queryClient.invalidateQueries({ queryKey: ['deployment', deploymentId] });
      } else {
        queryClient.invalidateQueries({ queryKey: ['deployment'] });
      }
    };

    DEPLOYMENT_EVENTS.forEach((eventName) => {
      connection.on(eventName, invalidateDeployments);
    });

    const abortController = new AbortController();
    void startSignalRHub(connection, abortController.signal);

    return () => {
      abortController.abort();
      connection.stop();
    };
  }, [organizationId, deploymentId, queryClient]);
};
