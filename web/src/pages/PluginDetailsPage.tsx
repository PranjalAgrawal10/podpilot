import { Link, useNavigate, useParams } from 'react-router-dom';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { toast } from 'react-toastify';
import { Alert, Badge, Button, Card, CardBody, Spinner } from 'reactstrap';
import { pluginService } from '../services/pluginService';
import { useOrganization } from '../contexts/OrganizationContext';
import { usePluginHub } from '../hooks/usePluginHub';
import { PERMISSIONS } from '../types';

export const PluginDetailsPage = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const { currentOrganization, hasPermission } = useOrganization();
  const canRead = hasPermission(PERMISSIONS.PluginRead);
  const canManage = hasPermission(PERMISSIONS.PluginManage);
  usePluginHub(currentOrganization?.id);

  const installationId = id;

  const { data: plugin, isLoading, error } = useQuery({
    queryKey: ['plugins', currentOrganization?.id, installationId],
    queryFn: () => pluginService.getById(installationId!),
    enabled: !!currentOrganization?.id && !!installationId && canRead,
  });

  const invalidate = () => {
    queryClient.invalidateQueries({ queryKey: ['plugins', currentOrganization?.id] });
    queryClient.invalidateQueries({ queryKey: ['plugin-dashboard', currentOrganization?.id] });
  };

  const enableMutation = useMutation({
    mutationFn: () => pluginService.enable(installationId!),
    onSuccess: () => {
      toast.success('Plugin enabled');
      invalidate();
    },
    onError: (err: Error) => toast.error(err.message),
  });

  const disableMutation = useMutation({
    mutationFn: () => pluginService.disable(installationId!),
    onSuccess: () => {
      toast.success('Plugin disabled');
      invalidate();
    },
    onError: (err: Error) => toast.error(err.message),
  });

  const uninstallMutation = useMutation({
    mutationFn: () => pluginService.uninstall(installationId!),
    onSuccess: () => {
      toast.success('Plugin uninstalled');
      invalidate();
      navigate('/plugins');
    },
    onError: (err: Error) => toast.error(err.message),
  });

  if (!canRead) return <Alert color="warning">Permission denied.</Alert>;
  if (isLoading) return <div className="text-center py-5"><Spinner /></div>;
  if (error || !plugin) {
    return <Alert color="danger">{error instanceof Error ? error.message : 'Plugin not found'}</Alert>;
  }

  const isEnabled = plugin.status?.toLowerCase() === 'enabled';
  const busy = enableMutation.isPending || disableMutation.isPending || uninstallMutation.isPending;

  return (
    <div>
      <div className="d-flex justify-content-between align-items-start mb-4">
        <div>
          <h1 className="page-title mb-1">{plugin.name}</h1>
          <p className="text-muted mb-0">
            {plugin.packageId} · v{plugin.version} · {plugin.pluginType}
          </p>
        </div>
        <div className="d-flex gap-2 flex-wrap">
          <Button tag={Link} to="/plugins" color="secondary" outline>Back</Button>
          {canManage && (
            <>
              <Button
                tag={Link}
                to={`/plugins/${installationId}/settings`}
                color="primary"
                outline
              >
                Settings
              </Button>
              {isEnabled ? (
                <Button
                  color="warning"
                  outline
                  disabled={busy}
                  onClick={() => disableMutation.mutate()}
                >
                  Disable
                </Button>
              ) : (
                <Button
                  color="success"
                  outline
                  disabled={busy}
                  onClick={() => enableMutation.mutate()}
                >
                  Enable
                </Button>
              )}
              <Button
                color="danger"
                outline
                disabled={busy}
                onClick={() => {
                  if (window.confirm(`Uninstall ${plugin.name}?`)) {
                    uninstallMutation.mutate();
                  }
                }}
              >
                Uninstall
              </Button>
            </>
          )}
        </div>
      </div>

      <Card className="mb-3">
        <CardBody>
          <p><strong>Status:</strong> <Badge color={isEnabled ? 'success' : 'secondary'}>{plugin.status || 'Unknown'}</Badge></p>
          <p>
            <strong>Health:</strong>{' '}
            {plugin.isHealthy == null ? (
              '—'
            ) : (
              <Badge color={plugin.isHealthy ? 'success' : 'danger'}>
                {plugin.isHealthy ? 'Healthy' : 'Unhealthy'}
              </Badge>
            )}
            {plugin.healthMessage ? ` · ${plugin.healthMessage}` : ''}
          </p>
          <p><strong>Publisher:</strong> {plugin.publisher}</p>
          <p><strong>First-party:</strong> {plugin.isFirstParty ? 'Yes' : 'No'}</p>
          <p><strong>Installation ID:</strong> {plugin.installationId || installationId}</p>
          <p><strong>Enabled at:</strong> {plugin.enabledAt ? new Date(plugin.enabledAt).toLocaleString() : '—'}</p>
          <p className="mb-0"><strong>Description:</strong> {plugin.description || '—'}</p>
        </CardBody>
      </Card>

      <Card>
        <CardBody>
          <h2 className="h6">Permissions</h2>
          <p className="mb-1"><strong>Required:</strong> {plugin.requiredPermissions.join(', ') || 'None'}</p>
          <p className="mb-0"><strong>Granted:</strong> {plugin.grantedPermissions.join(', ') || 'None'}</p>
        </CardBody>
      </Card>
    </div>
  );
};
