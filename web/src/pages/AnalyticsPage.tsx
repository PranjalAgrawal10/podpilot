import { useMemo, useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { Alert, Card, CardBody, CardTitle, Col, Row } from 'reactstrap';
import { MetricCard } from '../components/observability/MetricCard';
import { ObservabilityBarChart } from '../components/observability/ObservabilityBarChart';
import { ObservabilityFilters } from '../components/observability/ObservabilityFilters';
import { ObservabilityPieChart } from '../components/observability/ObservabilityPieChart';
import { LoadingSpinner } from '../components/LoadingSpinner';
import { useOrganization } from '../contexts/OrganizationContext';
import { useObservabilityHub } from '../hooks/useObservabilityHub';
import { observabilityService } from '../services/observabilityService';
import type { ObservabilityFilters as Filters } from '../types';
import { PERMISSIONS } from '../types';

export const AnalyticsPage = () => {
  const { currentOrganization, hasPermission } = useOrganization();
  const canRead = hasPermission(PERMISSIONS.ObservabilityRead);
  const [filters, setFilters] = useState<Filters>({ period: 'Daily' });

  useObservabilityHub(currentOrganization?.id);

  const { data: analytics, isLoading } = useQuery({
    queryKey: ['analytics', currentOrganization?.id, filters],
    queryFn: () =>
      observabilityService.getAnalytics({
        period: filters.period,
        from: filters.from,
        to: filters.to,
        providerId: filters.providerId,
        podId: filters.podId,
        model: filters.model,
      }),
    enabled: !!currentOrganization?.id && canRead,
    refetchInterval: 15000,
  });

  const modelChartData = useMemo(
    () =>
      (analytics?.modelBreakdowns ?? []).map((item) => ({
        name: item.modelName,
        requests: item.requestCount,
        tokens: item.tokenCount,
      })),
    [analytics],
  );

  const providerChartData = useMemo(
    () =>
      (analytics?.providerBreakdowns ?? []).map((item) => ({
        name: item.providerName,
        requests: item.requestCount,
        inferences: item.inferenceCount,
      })),
    [analytics],
  );

  const modelPieData = useMemo(
    () =>
      (analytics?.modelBreakdowns ?? []).map((item) => ({
        name: item.modelName,
        value: item.requestCount,
      })),
    [analytics],
  );

  const providerPieData = useMemo(
    () =>
      (analytics?.providerBreakdowns ?? []).map((item) => ({
        name: item.providerName,
        value: item.requestCount,
      })),
    [analytics],
  );

  if (!currentOrganization) {
    return <Alert color="info">Select an organization to view analytics.</Alert>;
  }

  if (!canRead) {
    return <Alert color="warning">You do not have permission to view analytics.</Alert>;
  }

  if (isLoading && !analytics) {
    return <LoadingSpinner />;
  }

  return (
    <div>
      <h1 className="page-title">Analytics</h1>
      <p className="text-muted mb-4">Request volume, model usage, and provider usage.</p>

      <ObservabilityFilters filters={filters} onChange={setFilters} showPeriod showDateRange />

      <Row className="g-4 mb-4">
        <Col md={3} sm={6}>
          <MetricCard title="Total Requests" value={(analytics?.totalRequests ?? 0).toLocaleString()} />
        </Col>
        <Col md={3} sm={6}>
          <MetricCard title="Total Tokens" value={(analytics?.totalTokens ?? 0).toLocaleString()} />
        </Col>
        <Col md={3} sm={6}>
          <MetricCard title="Total Inferences" value={(analytics?.totalInferences ?? 0).toLocaleString()} />
        </Col>
        <Col md={3} sm={6}>
          <MetricCard
            title="Avg Latency"
            value={`${(analytics?.averageLatencyMs ?? 0).toFixed(0)} ms`}
          />
        </Col>
      </Row>

      <Row className="g-4">
        <Col lg={6}>
          <Card>
            <CardBody>
              <CardTitle tag="h5">Request Volume by Model</CardTitle>
              {modelChartData.length === 0 ? (
                <Alert color="info" className="mb-0">No model usage data.</Alert>
              ) : (
                <ObservabilityBarChart
                  data={modelChartData}
                  xKey="name"
                  series={[{ key: 'requests', name: 'Requests' }]}
                />
              )}
            </CardBody>
          </Card>
        </Col>
        <Col lg={6}>
          <Card>
            <CardBody>
              <CardTitle tag="h5">Request Volume by Provider</CardTitle>
              {providerChartData.length === 0 ? (
                <Alert color="info" className="mb-0">No provider usage data.</Alert>
              ) : (
                <ObservabilityBarChart
                  data={providerChartData}
                  xKey="name"
                  series={[
                    { key: 'requests', name: 'Requests', color: '#0d6efd' },
                    { key: 'inferences', name: 'Inferences', color: '#198754' },
                  ]}
                />
              )}
            </CardBody>
          </Card>
        </Col>
        <Col lg={6}>
          <Card>
            <CardBody>
              <CardTitle tag="h5">Model Usage Distribution</CardTitle>
              {modelPieData.length === 0 ? (
                <Alert color="info" className="mb-0">No model usage data.</Alert>
              ) : (
                <ObservabilityPieChart data={modelPieData} />
              )}
            </CardBody>
          </Card>
        </Col>
        <Col lg={6}>
          <Card>
            <CardBody>
              <CardTitle tag="h5">Provider Usage Distribution</CardTitle>
              {providerPieData.length === 0 ? (
                <Alert color="info" className="mb-0">No provider usage data.</Alert>
              ) : (
                <ObservabilityPieChart data={providerPieData} />
              )}
            </CardBody>
          </Card>
        </Col>
      </Row>
    </div>
  );
};
