import { useMemo, useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { toast } from 'react-toastify';
import { Alert, Button, Card, CardBody, CardTitle, Col, Row } from 'reactstrap';
import { CostSummaryCard } from '../components/observability/CostSummaryCard';
import { ObservabilityBarChart } from '../components/observability/ObservabilityBarChart';
import { ObservabilityFilters } from '../components/observability/ObservabilityFilters';
import { ObservabilityPieChart } from '../components/observability/ObservabilityPieChart';
import { LoadingSpinner } from '../components/LoadingSpinner';
import { useOrganization } from '../contexts/OrganizationContext';
import { useObservabilityHub } from '../hooks/useObservabilityHub';
import { observabilityService } from '../services/observabilityService';
import type { ObservabilityExportFormat, ObservabilityFilters as Filters } from '../types';
import { PERMISSIONS } from '../types';

export const CostsPage = () => {
  const { currentOrganization, hasPermission } = useOrganization();
  const canRead = hasPermission(PERMISSIONS.ObservabilityRead);
  const canExport = hasPermission(PERMISSIONS.ObservabilityExport);
  const [filters, setFilters] = useState<Filters>({ period: 'Daily' });
  const [isExporting, setIsExporting] = useState(false);

  useObservabilityHub(currentOrganization?.id);

  const { data: cost, isLoading } = useQuery({
    queryKey: ['cost', currentOrganization?.id, filters],
    queryFn: () =>
      observabilityService.getCost({
        period: filters.period,
        providerId: filters.providerId,
        podId: filters.podId,
        model: filters.model,
      }),
    enabled: !!currentOrganization?.id && canRead,
    refetchInterval: 15000,
  });

  const podChartData = useMemo(
    () =>
      (cost?.podBreakdowns ?? []).map((item) => ({
        name: item.podName,
        periodCost: Number(item.periodCost),
        hourlyCost: Number(item.hourlyCost),
      })),
    [cost],
  );

  const providerChartData = useMemo(
    () =>
      (cost?.providerBreakdowns ?? []).map((item) => ({
        name: item.providerName,
        periodCost: Number(item.periodCost),
      })),
    [cost],
  );

  const providerPieData = useMemo(
    () =>
      (cost?.providerBreakdowns ?? []).map((item) => ({
        name: item.providerName,
        value: Number(item.periodCost),
      })),
    [cost],
  );

  const modelChartData = useMemo(
    () =>
      (cost?.modelBreakdowns ?? []).map((item) => ({
        name: item.modelName,
        periodCost: Number(item.periodCost),
        hourlyCost: Number(item.hourlyCost),
      })),
    [cost],
  );

  const handleExport = async (format: ObservabilityExportFormat) => {
    if (!canExport) {
      toast.warning('You do not have permission to export observability data.');
      return;
    }

    setIsExporting(true);
    try {
      const blob = await observabilityService.exportData({
        format,
        type: 'cost',
        from: filters.from,
        to: filters.to,
        providerId: filters.providerId,
        podId: filters.podId,
        model: filters.model,
      });

      const extension = format === 'excel' ? 'xlsx' : format;
      const url = URL.createObjectURL(blob);
      const link = document.createElement('a');
      link.href = url;
      link.download = `observability-cost.${extension}`;
      document.body.appendChild(link);
      link.click();
      link.remove();
      URL.revokeObjectURL(url);
      toast.success('Cost data exported successfully.');
    } catch {
      toast.error('Failed to export cost data.');
    } finally {
      setIsExporting(false);
    }
  };

  if (!currentOrganization) {
    return <Alert color="info">Select an organization to view costs.</Alert>;
  }

  if (!canRead) {
    return <Alert color="warning">You do not have permission to view costs.</Alert>;
  }

  if (isLoading && !cost) {
    return <LoadingSpinner />;
  }

  return (
    <div>
      <div className="d-flex justify-content-between align-items-start mb-4 flex-wrap gap-2">
        <div>
          <h1 className="page-title mb-1">Costs</h1>
          <p className="text-muted mb-0">Cost summaries and breakdowns across pods and providers.</p>
        </div>
        {canExport && (
          <div className="d-flex gap-2">
            <Button color="outline-primary" size="sm" disabled={isExporting} onClick={() => handleExport('csv')}>
              Export CSV
            </Button>
            <Button color="outline-primary" size="sm" disabled={isExporting} onClick={() => handleExport('json')}>
              Export JSON
            </Button>
            <Button color="outline-primary" size="sm" disabled={isExporting} onClick={() => handleExport('excel')}>
              Export Excel
            </Button>
          </div>
        )}
      </div>

      <ObservabilityFilters filters={filters} onChange={setFilters} showPeriod />

      {cost && <CostSummaryCard cost={cost} />}

      <Row className="g-4 mt-2">
        <Col lg={6}>
          <Card>
            <CardBody>
              <CardTitle tag="h5">Cost by Pod</CardTitle>
              {podChartData.length === 0 ? (
                <Alert color="info" className="mb-0">No pod cost breakdown available.</Alert>
              ) : (
                <ObservabilityBarChart
                  data={podChartData}
                  xKey="name"
                  series={[
                    { key: 'periodCost', name: 'Period Cost', color: '#0d6efd' },
                    { key: 'hourlyCost', name: 'Hourly Cost', color: '#198754' },
                  ]}
                />
              )}
            </CardBody>
          </Card>
        </Col>
        <Col lg={6}>
          <Card>
            <CardBody>
              <CardTitle tag="h5">Cost by Provider</CardTitle>
              {providerChartData.length === 0 ? (
                <Alert color="info" className="mb-0">No provider cost breakdown available.</Alert>
              ) : (
                <ObservabilityBarChart
                  data={providerChartData}
                  xKey="name"
                  series={[{ key: 'periodCost', name: 'Period Cost', color: '#ffc107' }]}
                />
              )}
            </CardBody>
          </Card>
        </Col>
        <Col lg={6}>
          <Card>
            <CardBody>
              <CardTitle tag="h5">Provider Cost Distribution</CardTitle>
              {providerPieData.length === 0 ? (
                <Alert color="info" className="mb-0">No provider cost data.</Alert>
              ) : (
                <ObservabilityPieChart data={providerPieData} />
              )}
            </CardBody>
          </Card>
        </Col>
        <Col lg={6}>
          <Card>
            <CardBody>
              <CardTitle tag="h5">Cost by Model</CardTitle>
              {modelChartData.length === 0 ? (
                <Alert color="info" className="mb-0">No model cost breakdown available.</Alert>
              ) : (
                <ObservabilityBarChart
                  data={modelChartData}
                  xKey="name"
                  series={[
                    { key: 'periodCost', name: 'Period Cost', color: '#6610f2' },
                    { key: 'hourlyCost', name: 'Hourly Cost', color: '#20c997' },
                  ]}
                />
              )}
            </CardBody>
          </Card>
        </Col>
      </Row>
    </div>
  );
};
