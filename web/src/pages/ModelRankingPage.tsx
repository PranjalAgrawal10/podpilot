import { useQuery } from '@tanstack/react-query';
import { Alert, Spinner, Table } from 'reactstrap';
import { useRoutingHub } from '../hooks/useRoutingHub';
import { routingService } from '../services/routingService';
import { useOrganization } from '../contexts/OrganizationContext';
import { PERMISSIONS } from '../types';

export const ModelRankingPage = () => {
  const { currentOrganization, hasPermission } = useOrganization();
  const canRead = hasPermission(PERMISSIONS.RoutingRead);
  useRoutingHub(currentOrganization?.id);

  const { data = [], isLoading, error } = useQuery({
    queryKey: ['routing-models', currentOrganization?.id],
    queryFn: routingService.listModels,
    enabled: !!currentOrganization?.id && canRead,
  });

  if (!canRead) return <Alert color="warning">Permission denied.</Alert>;
  if (isLoading) return <Spinner />;
  if (error) return <Alert color="danger">{(error as Error).message}</Alert>;

  return (
    <div>
      <h1 className="page-title mb-3">Model Ranking</h1>
      <p className="text-muted">Weighted scores across cost, latency, reliability, context, features, and availability.</p>
      <Table responsive hover>
        <thead>
          <tr>
            <th>Model</th>
            <th>Provider</th>
            <th>Overall</th>
            <th>Cost</th>
            <th>Latency</th>
            <th>Reliability</th>
            <th>Features</th>
            <th>Availability</th>
          </tr>
        </thead>
        <tbody>
          {data.length === 0 && (
            <tr><td colSpan={8} className="text-muted">No scored models yet. Sync a provider catalog and run a simulation.</td></tr>
          )}
          {data.map((m) => (
            <tr key={`${m.providerId}-${m.modelId}-${m.strategy}`}>
              <td>{m.modelName}</td>
              <td>{m.providerName}</td>
              <td>{m.overallScore.toFixed(1)}</td>
              <td>{m.costScore.toFixed(0)}</td>
              <td>{m.latencyScore.toFixed(0)}</td>
              <td>{m.reliabilityScore.toFixed(0)}</td>
              <td>{m.featuresScore.toFixed(0)}</td>
              <td>{m.availabilityScore.toFixed(0)}</td>
            </tr>
          ))}
        </tbody>
      </Table>
    </div>
  );
};
