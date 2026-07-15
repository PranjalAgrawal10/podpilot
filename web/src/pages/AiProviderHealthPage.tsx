import { useQuery } from '@tanstack/react-query';
import { Alert, Badge, Spinner, Table } from 'reactstrap';
import { aiProviderService } from '../services/aiProviderService';
import { useOrganization } from '../contexts/OrganizationContext';
import { useAiProviderHub } from '../hooks/useAiProviderHub';
import { PERMISSIONS } from '../types';

export const AiProviderHealthPage = () => {
  const { currentOrganization, hasPermission } = useOrganization();
  const canRead = hasPermission(PERMISSIONS.AiProviderRead);
  useAiProviderHub(currentOrganization?.id);

  const { data: providers = [], isLoading, error } = useQuery({
    queryKey: ['ai-providers', currentOrganization?.id],
    queryFn: aiProviderService.list,
    enabled: !!currentOrganization?.id && canRead,
  });

  const { data: healthEntries = [], isLoading: healthLoading } = useQuery({
    queryKey: ['ai-provider-health', currentOrganization?.id, providers.map((p) => p.id).join(',')],
    queryFn: async () => {
      const results = await Promise.all(
        providers.map(async (provider) => {
          try {
            const health = await aiProviderService.getHealth(provider.id);
            return { provider, health };
          } catch {
            return { provider, health: null };
          }
        }),
      );
      return results;
    },
    enabled: !!currentOrganization?.id && canRead && providers.length > 0,
    refetchInterval: 60000,
  });

  if (!canRead) return <Alert color="warning">Permission denied.</Alert>;

  return (
    <div>
      <h1 className="page-title mb-3">AI Provider Health</h1>
      {(isLoading || healthLoading) && <Spinner />}
      {error && <Alert color="danger">{error instanceof Error ? error.message : 'Failed to load'}</Alert>}
      {healthEntries.length > 0 && (
        <Table responsive hover>
          <thead>
            <tr>
              <th>Provider</th>
              <th>Kind</th>
              <th>Status</th>
              <th>Latency</th>
              <th>Last checked</th>
            </tr>
          </thead>
          <tbody>
            {healthEntries.map(({ provider, health }) => (
              <tr key={provider.id}>
                <td>{provider.displayName}</td>
                <td>{provider.providerKind}</td>
                <td>
                  <Badge color={health?.status === 'Healthy' ? 'success' : health?.status === 'Degraded' ? 'warning' : 'danger'}>
                    {health?.status || 'Unknown'}
                  </Badge>
                </td>
                <td>{health?.latencyMs != null ? `${health.latencyMs} ms` : '—'}</td>
                <td>{health?.lastCheckedAt ? new Date(health.lastCheckedAt).toLocaleString() : '—'}</td>
              </tr>
            ))}
          </tbody>
        </Table>
      )}
    </div>
  );
};
