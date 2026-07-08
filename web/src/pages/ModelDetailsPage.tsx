import { Link, useParams } from 'react-router-dom';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { Alert, Button, Card, CardBody, CardTitle, Table } from 'reactstrap';
import { toast } from 'react-toastify';
import { useOrganization } from '../contexts/OrganizationContext';
import { modelService } from '../services/modelService';
import { DefaultBadge } from '../components/models/DefaultBadge';
import { HealthBadge } from '../components/models/HealthBadge';
import { LoadingSpinner } from '../components/LoadingSpinner';
import { PERMISSIONS } from '../types';
import { formatBytes } from '../utils/formatBytes';

export const ModelDetailsPage = () => {
  const { id } = useParams<{ id: string }>();
  const queryClient = useQueryClient();
  const { currentOrganization, hasPermission } = useOrganization();
  const canRead = hasPermission(PERMISSIONS.ModelRead);
  const canManage = hasPermission(PERMISSIONS.ModelManage);

  const { data: model, isLoading, error } = useQuery({
    queryKey: ['model', id, currentOrganization?.id],
    queryFn: () => modelService.getById(id!),
    enabled: !!id && !!currentOrganization?.id && canRead,
  });

  const defaultMutation = useMutation({
    mutationFn: () => modelService.setDefault(id!),
    onSuccess: () => {
      toast.success('Default model updated');
      queryClient.invalidateQueries({ queryKey: ['model', id] });
    },
  });

  if (!canRead) {
    return <Alert color="warning">You do not have permission to view model details.</Alert>;
  }

  if (isLoading) {
    return <LoadingSpinner />;
  }

  if (error || !model) {
    return <Alert color="danger">Model not found.</Alert>;
  }

  return (
    <div>
      <Button tag={Link} to="/models" color="link" className="p-0 mb-2">
        ← Back to Models
      </Button>
      <div className="d-flex justify-content-between align-items-start mb-4 flex-wrap gap-2">
        <div>
          <h1 className="page-title mb-1">
            {model.fullName}
            {model.isDefault && <DefaultBadge />}
          </h1>
          <p className="text-muted mb-0">{model.podName}</p>
        </div>
        {canManage && model.status === 'Available' && !model.isDefault && (
          <Button color="primary" onClick={() => defaultMutation.mutate()} disabled={defaultMutation.isPending}>
            Set as Default
          </Button>
        )}
      </div>

      <Card className="mb-4">
        <CardBody>
          <CardTitle tag="h5">Metadata</CardTitle>
          <Table borderless className="mb-0">
            <tbody>
              <tr><th>Name</th><td>{model.fullName}</td></tr>
              <tr><th>Parameters</th><td>{model.parameters ?? '—'}</td></tr>
              <tr><th>Size</th><td>{formatBytes(model.size)}</td></tr>
              <tr><th>Quantization</th><td>{model.quantization ?? '—'}</td></tr>
              <tr><th>Context Window</th><td>{model.contextLength ?? '—'}</td></tr>
              <tr><th>Family</th><td>{model.family ?? '—'}</td></tr>
              <tr><th>License</th><td>{model.license ?? '—'}</td></tr>
              <tr><th>Status</th><td>{model.status}</td></tr>
              <tr><th>Last Used</th><td>{model.lastUsed ? new Date(model.lastUsed).toLocaleString() : '—'}</td></tr>
            </tbody>
          </Table>
        </CardBody>
      </Card>

      <Card className="mb-4">
        <CardBody>
          <CardTitle tag="h5">Health History</CardTitle>
          {model.healthHistory.length === 0 ? (
            <p className="text-muted mb-0">No health checks recorded yet.</p>
          ) : (
            <Table responsive hover>
              <thead>
                <tr>
                  <th>Status</th>
                  <th>Response Time</th>
                  <th>Checked</th>
                  <th>Error</th>
                </tr>
              </thead>
              <tbody>
                {model.healthHistory.map((record) => (
                  <tr key={record.id}>
                    <td><HealthBadge status={record.status} /></td>
                    <td>{record.responseTime ? `${record.responseTime}ms` : '—'}</td>
                    <td>{new Date(record.lastChecked).toLocaleString()}</td>
                    <td>{record.errorMessage ?? '—'}</td>
                  </tr>
                ))}
              </tbody>
            </Table>
          )}
        </CardBody>
      </Card>

      <Card>
        <CardBody>
          <CardTitle tag="h5">Recent Downloads</CardTitle>
          {model.downloads.length === 0 ? (
            <p className="text-muted mb-0">No download history.</p>
          ) : (
            <Table responsive hover>
              <thead>
                <tr>
                  <th>Status</th>
                  <th>Progress</th>
                  <th>Started</th>
                  <th>Completed</th>
                </tr>
              </thead>
              <tbody>
                {model.downloads.map((download) => (
                  <tr key={download.id}>
                    <td>{download.status}</td>
                    <td>{download.progress}%</td>
                    <td>{new Date(download.startedAt).toLocaleString()}</td>
                    <td>{download.completedAt ? new Date(download.completedAt).toLocaleString() : '—'}</td>
                  </tr>
                ))}
              </tbody>
            </Table>
          )}
        </CardBody>
      </Card>
    </div>
  );
};
