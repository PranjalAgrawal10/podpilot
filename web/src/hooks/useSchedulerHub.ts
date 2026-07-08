import { useQuery } from '@tanstack/react-query';
import { useOrganization } from '../contexts/OrganizationContext';
import { schedulerService } from '../services/schedulerService';

export const useSchedulerHub = () => {
  const { currentOrganization } = useOrganization();
  const orgId = currentOrganization?.id;

  const queueQuery = useQuery({
    queryKey: ['scheduler-queue', orgId],
    queryFn: schedulerService.getQueue,
    enabled: !!orgId,
    refetchInterval: 5000,
  });

  const statusQuery = useQuery({
    queryKey: ['scheduler-status', orgId],
    queryFn: schedulerService.getStatus,
    enabled: !!orgId,
    refetchInterval: 5000,
  });

  const requestsQuery = useQuery({
    queryKey: ['scheduler-requests', orgId],
    queryFn: () => schedulerService.listRequests(),
    enabled: !!orgId,
    refetchInterval: 5000,
  });

  return { queueQuery, statusQuery, requestsQuery };
};
