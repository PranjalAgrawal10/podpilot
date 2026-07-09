import { useMemo } from 'react';
import { useQuery } from '@tanstack/react-query';
import { Card, CardBody, Col, FormGroup, Input, Label, Row } from 'reactstrap';
import { useOrganization } from '../../contexts/OrganizationContext';
import { modelService } from '../../services/modelService';
import { podService } from '../../services/podService';
import { providerService } from '../../services/providerService';
import type { MetricsPeriod, ObservabilityFilters as ObservabilityFilterValues } from '../../types';
import { PERMISSIONS } from '../../types';

interface ObservabilityFiltersProps {
  filters: ObservabilityFilterValues;
  onChange: (filters: ObservabilityFilterValues) => void;
  showPeriod?: boolean;
  showDateRange?: boolean;
}

const PERIOD_OPTIONS: MetricsPeriod[] = ['Hourly', 'Daily', 'Weekly', 'Monthly'];

export const ObservabilityFilters = ({
  filters,
  onChange,
  showPeriod = false,
  showDateRange = true,
}: ObservabilityFiltersProps) => {
  const { currentOrganization, hasPermission } = useOrganization();
  const canRead = hasPermission(PERMISSIONS.ObservabilityRead);

  const { data: providers = [] } = useQuery({
    queryKey: ['providers', currentOrganization?.id],
    queryFn: providerService.list,
    enabled: !!currentOrganization?.id && canRead,
  });

  const { data: pods = [] } = useQuery({
    queryKey: ['pods', currentOrganization?.id],
    queryFn: podService.list,
    enabled: !!currentOrganization?.id && canRead,
  });

  const { data: models = [] } = useQuery({
    queryKey: ['models', currentOrganization?.id, filters.podId || 'all'],
    queryFn: () => modelService.list(filters.podId || undefined),
    enabled: !!currentOrganization?.id && canRead,
  });

  const modelOptions = useMemo(() => {
    const names = new Set(models.map((model) => model.fullName));
    if (filters.model) {
      names.add(filters.model);
    }
    return Array.from(names).sort();
  }, [filters.model, models]);

  const updateFilter = (key: keyof ObservabilityFilterValues, value: string) => {
    onChange({
      ...filters,
      [key]: value || undefined,
      ...(key === 'podId' && value !== filters.podId ? { model: undefined } : {}),
    });
  };

  return (
    <Card className="mb-4">
      <CardBody>
        <Row className="g-3">
          <Col md={3}>
            <FormGroup>
              <Label for="obsProvider">Provider</Label>
              <Input
                id="obsProvider"
                type="select"
                value={filters.providerId ?? ''}
                onChange={(e) => updateFilter('providerId', e.target.value)}
              >
                <option value="">All providers</option>
                {providers.map((provider) => (
                  <option key={provider.id} value={provider.id}>
                    {provider.name}
                  </option>
                ))}
              </Input>
            </FormGroup>
          </Col>
          <Col md={3}>
            <FormGroup>
              <Label for="obsPod">Pod</Label>
              <Input
                id="obsPod"
                type="select"
                value={filters.podId ?? ''}
                onChange={(e) => updateFilter('podId', e.target.value)}
              >
                <option value="">All pods</option>
                {pods.map((pod) => (
                  <option key={pod.id} value={pod.id}>
                    {pod.name}
                  </option>
                ))}
              </Input>
            </FormGroup>
          </Col>
          <Col md={3}>
            <FormGroup>
              <Label for="obsModel">Model</Label>
              <Input
                id="obsModel"
                type="select"
                value={filters.model ?? ''}
                onChange={(e) => updateFilter('model', e.target.value)}
              >
                <option value="">All models</option>
                {modelOptions.map((modelName) => (
                  <option key={modelName} value={modelName}>
                    {modelName}
                  </option>
                ))}
              </Input>
            </FormGroup>
          </Col>
          {showPeriod && (
            <Col md={3}>
              <FormGroup>
                <Label for="obsPeriod">Period</Label>
                <Input
                  id="obsPeriod"
                  type="select"
                  value={filters.period ?? 'Daily'}
                  onChange={(e) => updateFilter('period', e.target.value)}
                >
                  {PERIOD_OPTIONS.map((period) => (
                    <option key={period} value={period}>
                      {period}
                    </option>
                  ))}
                </Input>
              </FormGroup>
            </Col>
          )}
          {showDateRange && (
            <>
              <Col md={3}>
                <FormGroup>
                  <Label for="obsFrom">From</Label>
                  <Input
                    id="obsFrom"
                    type="datetime-local"
                    value={filters.from ?? ''}
                    onChange={(e) => updateFilter('from', e.target.value)}
                  />
                </FormGroup>
              </Col>
              <Col md={3}>
                <FormGroup>
                  <Label for="obsTo">To</Label>
                  <Input
                    id="obsTo"
                    type="datetime-local"
                    value={filters.to ?? ''}
                    onChange={(e) => updateFilter('to', e.target.value)}
                  />
                </FormGroup>
              </Col>
            </>
          )}
        </Row>
      </CardBody>
    </Card>
  );
};
