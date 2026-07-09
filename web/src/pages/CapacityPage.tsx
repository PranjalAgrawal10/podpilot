import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import {
  Alert,
  Card,
  CardBody,
  CardTitle,
  Col,
  FormGroup,
  Input,
  Label,
  Progress,
  Row,
} from 'reactstrap';
import { useOrganization } from '../contexts/OrganizationContext';
import { useOrchestratorHub } from '../hooks/useOrchestratorHub';
import { orchestratorService } from '../services/orchestratorService';
import { LoadingSpinner } from '../components/LoadingSpinner';
import { PERMISSIONS } from '../types';

const capacityColor = (value: number) => {
  if (value >= 0.8) return 'danger';
  if (value >= 0.6) return 'warning';
  return 'success';
};

const formatPercent = (value: number) => `${Math.round(value * 100)}%`;

export const CapacityPage = () => {
  const { currentOrganization, hasPermission } = useOrganization();
  const canRead = hasPermission(PERMISSIONS.OrchestratorRead);
  const [selectedPoolId, setSelectedPoolId] = useState('');

  useOrchestratorHub(currentOrganization?.id);

  const { data: pools = [] } = useQuery({
    queryKey: ['pod-pools', currentOrganization?.id],
    queryFn: orchestratorService.listPodPools,
    enabled: !!currentOrganization?.id && canRead,
  });

  const { data: capacity, isLoading, error } = useQuery({
    queryKey: ['capacity', currentOrganization?.id, selectedPoolId || 'all'],
    queryFn: () => orchestratorService.getCapacity(selectedPoolId || undefined),
    enabled: !!currentOrganization?.id && canRead,
    refetchInterval: 10000,
  });

  if (!currentOrganization) {
    return <Alert color="info">Select an organization to view capacity.</Alert>;
  }

  if (!canRead) {
    return <Alert color="warning">You do not have permission to view capacity planning.</Alert>;
  }

  return (
    <div>
      <h1 className="page-title">Capacity Planning</h1>
      <p className="text-muted mb-4">
        Monitor current and projected capacity across your orchestrated pod pools.
      </p>

      <Card className="mb-4">
        <CardBody>
          <FormGroup>
            <Label for="capacityPool">Filter by pool</Label>
            <Input
              id="capacityPool"
              type="select"
              value={selectedPoolId}
              onChange={(e) => setSelectedPoolId(e.target.value)}
            >
              <option value="">All pools (organization)</option>
              {pools.map((pool) => (
                <option key={pool.id} value={pool.id}>
                  {pool.name}
                </option>
              ))}
            </Input>
          </FormGroup>
        </CardBody>
      </Card>

      {isLoading && <LoadingSpinner />}
      {error && (
        <Alert color="danger">
          {error instanceof Error ? error.message : 'Failed to load capacity data'}
        </Alert>
      )}

      {capacity && (
        <>
          <Row className="g-4 mb-4">
            <Col md={4}>
              <Card className="stat-card h-100">
                <CardBody>
                  <CardTitle tag="h6">Current Capacity</CardTitle>
                  <p className="stat-value">{formatPercent(capacity.currentCapacity)}</p>
                  <Progress
                    value={capacity.currentCapacity * 100}
                    color={capacityColor(capacity.currentCapacity)}
                  />
                </CardBody>
              </Card>
            </Col>
            <Col md={4}>
              <Card className="stat-card h-100">
                <CardBody>
                  <CardTitle tag="h6">Projected Capacity</CardTitle>
                  <p className="stat-value">{formatPercent(capacity.projectedCapacity)}</p>
                  <Progress
                    value={capacity.projectedCapacity * 100}
                    color={capacityColor(capacity.projectedCapacity)}
                  />
                </CardBody>
              </Card>
            </Col>
            <Col md={4}>
              <Card className="stat-card h-100">
                <CardBody>
                  <CardTitle tag="h6">Remaining Capacity</CardTitle>
                  <p className="stat-value">{formatPercent(capacity.remainingCapacity)}</p>
                  <Progress value={capacity.remainingCapacity * 100} color="info" />
                </CardBody>
              </Card>
            </Col>
          </Row>

          <Row className="g-4 mb-4">
            <Col md={3}>
              <Card>
                <CardBody>
                  <CardTitle tag="h6">Total Pods</CardTitle>
                  <h3>{capacity.totalPods}</h3>
                </CardBody>
              </Card>
            </Col>
            <Col md={3}>
              <Card>
                <CardBody>
                  <CardTitle tag="h6">Healthy Pods</CardTitle>
                  <h3>{capacity.healthyPods}</h3>
                </CardBody>
              </Card>
            </Col>
            <Col md={3}>
              <Card>
                <CardBody>
                  <CardTitle tag="h6">Busy Pods</CardTitle>
                  <h3>{capacity.busyPods}</h3>
                </CardBody>
              </Card>
            </Col>
            <Col md={3}>
              <Card>
                <CardBody>
                  <CardTitle tag="h6">Queue Length</CardTitle>
                  <h3>{capacity.queueLength}</h3>
                </CardBody>
              </Card>
            </Col>
          </Row>

          <Row className="g-4">
            <Col md={4}>
              <Card>
                <CardBody>
                  <CardTitle tag="h6">Max Throughput</CardTitle>
                  <h3>{capacity.maximumThroughput.toFixed(1)} req/s</h3>
                </CardBody>
              </Card>
            </Col>
            <Col md={4}>
              <Card>
                <CardBody>
                  <CardTitle tag="h6">GPU Utilization</CardTitle>
                  <h3>{capacity.gpuUtilizationPercent.toFixed(1)}%</h3>
                </CardBody>
              </Card>
            </Col>
            <Col md={4}>
              <Card>
                <CardBody>
                  <CardTitle tag="h6">Suggested Scale</CardTitle>
                  <h3>
                    {capacity.suggestedScale > 0 ? '+' : ''}
                    {capacity.suggestedScale} pods
                  </h3>
                </CardBody>
              </Card>
            </Col>
          </Row>

          <Row className="g-4 mt-2">
            <Col md={4}>
              <Card>
                <CardBody>
                  <CardTitle tag="h6">Avg Wait Time</CardTitle>
                  <h3>{Math.round(capacity.averageWaitTimeMs)} ms</h3>
                </CardBody>
              </Card>
            </Col>
            <Col md={4}>
              <Card>
                <CardBody>
                  <CardTitle tag="h6">Avg Latency</CardTitle>
                  <h3>{Math.round(capacity.averageLatencyMs)} ms</h3>
                </CardBody>
              </Card>
            </Col>
            <Col md={4}>
              <Card>
                <CardBody>
                  <CardTitle tag="h6">Concurrent Streams</CardTitle>
                  <h3>{capacity.concurrentStreams}</h3>
                </CardBody>
              </Card>
            </Col>
          </Row>
        </>
      )}
    </div>
  );
};
