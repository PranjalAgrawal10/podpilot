import { Link } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { Alert, Badge, Button, Card, CardBody, Col, Row, Spinner, Table } from 'reactstrap';
import { pluginService } from '../services/pluginService';
import { useOrganization } from '../contexts/OrganizationContext';
import { usePluginHub } from '../hooks/usePluginHub';
import { PERMISSIONS } from '../types';

export const PluginsPage = () => {
  const { currentOrganization, hasPermission } = useOrganization();
  const canRead = hasPermission(PERMISSIONS.PluginRead);
  usePluginHub(currentOrganization?.id);

  const { data: plugins = [], isLoading, error } = useQuery({
    queryKey: ['plugins', currentOrganization?.id],
    queryFn: pluginService.list,
    enabled: !!currentOrganization?.id && canRead,
  });

  const { data: dashboard } = useQuery({
    queryKey: ['plugin-dashboard', currentOrganization?.id],
    queryFn: pluginService.getDashboard,
    enabled: !!currentOrganization?.id && canRead,
  });

  const installed = plugins.filter((p) => !!p.installationId);

  if (!currentOrganization) {
    return <Alert color="info">Select an organization to manage plugins.</Alert>;
  }

  if (!canRead) {
    return <Alert color="warning">You don&apos;t have permission to view plugins.</Alert>;
  }

  return (
    <div>
      <div className="d-flex justify-content-between align-items-center mb-4">
        <div>
          <h1 className="page-title mb-1">Plugins</h1>
          <p className="text-muted mb-0">Extend PodPilot for {currentOrganization.name}.</p>
        </div>
        <Button tag={Link} to="/plugins/marketplace" color="primary">
          Marketplace
        </Button>
      </div>

      {dashboard && (
        <Row className="mb-4">
          <Col md={2}>
            <Card><CardBody><div className="text-muted">Installed</div><h3>{dashboard.installedPlugins}</h3></CardBody></Card>
          </Col>
          <Col md={2}>
            <Card><CardBody><div className="text-muted">Enabled</div><h3>{dashboard.enabledPlugins}</h3></CardBody></Card>
          </Col>
          <Col md={2}>
            <Card><CardBody><div className="text-muted">MCP Servers</div><h3>{dashboard.connectedMcpServers}</h3></CardBody></Card>
          </Col>
          <Col md={2}>
            <Card><CardBody><div className="text-muted">Tools</div><h3>{dashboard.availableTools}</h3></CardBody></Card>
          </Col>
          <Col md={2}>
            <Card><CardBody><div className="text-muted">Unhealthy</div><h3>{dashboard.unhealthyPlugins}</h3></CardBody></Card>
          </Col>
          <Col md={2}>
            <Card><CardBody><div className="text-muted">Executions</div><h3>{dashboard.recentExecutions}</h3></CardBody></Card>
          </Col>
        </Row>
      )}

      {isLoading && <div className="text-center py-5"><Spinner /></div>}
      {error && <Alert color="danger">{error instanceof Error ? error.message : 'Failed to load plugins'}</Alert>}
      {!isLoading && !error && installed.length === 0 && (
        <Alert color="info">
          No plugins installed yet. <Link to="/plugins/marketplace">Browse the marketplace</Link>
        </Alert>
      )}

      {!isLoading && installed.length > 0 && (
        <Table responsive hover>
          <thead>
            <tr>
              <th>Name</th>
              <th>Version</th>
              <th>Type</th>
              <th>Status</th>
              <th>Health</th>
              <th />
            </tr>
          </thead>
          <tbody>
            {installed.map((plugin) => (
              <tr key={plugin.installationId!}>
                <td>
                  <div className="fw-semibold">{plugin.name}</div>
                  <div className="text-muted small">{plugin.packageId}</div>
                </td>
                <td>{plugin.version}</td>
                <td><Badge color="secondary">{plugin.pluginType}</Badge></td>
                <td>{plugin.status || '—'}</td>
                <td>
                  {plugin.isHealthy == null ? (
                    '—'
                  ) : (
                    <Badge color={plugin.isHealthy ? 'success' : 'danger'}>
                      {plugin.isHealthy ? 'Healthy' : 'Unhealthy'}
                    </Badge>
                  )}
                </td>
                <td className="text-end">
                  <Button tag={Link} to={`/plugins/${plugin.installationId}`} color="link" className="p-0">
                    Details
                  </Button>
                </td>
              </tr>
            ))}
          </tbody>
        </Table>
      )}
    </div>
  );
};
