import { useParams, Link } from 'react-router-dom';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { toast } from 'react-toastify';
import {
  Card, CardBody, CardHeader, Row, Col, Spinner, Alert, Table,
} from 'reactstrap';
import { podService } from '../services/podService';
import { StatusBadge } from '../components/pods/StatusBadge';
import { GpuBadge } from '../components/pods/GpuBadge';
import { CostBadge } from '../components/pods/CostBadge';
import { RegionBadge } from '../components/pods/RegionBadge';
import { ActionMenu } from '../components/pods/ActionMenu';
import { PodLifecyclePanel } from '../components/pods/PodLifecyclePanel';
import { useOrganization } from '../contexts/OrganizationContext';
import { usePodStatusHub } from '../hooks/usePodStatusHub';
import { PERMISSIONS } from '../types';

export const PodDetailsPage = () => {
  const { id } = useParams<{ id: string }>();
  const queryClient = useQueryClient();
  const { currentOrganization, hasPermission } = useOrganization();
  const canRead = hasPermission(PERMISSIONS.PodRead);
  const canUpdate = hasPermission(PERMISSIONS.PodUpdate);
  const canDelete = hasPermission(PERMISSIONS.PodDelete);

  usePodStatusHub(currentOrganization?.id);

  const { data: pod, isLoading, error } = useQuery({
    queryKey: ['pod', id],
    queryFn: () => podService.getById(id!),
    enabled: !!id && canRead,
    refetchInterval: 30000,
  });

  const invalidate = () => {
    queryClient.invalidateQueries({ queryKey: ['pod', id] });
    queryClient.invalidateQueries({ queryKey: ['pods', currentOrganization?.id] });
  };

  const startMutation = useMutation({
    mutationFn: () => podService.start(id!),
    onSuccess: () => { toast.success('Pod started'); invalidate(); },
    onError: (err) => toast.error(err instanceof Error ? err.message : 'Failed to start pod'),
  });

  const stopMutation = useMutation({
    mutationFn: () => podService.stop(id!),
    onSuccess: () => { toast.success('Pod stopped'); invalidate(); },
    onError: (err) => toast.error(err instanceof Error ? err.message : 'Failed to stop pod'),
  });

  const restartMutation = useMutation({
    mutationFn: () => podService.restart(id!),
    onSuccess: () => { toast.success('Pod restarted'); invalidate(); },
    onError: (err) => toast.error(err instanceof Error ? err.message : 'Failed to restart pod'),
  });

  const deleteMutation = useMutation({
    mutationFn: (force: boolean) => podService.delete(id!, force),
    onSuccess: () => { toast.success('Pod deleted'); window.location.href = '/pods'; },
    onError: (err) => toast.error(err instanceof Error ? err.message : 'Failed to delete pod'),
  });

  const syncMutation = useMutation({
    mutationFn: () => podService.sync(id!),
    onSuccess: () => { toast.success('Pod synced'); invalidate(); },
    onError: (err) => toast.error(err instanceof Error ? err.message : 'Failed to sync pod'),
  });

  if (!canRead) {
    return <Alert color="warning">You don&apos;t have permission to view pods.</Alert>;
  }

  if (isLoading) {
    return <div className="text-center py-5"><Spinner /></div>;
  }

  if (error || !pod) {
    return <Alert color="danger">{error instanceof Error ? error.message : 'Pod not found'}</Alert>;
  }

  return (
    <div>
      <div className="d-flex justify-content-between align-items-center mb-4">
        <div>
          <h1 className="page-title mb-1">{pod.name}</h1>
          <p className="text-muted mb-0">
            <Link to="/pods">Pods</Link> / {pod.name}
          </p>
        </div>
        <div className="d-flex gap-2 align-items-center">
          <StatusBadge status={pod.status} />
          <ActionMenu
            pod={pod}
            canUpdate={canUpdate}
            canDelete={canDelete}
            onStart={() => startMutation.mutate()}
            onStop={() => stopMutation.mutate()}
            onRestart={() => restartMutation.mutate()}
            onDelete={(_, force) => deleteMutation.mutate(force)}
            onSync={() => syncMutation.mutate()}
          />
        </div>
      </div>

      <Row className="g-4">
        <Col md={8}>
          <Card>
            <CardHeader>Overview</CardHeader>
            <CardBody>
              <Row className="mb-3">
                <Col sm={4} className="text-muted">Provider</Col>
                <Col sm={8}>{pod.providerName} ({pod.providerType})</Col>
              </Row>
              <Row className="mb-3">
                <Col sm={4} className="text-muted">GPU</Col>
                <Col sm={8}><GpuBadge gpuType={pod.gpuType} /> {pod.gpuId}</Col>
              </Row>
              <Row className="mb-3">
                <Col sm={4} className="text-muted">Region</Col>
                <Col sm={8}><RegionBadge region={pod.region} /></Col>
              </Row>
              <Row className="mb-3">
                <Col sm={4} className="text-muted">Hourly Cost</Col>
                <Col sm={8}><CostBadge hourlyCost={pod.hourlyCost} /></Col>
              </Row>
              <Row className="mb-3">
                <Col sm={4} className="text-muted">Image</Col>
                <Col sm={8}><code>{pod.imageName}</code></Col>
              </Row>
              <Row className="mb-3">
                <Col sm={4} className="text-muted">Endpoint</Col>
                <Col sm={8}>
                  {pod.endpoint ? (
                    <a href={pod.endpoint} target="_blank" rel="noreferrer">{pod.endpoint}</a>
                  ) : (
                    <span className="text-muted">Not available</span>
                  )}
                </Col>
              </Row>
              <Row>
                <Col sm={4} className="text-muted">Last Synced</Col>
                <Col sm={8}>
                  {pod.lastSyncedAt ? new Date(pod.lastSyncedAt).toLocaleString() : '—'}
                </Col>
              </Row>
            </CardBody>
          </Card>

          {pod.configuration && (
            <Card className="mt-4">
              <CardHeader>Configuration</CardHeader>
              <CardBody>
                <Row className="mb-2"><Col sm={4} className="text-muted">Container Disk</Col><Col sm={8}>{pod.configuration.containerDiskGb} GB</Col></Row>
                <Row className="mb-2"><Col sm={4} className="text-muted">Volume Disk</Col><Col sm={8}>{pod.configuration.volumeDiskGb} GB</Col></Row>
                <Row className="mb-2"><Col sm={4} className="text-muted">GPU Count</Col><Col sm={8}>{pod.configuration.gpuCount}</Col></Row>
                <Row className="mb-2"><Col sm={4} className="text-muted">Public IP</Col><Col sm={8}>{pod.configuration.enablePublicIp ? 'Enabled' : 'Disabled'}</Col></Row>
                {pod.configuration.ports.length > 0 && (
                  <Row><Col sm={4} className="text-muted">Ports</Col><Col sm={8}>{pod.configuration.ports.join(', ')}</Col></Row>
                )}
              </CardBody>
            </Card>
          )}
        </Col>

        <Col md={4}>
          <PodLifecyclePanel podId={pod.id} canUpdate={canUpdate} onUpdated={invalidate} />
          <Card className="mt-4">
            <CardHeader>Status History</CardHeader>
            <CardBody className="p-0">
              {pod.statusHistory.length === 0 ? (
                <p className="p-3 text-muted mb-0">No history yet.</p>
              ) : (
                <Table size="sm" responsive className="mb-0">
                  <tbody>
                    {pod.statusHistory.map((entry, index) => (
                      <tr key={`${entry.recordedAt}-${index}`}>
                        <td><StatusBadge status={entry.status} /></td>
                        <td className="small text-muted">{new Date(entry.recordedAt).toLocaleString()}</td>
                      </tr>
                    ))}
                  </tbody>
                </Table>
              )}
            </CardBody>
          </Card>
        </Col>
      </Row>
    </div>
  );
};
