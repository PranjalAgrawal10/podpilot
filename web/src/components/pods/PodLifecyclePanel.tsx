import { useState } from 'react';
import { useMutation, useQuery } from '@tanstack/react-query';
import { toast } from 'react-toastify';
import {
  Card, CardBody, CardHeader, Row, Col, Button, Form, FormGroup, Label, Input, Badge, Table, Spinner,
} from 'reactstrap';
import { podService } from '../../services/podService';
import type { UpdatePodIdlePolicyRequest } from '../../types';

interface PodLifecyclePanelProps {
  podId: string;
  canUpdate: boolean;
  onUpdated: () => void;
}

const formatMinutes = (minutes: number) => {
  if (minutes < 60) return `${Math.round(minutes)} min`;
  const hours = Math.floor(minutes / 60);
  const mins = Math.round(minutes % 60);
  return `${hours}h ${mins}m`;
};

export const PodLifecyclePanel = ({ podId, canUpdate, onUpdated }: PodLifecyclePanelProps) => {
  const { data: lifecycle, isLoading, refetch } = useQuery({
    queryKey: ['pod-lifecycle', podId],
    queryFn: () => podService.getLifecycle(podId),
    refetchInterval: 30000,
  });

  const { data: activities = [] } = useQuery({
    queryKey: ['pod-activity', podId],
    queryFn: () => podService.getActivity(podId),
    refetchInterval: 30000,
  });

  const [policyForm, setPolicyForm] = useState<UpdatePodIdlePolicyRequest | null>(null);

  const wakeMutation = useMutation({
    mutationFn: () => podService.wake(podId),
    onSuccess: (result) => {
      if (result.success) {
        toast.success(result.queued ? 'Wake queued' : 'Pod waking');
      } else {
        toast.error(result.errorMessage || 'Wake failed');
      }
      onUpdated();
      refetch();
    },
    onError: (err) => toast.error(err instanceof Error ? err.message : 'Wake failed'),
  });

  const shutdownMutation = useMutation({
    mutationFn: () => podService.shutdown(podId),
    onSuccess: (result) => {
      if (result.success) {
        toast.success('Pod shutdown initiated');
      } else {
        toast.error(result.errorMessage || 'Shutdown failed');
      }
      onUpdated();
      refetch();
    },
    onError: (err) => toast.error(err instanceof Error ? err.message : 'Shutdown failed'),
  });

  const policyMutation = useMutation({
    mutationFn: (data: UpdatePodIdlePolicyRequest) => podService.updateIdlePolicy(podId, data),
    onSuccess: () => {
      toast.success('Idle policy updated');
      setPolicyForm(null);
      onUpdated();
      refetch();
    },
    onError: (err) => toast.error(err instanceof Error ? err.message : 'Failed to update policy'),
  });

  if (isLoading || !lifecycle) {
    return <div className="text-center py-3"><Spinner size="sm" /></div>;
  }

  const form = policyForm ?? {
    idleTimeoutMinutes: lifecycle.policy.idleTimeoutMinutes,
    gracePeriodMinutes: lifecycle.policy.gracePeriodMinutes,
    autoShutdownEnabled: lifecycle.policy.autoShutdownEnabled,
    autoWakeEnabled: lifecycle.policy.autoWakeEnabled,
    minimumRunningTimeMinutes: lifecycle.policy.minimumRunningTimeMinutes,
  };

  return (
    <div>
      <Card className="mt-4">
        <CardHeader className="d-flex justify-content-between align-items-center">
          <span>Lifecycle Engine</span>
          {canUpdate && (
            <div className="d-flex gap-2">
              <Button size="sm" color="success" outline onClick={() => wakeMutation.mutate()} disabled={wakeMutation.isPending}>
                Wake
              </Button>
              <Button size="sm" color="warning" outline onClick={() => shutdownMutation.mutate()} disabled={shutdownMutation.isPending}>
                Shutdown
              </Button>
            </div>
          )}
        </CardHeader>
        <CardBody>
          <Row className="g-3 mb-3">
            <Col md={4}>
              <div className="text-muted small">Running Time</div>
              <div className="fw-semibold">{formatMinutes(lifecycle.runningTimeMinutes)}</div>
            </Col>
            <Col md={4}>
              <div className="text-muted small">Idle Time</div>
              <div className="fw-semibold">
                {formatMinutes(lifecycle.idleTimeMinutes)}
                {lifecycle.isIdle && <Badge color="warning" className="ms-2">Idle</Badge>}
              </div>
            </Col>
            <Col md={4}>
              <div className="text-muted small">Last Activity</div>
              <div className="fw-semibold">
                {lifecycle.lastActivityAt ? new Date(lifecycle.lastActivityAt).toLocaleString() : '—'}
              </div>
            </Col>
            <Col md={4}>
              <div className="text-muted small">Next Shutdown</div>
              <div className="fw-semibold">
                {lifecycle.nextShutdownAt ? new Date(lifecycle.nextShutdownAt).toLocaleString() : '—'}
              </div>
            </Col>
            <Col md={4}>
              <div className="text-muted small">Auto Wake</div>
              <Badge color={lifecycle.autoWakeEnabled ? 'success' : 'secondary'}>
                {lifecycle.autoWakeEnabled ? 'Enabled' : 'Disabled'}
              </Badge>
            </Col>
            <Col md={4}>
              <div className="text-muted small">Auto Shutdown</div>
              <Badge color={lifecycle.autoShutdownEnabled ? 'success' : 'secondary'}>
                {lifecycle.autoShutdownEnabled ? 'Enabled' : 'Disabled'}
              </Badge>
            </Col>
          </Row>
        </CardBody>
      </Card>

      {canUpdate && (
        <Card className="mt-4">
          <CardHeader>Idle Policy Settings</CardHeader>
          <CardBody>
            <Form
              onSubmit={(e) => {
                e.preventDefault();
                policyMutation.mutate(form);
              }}
            >
              <Row>
                <Col md={4}>
                  <FormGroup>
                    <Label>Idle Timeout (minutes)</Label>
                    <Input
                      type="number"
                      min={1}
                      value={form.idleTimeoutMinutes}
                      onChange={(e) => setPolicyForm({ ...form, idleTimeoutMinutes: Number(e.target.value) })}
                    />
                  </FormGroup>
                </Col>
                <Col md={4}>
                  <FormGroup>
                    <Label>Grace Period (minutes)</Label>
                    <Input
                      type="number"
                      min={0}
                      value={form.gracePeriodMinutes}
                      onChange={(e) => setPolicyForm({ ...form, gracePeriodMinutes: Number(e.target.value) })}
                    />
                  </FormGroup>
                </Col>
                <Col md={4}>
                  <FormGroup>
                    <Label>Minimum Runtime (minutes)</Label>
                    <Input
                      type="number"
                      min={0}
                      value={form.minimumRunningTimeMinutes}
                      onChange={(e) => setPolicyForm({ ...form, minimumRunningTimeMinutes: Number(e.target.value) })}
                    />
                  </FormGroup>
                </Col>
              </Row>
              <FormGroup check className="mb-2">
                <Input
                  type="checkbox"
                  checked={form.autoShutdownEnabled}
                  onChange={(e) => setPolicyForm({ ...form, autoShutdownEnabled: e.target.checked })}
                />
                <Label check>Enable Auto Shutdown</Label>
              </FormGroup>
              <FormGroup check className="mb-3">
                <Input
                  type="checkbox"
                  checked={form.autoWakeEnabled}
                  onChange={(e) => setPolicyForm({ ...form, autoWakeEnabled: e.target.checked })}
                />
                <Label check>Enable Auto Wake</Label>
              </FormGroup>
              <Button color="primary" type="submit" disabled={policyMutation.isPending}>
                Save Policy
              </Button>
            </Form>
          </CardBody>
        </Card>
      )}

      <Card className="mt-4">
        <CardHeader>Recent Activity</CardHeader>
        <CardBody className="p-0">
          {activities.length === 0 ? (
            <p className="p-3 text-muted mb-0">No activity recorded yet.</p>
          ) : (
            <Table size="sm" responsive className="mb-0">
              <tbody>
                {activities.slice(0, 10).map((activity) => (
                  <tr key={activity.id}>
                    <td>{activity.activityType}</td>
                    <td className="text-muted small">{activity.source}</td>
                    <td className="small text-muted">{new Date(activity.timestamp).toLocaleString()}</td>
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
