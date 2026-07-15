import { Link } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { Alert, Spinner, Table } from 'reactstrap';
import { useRoutingHub } from '../hooks/useRoutingHub';
import { routingService } from '../services/routingService';
import { useOrganization } from '../contexts/OrganizationContext';
import { PERMISSIONS } from '../types';

export const RoutingDashboardPage = () => {
  const { currentOrganization, hasPermission } = useOrganization();
  const canRead = hasPermission(PERMISSIONS.RoutingRead);
  useRoutingHub(currentOrganization?.id);

  const { data, isLoading, error } = useQuery({
    queryKey: ['routing', currentOrganization?.id],
    queryFn: routingService.getDashboard,
    enabled: !!currentOrganization?.id && canRead,
    refetchInterval: 15000,
  });

  if (!canRead) return <Alert color="warning">Permission denied.</Alert>;
  if (isLoading) return <Spinner />;
  if (error) return <Alert color="danger">{(error as Error).message}</Alert>;
  if (!data) return null;

  return (
    <div>
      <div className="d-flex justify-content-between align-items-center mb-3">
        <h1 className="page-title mb-0">Intelligent Routing</h1>
        <div className="d-flex gap-2">
          <Link to="/routing/policies" className="btn btn-outline-primary btn-sm">Policies</Link>
          <Link to="/routing/models" className="btn btn-outline-primary btn-sm">Model Ranking</Link>
          <Link to="/routing/simulate" className="btn btn-primary btn-sm">Simulate</Link>
        </div>
      </div>

      <div className="row g-3 mb-4">
        <div className="col-md-3">
          <div className="p-3 border rounded h-100">
            <div className="text-muted small">Current Model</div>
            <div className="fs-5">{data.currentModel || '—'}</div>
          </div>
        </div>
        <div className="col-md-3">
          <div className="p-3 border rounded h-100">
            <div className="text-muted small">Current Provider</div>
            <div className="fs-5">{data.currentProvider || '—'}</div>
          </div>
        </div>
        <div className="col-md-2">
          <div className="p-3 border rounded h-100">
            <div className="text-muted small">Est. Cost</div>
            <div className="fs-5">${data.estimatedCostUsd.toFixed(6)}</div>
          </div>
        </div>
        <div className="col-md-2">
          <div className="p-3 border rounded h-100">
            <div className="text-muted small">Est. Latency</div>
            <div className="fs-5">{data.estimatedLatencyMs} ms</div>
          </div>
        </div>
        <div className="col-md-2">
          <div className="p-3 border rounded h-100">
            <div className="text-muted small">Fallbacks</div>
            <div className="fs-5">{data.fallbackCount}</div>
          </div>
        </div>
      </div>

      <p className="text-muted mb-4">Active strategy: <strong>{data.strategy}</strong></p>

      <div className="row g-4">
        <div className="col-lg-6">
          <h2 className="h5">Most Used Models</h2>
          <Table size="sm" responsive>
            <thead>
              <tr><th>Model</th><th>Count</th></tr>
            </thead>
            <tbody>
              {data.mostUsedModels.length === 0 && (
                <tr><td colSpan={2} className="text-muted">No routing history yet.</td></tr>
              )}
              {data.mostUsedModels.map((m) => (
                <tr key={m.modelName}><td>{m.modelName}</td><td>{m.count}</td></tr>
              ))}
            </tbody>
          </Table>
        </div>
        <div className="col-lg-6">
          <h2 className="h5">Provider Ranking</h2>
          <Table size="sm" responsive>
            <thead>
              <tr><th>Provider</th><th>Score</th><th>Availability</th></tr>
            </thead>
            <tbody>
              {data.providerRanking.length === 0 && (
                <tr><td colSpan={3} className="text-muted">Run a simulation to generate scores.</td></tr>
              )}
              {data.providerRanking.map((p) => (
                <tr key={p.providerId}>
                  <td>{p.providerName}</td>
                  <td>{p.score.toFixed(1)}</td>
                  <td>{p.availabilityScore.toFixed(0)}</td>
                </tr>
              ))}
            </tbody>
          </Table>
        </div>
      </div>
    </div>
  );
};
