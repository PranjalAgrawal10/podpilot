import { useMemo, useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { Alert, Card, CardBody, CardTitle, Col, Row } from 'reactstrap';
import { ObservabilityAreaChart } from '../components/observability/ObservabilityAreaChart';
import { ObservabilityFilters } from '../components/observability/ObservabilityFilters';
import { ObservabilityLineChart } from '../components/observability/ObservabilityLineChart';
import { LoadingSpinner } from '../components/LoadingSpinner';
import { useOrganization } from '../contexts/OrganizationContext';
import { useObservabilityHub } from '../hooks/useObservabilityHub';
import { observabilityService } from '../services/observabilityService';
import { formatBytes } from '../utils/formatBytes';
import type { ObservabilityFilters as Filters } from '../types';
import { PERMISSIONS } from '../types';

const formatTime = (value: string) =>
  new Date(value).toLocaleString(undefined, { month: 'short', day: 'numeric', hour: '2-digit', minute: '2-digit' });

export const MetricsPage = () => {
  const { currentOrganization, hasPermission } = useOrganization();
  const canRead = hasPermission(PERMISSIONS.ObservabilityRead);
  const [filters, setFilters] = useState<Filters>({});

  useObservabilityHub(currentOrganization?.id);

  const { data: metrics = [], isLoading } = useQuery({
    queryKey: ['metrics', currentOrganization?.id, filters],
    queryFn: () =>
      observabilityService.getMetrics({
        from: filters.from,
        to: filters.to,
        providerId: filters.providerId,
        podId: filters.podId,
        model: filters.model,
        limit: 200,
      }),
    enabled: !!currentOrganization?.id && canRead,
    refetchInterval: 15000,
  });

  const chartData = useMemo(
    () =>
      [...metrics]
        .sort((a, b) => new Date(a.recordedAt).getTime() - new Date(b.recordedAt).getTime())
        .map((snapshot) => ({
          time: formatTime(snapshot.recordedAt),
          gpuUtilizationPercent: snapshot.gpuUtilizationPercent,
          cpuUtilizationPercent: snapshot.cpuUtilizationPercent,
          memoryUsedGb:
            snapshot.memoryUsedBytes != null ? Number((snapshot.memoryUsedBytes / (1024 ** 3)).toFixed(2)) : 0,
          memoryTotalGb:
            snapshot.memoryTotalBytes != null ? Number((snapshot.memoryTotalBytes / (1024 ** 3)).toFixed(2)) : 0,
          gpuMemoryUsedGb:
            snapshot.gpuMemoryUsedBytes != null
              ? Number((snapshot.gpuMemoryUsedBytes / (1024 ** 3)).toFixed(2))
              : 0,
          gpuMemoryTotalGb:
            snapshot.gpuMemoryTotalBytes != null
              ? Number((snapshot.gpuMemoryTotalBytes / (1024 ** 3)).toFixed(2))
              : 0,
          averageLatencyMs: snapshot.averageLatencyMs,
          queueSize: snapshot.queueSize,
        })),
    [metrics],
  );

  const latest = metrics[0];

  if (!currentOrganization) {
    return <Alert color="info">Select an organization to view metrics.</Alert>;
  }

  if (!canRead) {
    return <Alert color="warning">You do not have permission to view metrics.</Alert>;
  }

  return (
    <div>
      <h1 className="page-title">Metrics</h1>
      <p className="text-muted mb-4">GPU, CPU, and memory utilization over time.</p>

      <ObservabilityFilters filters={filters} onChange={setFilters} showDateRange />

      {isLoading && metrics.length === 0 ? (
        <LoadingSpinner />
      ) : metrics.length === 0 ? (
        <Alert color="info">No metrics snapshots found for the selected filters.</Alert>
      ) : (
        <>
          {latest && (
            <Row className="g-4 mb-4">
              <Col md={4}>
                <Card>
                  <CardBody>
                    <CardTitle tag="h6" className="text-muted">
                      Latest GPU VRAM
                    </CardTitle>
                    <p className="mb-0">
                      {latest.gpuMemoryUsedBytes != null && latest.gpuMemoryTotalBytes != null
                        ? `${formatBytes(latest.gpuMemoryUsedBytes)} / ${formatBytes(latest.gpuMemoryTotalBytes)}`
                        : '—'}
                    </p>
                  </CardBody>
                </Card>
              </Col>
              <Col md={4}>
                <Card>
                  <CardBody>
                    <CardTitle tag="h6" className="text-muted">
                      Latest RAM
                    </CardTitle>
                    <p className="mb-0">
                      {latest.memoryUsedBytes != null && latest.memoryTotalBytes != null
                        ? `${formatBytes(latest.memoryUsedBytes)} / ${formatBytes(latest.memoryTotalBytes)}`
                        : '—'}
                    </p>
                  </CardBody>
                </Card>
              </Col>
              <Col md={4}>
                <Card>
                  <CardBody>
                    <CardTitle tag="h6" className="text-muted">
                      Latest Queue
                    </CardTitle>
                    <p className="mb-0">{latest.queueSize}</p>
                  </CardBody>
                </Card>
              </Col>
            </Row>
          )}

          <Row className="g-4">
            <Col lg={6}>
              <Card>
                <CardBody>
                  <CardTitle tag="h5">GPU & CPU Utilization</CardTitle>
                  <ObservabilityLineChart
                    data={chartData}
                    xKey="time"
                    yAxisLabel="%"
                    series={[
                      { key: 'gpuUtilizationPercent', name: 'GPU', unit: '%' },
                      { key: 'cpuUtilizationPercent', name: 'CPU', unit: '%' },
                    ]}
                  />
                </CardBody>
              </Card>
            </Col>
            <Col lg={6}>
              <Card>
                <CardBody>
                  <CardTitle tag="h5">System RAM (GB)</CardTitle>
                  <ObservabilityAreaChart
                    data={chartData}
                    xKey="time"
                    yAxisLabel="GB"
                    series={[
                      { key: 'memoryUsedGb', name: 'Used', color: '#0d6efd' },
                      { key: 'memoryTotalGb', name: 'Total', color: '#6c757d' },
                    ]}
                  />
                </CardBody>
              </Card>
            </Col>
            <Col lg={6}>
              <Card>
                <CardBody>
                  <CardTitle tag="h5">GPU VRAM (GB)</CardTitle>
                  <ObservabilityAreaChart
                    data={chartData}
                    xKey="time"
                    yAxisLabel="GB"
                    series={[
                      { key: 'gpuMemoryUsedGb', name: 'Used', color: '#198754' },
                      { key: 'gpuMemoryTotalGb', name: 'Total', color: '#6c757d' },
                    ]}
                  />
                </CardBody>
              </Card>
            </Col>
            <Col lg={6}>
              <Card>
                <CardBody>
                  <CardTitle tag="h5">Latency & Queue</CardTitle>
                  <ObservabilityLineChart
                    data={chartData}
                    xKey="time"
                    series={[
                      { key: 'averageLatencyMs', name: 'Latency (ms)', color: '#ffc107' },
                      { key: 'queueSize', name: 'Queue Size', color: '#dc3545' },
                    ]}
                  />
                </CardBody>
              </Card>
            </Col>
          </Row>
        </>
      )}
    </div>
  );
};
