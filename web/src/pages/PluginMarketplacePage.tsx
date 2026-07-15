import { Link } from 'react-router-dom';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { toast } from 'react-toastify';
import { Alert, Badge, Button, Card, CardBody, Col, Row, Spinner } from 'reactstrap';
import { pluginService } from '../services/pluginService';
import { useOrganization } from '../contexts/OrganizationContext';
import { usePluginHub } from '../hooks/usePluginHub';
import { PERMISSIONS } from '../types';

export const PluginMarketplacePage = () => {
  const queryClient = useQueryClient();
  const { currentOrganization, hasPermission } = useOrganization();
  const canRead = hasPermission(PERMISSIONS.PluginRead);
  const canManage = hasPermission(PERMISSIONS.PluginManage);
  usePluginHub(currentOrganization?.id);

  const { data: plugins = [], isLoading, error } = useQuery({
    queryKey: ['plugins', currentOrganization?.id],
    queryFn: pluginService.list,
    enabled: !!currentOrganization?.id && canRead,
  });

  const installMutation = useMutation({
    mutationFn: (packageId: string) => pluginService.install({ packageId }),
    onSuccess: () => {
      toast.success('Plugin installed');
      queryClient.invalidateQueries({ queryKey: ['plugins', currentOrganization?.id] });
      queryClient.invalidateQueries({ queryKey: ['plugin-dashboard', currentOrganization?.id] });
    },
    onError: (err: Error) => toast.error(err.message),
  });

  if (!currentOrganization) {
    return <Alert color="info">Select an organization to browse plugins.</Alert>;
  }

  if (!canRead) {
    return <Alert color="warning">You don&apos;t have permission to view plugins.</Alert>;
  }

  return (
    <div>
      <div className="d-flex justify-content-between align-items-center mb-4">
        <div>
          <h1 className="page-title mb-1">Plugin Marketplace</h1>
          <p className="text-muted mb-0">Local catalog of available plugin definitions.</p>
        </div>
        <Button tag={Link} to="/plugins" color="secondary" outline>
          Installed plugins
        </Button>
      </div>

      {isLoading && <div className="text-center py-5"><Spinner /></div>}
      {error && <Alert color="danger">{error instanceof Error ? error.message : 'Failed to load catalog'}</Alert>}
      {!isLoading && !error && plugins.length === 0 && (
        <Alert color="info">No plugins in the catalog yet.</Alert>
      )}

      <Row>
        {plugins.map((plugin) => {
          const installed = !!plugin.installationId;
          return (
            <Col key={plugin.id} md={6} lg={4} className="mb-4">
              <Card className="h-100">
                <CardBody>
                  <div className="d-flex justify-content-between mb-2">
                    <h5 className="mb-0">{plugin.name}</h5>
                    <Badge color={plugin.isFirstParty ? 'primary' : 'secondary'}>
                      {plugin.isFirstParty ? 'First-party' : plugin.pluginType}
                    </Badge>
                  </div>
                  <p className="text-muted small mb-1">{plugin.packageId} · v{plugin.version}</p>
                  <p className="text-muted small mb-3">{plugin.publisher}</p>
                  <p className="mb-3">{plugin.description || 'No description.'}</p>
                  {installed ? (
                    <Button tag={Link} to={`/plugins/${plugin.installationId}`} color="link" className="p-0">
                      View installation
                    </Button>
                  ) : (
                    canManage && (
                      <Button
                        color="primary"
                        size="sm"
                        disabled={installMutation.isPending}
                        onClick={() => installMutation.mutate(plugin.packageId)}
                      >
                        Install
                      </Button>
                    )
                  )}
                </CardBody>
              </Card>
            </Col>
          );
        })}
      </Row>
    </div>
  );
};
