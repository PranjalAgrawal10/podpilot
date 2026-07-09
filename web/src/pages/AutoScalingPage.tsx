import { useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import {
  Alert,
  Badge,
  Button,
  Card,
  CardBody,
  CardTitle,
  Col,
  FormGroup,
  Input,
  Label,
  Row,
  Table,
} from 'reactstrap';
import { toast } from 'react-toastify';
import { useOrganization } from '../contexts/OrganizationContext';
import { useOrchestratorHub } from '../hooks/useOrchestratorHub';
import { orchestratorService } from '../services/orchestratorService';
import { LoadingSpinner } from '../components/LoadingSpinner';
import { PERMISSIONS } from '../types';

const directionColor = (direction: string) =>
  direction.toLowerCase().includes('up') ? 'success' : 'warning';

const formatPercent = (value: number) => `${Math.round(value * 100)}%`;

export const AutoScalingPage = () => {
  const queryClient = useQueryClient();
  const { currentOrganization, hasPermission } = useOrganization();
  const canRead = hasPermission(PERMISSIONS.OrchestratorRead);
  const canManage = hasPermission(PERMISSIONS.OrchestratorManage);

  const [scaleReason, setScaleReason] = useState('');

  useOrchestratorHub(currentOrganization?.id);

  const { data: status, isLoading, error } = useQuery({
    queryKey: ['autoscaler-status', currentOrganization?.id],
    queryFn: orchestratorService.getAutoScalerStatus,
    enabled: !!currentOrganization?.id && canRead,
    refetchInterval: 10000,
  });

  const { data: events = [] } = useQuery({
    queryKey: ['scaling-events', currentOrganization?.id],
    queryFn: () => orchestratorService.listScalingEvents(undefined, 25),
    enabled: !!currentOrganization?.id && canRead,
    refetchInterval: 10000,
  });

  const invalidate = () => {
    queryClient.invalidateQueries({ queryKey: ['autoscaler-status', currentOrganization?.id] });
    queryClient.invalidateQueries({ queryKey: ['scaling-events', currentOrganization?.id] });
    queryClient.invalidateQueries({ queryKey: ['pod-pools', currentOrganization?.id] });
  };

  const scaleUpMutation = useMutation({
    mutationFn: (poolId: string) =>
      orchestratorService.scaleUp({ poolId, reason: scaleReason || undefined }),
    onSuccess: (result) => {
      toast.success(result.success ? 'Scale-up initiated' : 'Scale-up failed');
      invalidate();
    },
    onError: (err) => toast.error(err instanceof Error ? err.message : 'Failed to scale up'),
  });

  const scaleDownMutation = useMutation({
    mutationFn: (poolId: string) =>
      orchestratorService.scaleDown({ poolId, reason: scaleReason || undefined }),
    onSuccess: (result) => {
      toast.success(result.success ? 'Scale-down initiated' : 'Scale-down failed');
      invalidate();
    },
    onError: (err) => toast.error(err instanceof Error ? err.message : 'Failed to scale down'),
  });

  if (!currentOrganization) {
    return <Alert color="info">Select an organization to view auto-scaling.</Alert>;
  }

  if (!canRead) {
    return <Alert color="warning">You do not have permission to view auto-scaling.</Alert>;
  }

  const pools = status?.pools ?? [];
  const recentEvents = events.length > 0 ? events : (status?.recentEvents ?? []);

  return (
    <div>
      <h1 className="page-title">Auto Scaling</h1>
      <p className="text-muted mb-4">
        Monitor pool utilization and trigger manual scale-up or scale-down actions.
      </p>

      {canManage && (
        <Card className="mb-4">
          <CardBody>
            <FormGroup>
              <Label for="scaleReason">Manual scale reason (optional)</Label>
              <Input
                id="scaleReason"
                value={scaleReason}
                onChange={(e) => setScaleReason(e.target.value)}
                placeholder="e.g. Expected traffic spike"
              />
            </FormGroup>
          </CardBody>
        </Card>
      )}

      {isLoading && <LoadingSpinner />}
      {error && (
        <Alert color="danger">
          {error instanceof Error ? error.message : 'Failed to load auto-scaler status'}
        </Alert>
      )}

      {!isLoading && !error && pools.length === 0 && (
        <Alert color="info">
          No pools with scaling policies yet. Create a pod pool to enable auto-scaling.
        </Alert>
      )}

      {!isLoading && pools.length > 0 && (
        <Row className="g-4 mb-4">
          {pools.map((pool) => (
            <Col key={pool.poolId} md={6} lg={4}>
              <Card className="h-100">
                <CardBody>
                  <CardTitle tag="h5">{pool.poolName}</CardTitle>
                  <p className="mb-2">
                    <strong>Pods:</strong> {pool.currentPods} / {pool.maxPods}{' '}
                    <span className="text-muted">(min {pool.minPods})</span>
                  </p>
                  <p className="mb-2">
                    <strong>Utilization:</strong> {formatPercent(pool.utilization)}
                  </p>
                  <p className="mb-3">
                    <strong>Warm standby:</strong> {pool.warmStandbyCount}
                  </p>
                  <div className="d-flex gap-1 mb-3">
                    {pool.scaleUpRecommended && <Badge color="success">Scale up</Badge>}
                    {pool.scaleDownRecommended && <Badge color="warning">Scale down</Badge>}
                    {!pool.scaleUpRecommended && !pool.scaleDownRecommended && (
                      <Badge color="secondary">Stable</Badge>
                    )}
                  </div>
                  {canManage && (
                    <div className="d-flex gap-2">
                      <Button
                        size="sm"
                        color="success"
                        disabled={scaleUpMutation.isPending || pool.currentPods >= pool.maxPods}
                        onClick={() => scaleUpMutation.mutate(pool.poolId)}
                      >
                        Scale Up
                      </Button>
                      <Button
                        size="sm"
                        color="warning"
                        disabled={scaleDownMutation.isPending || pool.currentPods <= pool.minPods}
                        onClick={() => scaleDownMutation.mutate(pool.poolId)}
                      >
                        Scale Down
                      </Button>
                    </div>
                  )}
                </CardBody>
              </Card>
            </Col>
          ))}
        </Row>
      )}

      <Card>
        <CardBody>
          <CardTitle tag="h5">Recent Scaling Events</CardTitle>
          {recentEvents.length === 0 ? (
            <p className="text-muted mb-0">No scaling events recorded yet.</p>
          ) : (
            <Table responsive hover className="mb-0">
              <thead>
                <tr>
                  <th>Time</th>
                  <th>Direction</th>
                  <th>Trigger</th>
                  <th>Reason</th>
                  <th>Pods</th>
                  <th>Status</th>
                </tr>
              </thead>
              <tbody>
                {recentEvents.map((event) => (
                  <tr key={event.id}>
                    <td className="small">{new Date(event.occurredAt).toLocaleString()}</td>
                    <td>
                      <Badge color={directionColor(event.direction)}>{event.direction}</Badge>
                    </td>
                    <td>{event.triggerType}</td>
                    <td>{event.reason}</td>
                    <td>
                      {event.podCountBefore} → {event.podCountAfter}
                    </td>
                    <td>
                      <Badge color={event.success ? 'success' : 'danger'}>
                        {event.success ? 'Success' : 'Failed'}
                      </Badge>
                    </td>
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
