import { useState } from 'react';
import { Link } from 'react-router-dom';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import {
  Alert,
  Badge,
  Button,
  Card,
  CardBody,
  CardTitle,
  Col,
  Form,
  FormGroup,
  Input,
  Label,
  Modal,
  ModalBody,
  ModalFooter,
  ModalHeader,
  Row,
  Table,
} from 'reactstrap';
import { toast } from 'react-toastify';
import { useOrganization } from '../contexts/OrganizationContext';
import { useOrchestratorHub } from '../hooks/useOrchestratorHub';
import { orchestratorService } from '../services/orchestratorService';
import { podService } from '../services/podService';
import { LoadingSpinner } from '../components/LoadingSpinner';
import {
  PERMISSIONS,
  POD_POOL_TYPES,
  type CreatePodPoolRequest,
  type PodPool,
  type ScalingPolicy,
  type UpdatePodPoolRequest,
} from '../types';

const defaultScalingPolicy = (): ScalingPolicy => ({
  name: 'Default',
  minPods: 1,
  maxPods: 10,
  maxQueueLength: 50,
  maxLatencyMs: 5000,
  scaleUpThreshold: 0.8,
  scaleDownThreshold: 0.3,
  warmStandbyCount: 1,
  minRuntimeMinutes: 10,
  autoScaleUpEnabled: true,
  autoScaleDownEnabled: true,
});

interface PoolFormState {
  name: string;
  description: string;
  poolType: string;
  isDefault: boolean;
  isActive: boolean;
  models: string;
  podIds: string[];
  scalingPolicy: ScalingPolicy;
}

const emptyForm = (): PoolFormState => ({
  name: '',
  description: '',
  poolType: 'Custom',
  isDefault: false,
  isActive: true,
  models: '',
  podIds: [],
  scalingPolicy: defaultScalingPolicy(),
});

const toFormState = (pool: PodPool): PoolFormState => ({
  name: pool.name,
  description: pool.description ?? '',
  poolType: pool.poolType,
  isDefault: pool.isDefault,
  isActive: pool.isActive,
  models: pool.models.join(', '),
  podIds: pool.members.map((m) => m.gpuPodId),
  scalingPolicy: pool.scalingPolicy ?? defaultScalingPolicy(),
});

const toCreateRequest = (form: PoolFormState): CreatePodPoolRequest => ({
  name: form.name,
  description: form.description || null,
  poolType: form.poolType,
  isDefault: form.isDefault,
  models: form.models
    .split(',')
    .map((m) => m.trim())
    .filter(Boolean),
  podIds: form.podIds,
  scalingPolicy: form.scalingPolicy,
});

const toUpdateRequest = (form: PoolFormState): UpdatePodPoolRequest => ({
  name: form.name,
  description: form.description || null,
  poolType: form.poolType,
  isDefault: form.isDefault,
  isActive: form.isActive,
  models: form.models
    .split(',')
    .map((m) => m.trim())
    .filter(Boolean),
  podIds: form.podIds,
  scalingPolicy: form.scalingPolicy,
});

export const PodPoolsPage = () => {
  const queryClient = useQueryClient();
  const { currentOrganization, hasPermission } = useOrganization();
  const canRead = hasPermission(PERMISSIONS.OrchestratorRead);
  const canManage = hasPermission(PERMISSIONS.OrchestratorManage);

  const [modalOpen, setModalOpen] = useState(false);
  const [editingPool, setEditingPool] = useState<PodPool | null>(null);
  const [form, setForm] = useState<PoolFormState>(emptyForm());
  const [deleteTarget, setDeleteTarget] = useState<PodPool | null>(null);

  useOrchestratorHub(currentOrganization?.id);

  const { data: pools = [], isLoading, error } = useQuery({
    queryKey: ['pod-pools', currentOrganization?.id],
    queryFn: orchestratorService.listPodPools,
    enabled: !!currentOrganization?.id && canRead,
    refetchInterval: 15000,
  });

  const { data: pods = [] } = useQuery({
    queryKey: ['pods', currentOrganization?.id],
    queryFn: podService.list,
    enabled: !!currentOrganization?.id && canRead && modalOpen,
  });

  const invalidate = () =>
    queryClient.invalidateQueries({ queryKey: ['pod-pools', currentOrganization?.id] });

  const createMutation = useMutation({
    mutationFn: (data: CreatePodPoolRequest) => orchestratorService.createPodPool(data),
    onSuccess: () => {
      toast.success('Pod pool created');
      setModalOpen(false);
      invalidate();
    },
    onError: (err) => toast.error(err instanceof Error ? err.message : 'Failed to create pod pool'),
  });

  const updateMutation = useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdatePodPoolRequest }) =>
      orchestratorService.updatePodPool(id, data),
    onSuccess: () => {
      toast.success('Pod pool updated');
      setModalOpen(false);
      invalidate();
    },
    onError: (err) => toast.error(err instanceof Error ? err.message : 'Failed to update pod pool'),
  });

  const deleteMutation = useMutation({
    mutationFn: (poolId: string) => orchestratorService.deletePodPool(poolId),
    onSuccess: () => {
      toast.success('Pod pool deleted');
      setDeleteTarget(null);
      invalidate();
    },
    onError: (err) => toast.error(err instanceof Error ? err.message : 'Failed to delete pod pool'),
  });

  const openCreate = () => {
    setEditingPool(null);
    setForm(emptyForm());
    setModalOpen(true);
  };

  const openEdit = (pool: PodPool) => {
    setEditingPool(pool);
    setForm(toFormState(pool));
    setModalOpen(true);
  };

  const handleSubmit = () => {
    if (!form.name.trim()) {
      toast.error('Pool name is required');
      return;
    }

    if (editingPool) {
      updateMutation.mutate({ id: editingPool.id, data: toUpdateRequest(form) });
    } else {
      createMutation.mutate(toCreateRequest(form));
    }
  };

  const togglePodId = (podId: string) => {
    setForm((prev) => ({
      ...prev,
      podIds: prev.podIds.includes(podId)
        ? prev.podIds.filter((id) => id !== podId)
        : [...prev.podIds, podId],
    }));
  };

  if (!currentOrganization) {
    return <Alert color="info">Select an organization to view pod pools.</Alert>;
  }

  if (!canRead) {
    return <Alert color="warning">You do not have permission to view pod pools.</Alert>;
  }

  return (
    <div>
      <div className="d-flex justify-content-between align-items-center mb-4">
        <div>
          <h1 className="page-title mb-1">Pod Pools</h1>
          <p className="text-muted mb-0">
            Group GPU pods into pools for multi-pod orchestration and auto-scaling.
          </p>
        </div>
        {canManage && (
          <Button color="primary" onClick={openCreate}>
            Create Pool
          </Button>
        )}
      </div>

      {isLoading && <LoadingSpinner />}
      {error && (
        <Alert color="danger">
          {error instanceof Error ? error.message : 'Failed to load pod pools'}
        </Alert>
      )}

      {!isLoading && !error && pools.length === 0 && (
        <Alert color="info">
          No pod pools yet.{' '}
          {canManage && (
            <Button color="link" className="p-0 align-baseline" onClick={openCreate}>
              Create your first pool
            </Button>
          )}
        </Alert>
      )}

      {!isLoading && pools.length > 0 && (
        <Row>
          {pools.map((pool) => (
            <Col key={pool.id} md={6} lg={4} className="mb-4">
              <Card className="h-100">
                <CardBody>
                  <div className="d-flex justify-content-between align-items-start mb-2">
                    <CardTitle tag="h5" className="mb-0">
                      {pool.name}
                    </CardTitle>
                    <div className="d-flex gap-1">
                      {pool.isDefault && <Badge color="primary">Default</Badge>}
                      <Badge color={pool.isActive ? 'success' : 'secondary'}>
                        {pool.isActive ? 'Active' : 'Inactive'}
                      </Badge>
                    </div>
                  </div>
                  {pool.description && <p className="text-muted small">{pool.description}</p>}
                  <p className="small mb-2">
                    <strong>Type:</strong> {pool.poolType} · <strong>Members:</strong>{' '}
                    {pool.members.length}
                  </p>
                  {pool.models.length > 0 && (
                    <p className="small mb-2">
                      <strong>Models:</strong> {pool.models.join(', ')}
                    </p>
                  )}
                  {pool.scalingPolicy && (
                    <p className="small text-muted mb-3">
                      Scale {pool.scalingPolicy.minPods}–{pool.scalingPolicy.maxPods} pods
                    </p>
                  )}
                  <div className="d-flex gap-2">
                    {canManage && (
                      <>
                        <Button size="sm" color="outline-primary" onClick={() => openEdit(pool)}>
                          Edit
                        </Button>
                        <Button
                          size="sm"
                          color="outline-danger"
                          onClick={() => setDeleteTarget(pool)}
                        >
                          Delete
                        </Button>
                      </>
                    )}
                    <Button
                      tag={Link}
                      to="/orchestration/scaling"
                      size="sm"
                      color="outline-secondary"
                    >
                      Scaling
                    </Button>
                  </div>
                </CardBody>
              </Card>
            </Col>
          ))}
        </Row>
      )}

      {!isLoading && pools.length > 0 && (
        <Card className="mt-2">
          <CardBody>
            <CardTitle tag="h5">Pool Members</CardTitle>
            <Table responsive hover className="mb-0">
              <thead>
                <tr>
                  <th>Pool</th>
                  <th>Pod</th>
                  <th>Status</th>
                  <th>State</th>
                  <th>Streams</th>
                  <th>Warm Standby</th>
                </tr>
              </thead>
              <tbody>
                {pools.flatMap((pool) =>
                  pool.members.map((member) => (
                    <tr key={member.id}>
                      <td>{pool.name}</td>
                      <td>
                        <Link to={`/pods/${member.gpuPodId}`}>{member.podName}</Link>
                      </td>
                      <td>{member.podStatus}</td>
                      <td>{member.state}</td>
                      <td>{member.activeStreams}</td>
                      <td>{member.isWarmStandby ? 'Yes' : 'No'}</td>
                    </tr>
                  )),
                )}
              </tbody>
            </Table>
          </CardBody>
        </Card>
      )}

      <Modal isOpen={modalOpen} toggle={() => setModalOpen(false)} size="lg">
        <ModalHeader toggle={() => setModalOpen(false)}>
          {editingPool ? 'Edit Pod Pool' : 'Create Pod Pool'}
        </ModalHeader>
        <ModalBody>
          <Form>
            <Row>
              <Col md={6}>
                <FormGroup>
                  <Label for="poolName">Name</Label>
                  <Input
                    id="poolName"
                    value={form.name}
                    onChange={(e) => setForm((prev) => ({ ...prev, name: e.target.value }))}
                  />
                </FormGroup>
              </Col>
              <Col md={6}>
                <FormGroup>
                  <Label for="poolType">Pool Type</Label>
                  <Input
                    id="poolType"
                    type="select"
                    value={form.poolType}
                    onChange={(e) => setForm((prev) => ({ ...prev, poolType: e.target.value }))}
                  >
                    {POD_POOL_TYPES.map((type) => (
                      <option key={type} value={type}>
                        {type}
                      </option>
                    ))}
                  </Input>
                </FormGroup>
              </Col>
            </Row>
            <FormGroup>
              <Label for="poolDescription">Description</Label>
              <Input
                id="poolDescription"
                type="textarea"
                value={form.description}
                onChange={(e) => setForm((prev) => ({ ...prev, description: e.target.value }))}
              />
            </FormGroup>
            <FormGroup>
              <Label for="poolModels">Models (comma-separated)</Label>
              <Input
                id="poolModels"
                value={form.models}
                onChange={(e) => setForm((prev) => ({ ...prev, models: e.target.value }))}
                placeholder="llama3, mistral"
              />
            </FormGroup>
            <Row>
              <Col md={6}>
                <FormGroup check>
                  <Input
                    id="poolDefault"
                    type="checkbox"
                    checked={form.isDefault}
                    onChange={(e) => setForm((prev) => ({ ...prev, isDefault: e.target.checked }))}
                  />
                  <Label for="poolDefault" check>
                    Default pool
                  </Label>
                </FormGroup>
              </Col>
              {editingPool && (
                <Col md={6}>
                  <FormGroup check>
                    <Input
                      id="poolActive"
                      type="checkbox"
                      checked={form.isActive}
                      onChange={(e) => setForm((prev) => ({ ...prev, isActive: e.target.checked }))}
                    />
                    <Label for="poolActive" check>
                      Active
                    </Label>
                  </FormGroup>
                </Col>
              )}
            </Row>
            <hr />
            <h6>Scaling Policy</h6>
            <Row>
              <Col md={3}>
                <FormGroup>
                  <Label>Min Pods</Label>
                  <Input
                    type="number"
                    min={0}
                    value={form.scalingPolicy.minPods}
                    onChange={(e) =>
                      setForm((prev) => ({
                        ...prev,
                        scalingPolicy: { ...prev.scalingPolicy, minPods: Number(e.target.value) },
                      }))
                    }
                  />
                </FormGroup>
              </Col>
              <Col md={3}>
                <FormGroup>
                  <Label>Max Pods</Label>
                  <Input
                    type="number"
                    min={1}
                    value={form.scalingPolicy.maxPods}
                    onChange={(e) =>
                      setForm((prev) => ({
                        ...prev,
                        scalingPolicy: { ...prev.scalingPolicy, maxPods: Number(e.target.value) },
                      }))
                    }
                  />
                </FormGroup>
              </Col>
              <Col md={3}>
                <FormGroup>
                  <Label>Warm Standby</Label>
                  <Input
                    type="number"
                    min={0}
                    value={form.scalingPolicy.warmStandbyCount}
                    onChange={(e) =>
                      setForm((prev) => ({
                        ...prev,
                        scalingPolicy: {
                          ...prev.scalingPolicy,
                          warmStandbyCount: Number(e.target.value),
                        },
                      }))
                    }
                  />
                </FormGroup>
              </Col>
              <Col md={3}>
                <FormGroup>
                  <Label>Max Queue</Label>
                  <Input
                    type="number"
                    min={0}
                    value={form.scalingPolicy.maxQueueLength}
                    onChange={(e) =>
                      setForm((prev) => ({
                        ...prev,
                        scalingPolicy: {
                          ...prev.scalingPolicy,
                          maxQueueLength: Number(e.target.value),
                        },
                      }))
                    }
                  />
                </FormGroup>
              </Col>
            </Row>
            <FormGroup>
              <Label>Pod Members</Label>
              <div className="border rounded p-2" style={{ maxHeight: 160, overflowY: 'auto' }}>
                {pods.length === 0 ? (
                  <span className="text-muted small">No pods available</span>
                ) : (
                  pods.map((pod) => (
                    <FormGroup check key={pod.id}>
                      <Input
                        id={`pod-${pod.id}`}
                        type="checkbox"
                        checked={form.podIds.includes(pod.id)}
                        onChange={() => togglePodId(pod.id)}
                      />
                      <Label for={`pod-${pod.id}`} check>
                        {pod.name} ({pod.status})
                      </Label>
                    </FormGroup>
                  ))
                )}
              </div>
            </FormGroup>
          </Form>
        </ModalBody>
        <ModalFooter>
          <Button color="secondary" onClick={() => setModalOpen(false)}>
            Cancel
          </Button>
          <Button
            color="primary"
            onClick={handleSubmit}
            disabled={createMutation.isPending || updateMutation.isPending}
          >
            {editingPool ? 'Save Changes' : 'Create Pool'}
          </Button>
        </ModalFooter>
      </Modal>

      <Modal isOpen={!!deleteTarget} toggle={() => setDeleteTarget(null)}>
        <ModalHeader toggle={() => setDeleteTarget(null)}>Delete Pod Pool</ModalHeader>
        <ModalBody>
          Are you sure you want to delete <strong>{deleteTarget?.name}</strong>? This cannot be
          undone.
        </ModalBody>
        <ModalFooter>
          <Button color="secondary" onClick={() => setDeleteTarget(null)}>
            Cancel
          </Button>
          <Button
            color="danger"
            onClick={() => deleteTarget && deleteMutation.mutate(deleteTarget.id)}
            disabled={deleteMutation.isPending}
          >
            Delete
          </Button>
        </ModalFooter>
      </Modal>
    </div>
  );
};
