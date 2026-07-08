import { Link } from 'react-router-dom';
import { useMemo } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { toast } from 'react-toastify';
import { Button, Row, Col, Spinner, Alert } from 'reactstrap';
import { podService } from '../services/podService';
import { PodCard } from '../components/pods/PodCard';
import { useOrganization } from '../contexts/OrganizationContext';
import { usePodStatusHub } from '../hooks/usePodStatusHub';
import { PERMISSIONS, type Pod } from '../types';

export const PodsPage = () => {
  const queryClient = useQueryClient();
  const { currentOrganization, hasPermission } = useOrganization();
  const canRead = hasPermission(PERMISSIONS.PodRead);
  const canCreate = hasPermission(PERMISSIONS.PodCreate);
  const canUpdate = hasPermission(PERMISSIONS.PodUpdate);
  const canDelete = hasPermission(PERMISSIONS.PodDelete);

  usePodStatusHub(currentOrganization?.id);

  const { data: pods = [], isLoading, error } = useQuery({
    queryKey: ['pods', currentOrganization?.id],
    queryFn: podService.list,
    enabled: !!currentOrganization?.id && canRead,
    refetchInterval: 30000,
  });

  const invalidate = () =>
    queryClient.invalidateQueries({ queryKey: ['pods', currentOrganization?.id] });

  const startMutation = useMutation({
    mutationFn: (pod: Pod) => podService.start(pod.id),
    onSuccess: () => { toast.success('Pod started'); invalidate(); },
    onError: (err) => toast.error(err instanceof Error ? err.message : 'Failed to start pod'),
  });

  const stopMutation = useMutation({
    mutationFn: (pod: Pod) => podService.stop(pod.id),
    onSuccess: () => { toast.success('Pod stopped'); invalidate(); },
    onError: (err) => toast.error(err instanceof Error ? err.message : 'Failed to stop pod'),
  });

  const restartMutation = useMutation({
    mutationFn: (pod: Pod) => podService.restart(pod.id),
    onSuccess: () => { toast.success('Pod restarted'); invalidate(); },
    onError: (err) => toast.error(err instanceof Error ? err.message : 'Failed to restart pod'),
  });

  const deleteMutation = useMutation({
    mutationFn: ({ pod, force }: { pod: Pod; force: boolean }) => podService.delete(pod.id, force),
    onSuccess: () => { toast.success('Pod deleted'); invalidate(); },
    onError: (err) => toast.error(err instanceof Error ? err.message : 'Failed to delete pod'),
  });

  const syncMutation = useMutation({
    mutationFn: (pod: Pod) => podService.sync(pod.id),
    onSuccess: () => { toast.success('Pod synced'); invalidate(); },
    onError: (err) => toast.error(err instanceof Error ? err.message : 'Failed to sync pod'),
  });

  if (!currentOrganization) {
    return <Alert color="info">Select an organization to view pods.</Alert>;
  }

  if (!canRead) {
    return <Alert color="warning">You don&apos;t have permission to view pods.</Alert>;
  }

  const visiblePods = useMemo(
    () => pods.filter((pod) => pod.status !== 'Deleted' && pod.status !== 'Deleting'),
    [pods],
  );

  const runningPods = visiblePods.filter((p) => p.status === 'Running').length;
  const stoppedPods = visiblePods.filter((p) => p.status === 'Stopped').length;

  return (
    <div>
      <div className="d-flex justify-content-between align-items-center mb-4">
        <div>
          <h1 className="page-title mb-1">GPU Pods</h1>
          <p className="text-muted mb-0">
            {runningPods} running · {stoppedPods} stopped · {visiblePods.length} total
          </p>
        </div>
        {canCreate && (
          <Button tag={Link} to="/pods/create" color="primary">
            Create Pod
          </Button>
        )}
      </div>

      {isLoading && <div className="text-center py-5"><Spinner /></div>}
      {error && <Alert color="danger">{error instanceof Error ? error.message : 'Failed to load pods'}</Alert>}

      {!isLoading && !error && visiblePods.length === 0 && (
        <Alert color="info">
          No pods yet.{' '}
          {canCreate && <Link to="/pods/create">Create your first GPU pod</Link>}
        </Alert>
      )}

      {!isLoading && visiblePods.length > 0 && (
        <Row>
          {visiblePods.map((pod) => (
            <Col key={pod.id} md={6} lg={4} className="mb-4">
              <PodCard
                pod={pod}
                canUpdate={canUpdate}
                canDelete={canDelete}
                onStart={(p) => startMutation.mutate(p)}
                onStop={(p) => stopMutation.mutate(p)}
                onRestart={(p) => restartMutation.mutate(p)}
                onDelete={(p, force) => deleteMutation.mutate({ pod: p, force })}
                onRefresh={(p) => syncMutation.mutate(p)}
                isRefreshing={syncMutation.isPending && syncMutation.variables?.id === pod.id}
              />
            </Col>
          ))}
        </Row>
      )}
    </div>
  );
};
