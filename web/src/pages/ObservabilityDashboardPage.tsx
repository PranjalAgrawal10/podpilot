import { Link } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { Alert, Col, Row } from 'reactstrap';
import { AlertPanel } from '../components/observability/AlertPanel';
import { GpuWidget } from '../components/observability/GpuWidget';
import { HealthStatusBadge } from '../components/observability/HealthStatusBadge';
import { MetricCard } from '../components/observability/MetricCard';
import { QueueWidget } from '../components/observability/QueueWidget';
import { LoadingSpinner } from '../components/LoadingSpinner';
import { useOrganization } from '../contexts/OrganizationContext';
import { useObservabilityHub } from '../hooks/useObservabilityHub';
import { observabilityService } from '../services/observabilityService';
import { PERMISSIONS } from '../types';

const formatCurrency = (value: number) =>
  new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(value);

const formatBytes = (value?: number | null) => {
  if (value == null || value <= 0) {
    return '—';
  }

  const gb = value / (1024 * 1024 * 1024);
  return `${gb.toFixed(1)} GB`;
};

export const ObservabilityDashboardPage = () => {
  const { currentOrganization, hasPermission } = useOrganization();
  const canRead = hasPermission(PERMISSIONS.ObservabilityRead);

  useObservabilityHub(currentOrganization?.id);

  const { data: liveMetrics, isLoading: liveLoading } = useQuery({
    queryKey: ['live-metrics', currentOrganization?.id],
    queryFn: observabilityService.getLiveMetrics,
    enabled: !!currentOrganization?.id && canRead,
    refetchInterval: 10000,
  });

  const { data: cost } = useQuery({
    queryKey: ['cost', currentOrganization?.id, 'Daily'],
    queryFn: () => observabilityService.getCost({ period: 'Daily' }),
    enabled: !!currentOrganization?.id && canRead,
    refetchInterval: 15000,
  });

  const { data: systemHealth } = useQuery({
    queryKey: ['system-health', currentOrganization?.id],
    queryFn: observabilityService.getSystemHealth,
    enabled: !!currentOrganization?.id && canRead,
    refetchInterval: 15000,
  });

  const { data: podHealth } = useQuery({
    queryKey: ['pod-health-overview', currentOrganization?.id],
    queryFn: observabilityService.getPodHealth,
    enabled: !!currentOrganization?.id && canRead,
    refetchInterval: 15000,
  });

  const { data: alerts = [] } = useQuery({
    queryKey: ['alerts', currentOrganization?.id, true],
    queryFn: () => observabilityService.listAlerts(true, 10),
    enabled: !!currentOrganization?.id && canRead,
    refetchInterval: 15000,
  });

  if (!currentOrganization) {
    return <Alert color="info">Select an organization to view observability.</Alert>;
  }

  if (!canRead) {
    return <Alert color="warning">You do not have permission to view observability.</Alert>;
  }

  if (liveLoading && !liveMetrics) {
    return <LoadingSpinner />;
  }

  const activeAlertCount = alerts.filter((alert) => alert.isActive).length;
  const vramLabel =
    liveMetrics?.gpuMemoryUsedBytes != null && liveMetrics?.gpuMemoryTotalBytes != null
      ? `${formatBytes(liveMetrics.gpuMemoryUsedBytes)} / ${formatBytes(liveMetrics.gpuMemoryTotalBytes)}`
      : formatBytes(liveMetrics?.gpuMemoryUsedBytes);

  return (
    <div>
      <h1 className="page-title">Observability</h1>
      <p className="text-muted mb-4">
        Real-time overview of GPU infrastructure, costs, health, and alerts.
      </p>

      <Row className="g-4 mb-4">
        <Col md={3} sm={6}>
          <MetricCard title="Running Pods" value={liveMetrics?.runningPods ?? 0} />
        </Col>
        <Col md={3} sm={6}>
          <MetricCard title="Stopped Pods" value={liveMetrics?.stoppedPods ?? 0} />
        </Col>
        <Col md={3} sm={6}>
          <MetricCard title="Healthy Pods" value={liveMetrics?.healthyPods ?? 0} />
        </Col>
        <Col md={3} sm={6}>
          <MetricCard title="Active Requests" value={liveMetrics?.activeStreams ?? 0} />
        </Col>
      </Row>

      <Row className="g-4 mb-4">
        <Col md={3} sm={6}>
          <MetricCard title="Queue Size" value={liveMetrics?.queueSize ?? 0} />
        </Col>
        <Col md={3} sm={6}>
          <MetricCard
            title="Average Latency"
            value={`${(liveMetrics?.averageLatencyMs ?? 0).toFixed(0)} ms`}
          />
        </Col>
        <Col md={3} sm={6}>
          <MetricCard
            title="GPU Utilization"
            value={`${(liveMetrics?.gpuUtilizationPercent ?? 0).toFixed(1)}%`}
          />
        </Col>
        <Col md={3} sm={6}>
          <MetricCard title="VRAM Usage" value={vramLabel} />
        </Col>
      </Row>

      <Row className="g-4 mb-4">
        <Col md={3} sm={6}>
          <MetricCard title="Today's Cost" value={formatCurrency(cost?.dailyCost ?? 0)} />
        </Col>
        <Col md={3} sm={6}>
          <MetricCard
            title="Monthly Projection"
            value={formatCurrency(cost?.projectedMonthlyCost ?? 0)}
          />
        </Col>
        <Col md={3} sm={6}>
          <MetricCard
            title="Auto Shutdown Savings"
            value={formatCurrency(cost?.autoShutdownSavings ?? 0)}
          />
        </Col>
        <Col md={3} sm={6}>
          <MetricCard title="Models Installed" value={liveMetrics?.modelsInstalled ?? 0} />
        </Col>
      </Row>

      <Row className="g-4 mb-4">
        <Col md={3} sm={6}>
          <MetricCard
            title="Error Rate"
            value={`${((liveMetrics?.errorRate ?? 0) * 100).toFixed(2)}%`}
          />
        </Col>
        <Col md={3} sm={6}>
          <MetricCard title="Active Alerts" value={activeAlertCount} />
        </Col>
        <Col md={3} sm={6}>
          <MetricCard
            title="Pod Health"
            value={`${podHealth?.healthyPods ?? 0} / ${podHealth?.totalPods ?? 0}`}
            subtitle={`${podHealth?.degradedPods ?? 0} degraded · ${podHealth?.unhealthyPods ?? 0} unhealthy`}
          />
        </Col>
        <Col md={3} sm={6}>
          <CardSystemHealth status={systemHealth?.overallStatus ?? 'Unknown'} />
        </Col>
      </Row>

      <Row className="g-4 mb-4">
        <Col lg={4}>
          <GpuWidget
            utilizationPercent={liveMetrics?.gpuUtilizationPercent ?? 0}
            memoryUsedBytes={liveMetrics?.gpuMemoryUsedBytes}
            memoryTotalBytes={liveMetrics?.gpuMemoryTotalBytes}
          />
        </Col>
        <Col lg={4}>
          <QueueWidget
            queueSize={liveMetrics?.queueSize ?? 0}
            activeStreams={liveMetrics?.activeStreams ?? 0}
            requestsPerSecond={liveMetrics?.requestsPerSecond}
          />
        </Col>
        <Col lg={4}>
          <div className="d-flex flex-column gap-3 h-100">
            <Link to="/observability/metrics" className="btn btn-outline-primary">
              View Metrics
            </Link>
            <Link to="/observability/analytics" className="btn btn-outline-primary">
              View Analytics
            </Link>
            <Link to="/observability/health" className="btn btn-outline-primary">
              View Health
            </Link>
            <Link to="/observability/costs" className="btn btn-outline-primary">
              View Costs
            </Link>
            <Link to="/observability/alerts" className="btn btn-outline-primary">
              View Alerts
            </Link>
          </div>
        </Col>
      </Row>

      <Row className="g-4">
        <Col lg={12}>
          <AlertPanel alerts={alerts} />
        </Col>
      </Row>
    </div>
  );
};

const CardSystemHealth = ({ status }: { status: string }) => (
  <div className="stat-card card h-100">
    <div className="card-body">
      <h6 className="text-muted mb-2">System Health</h6>
      <HealthStatusBadge status={status} className="fs-6" />
    </div>
  </div>
);
