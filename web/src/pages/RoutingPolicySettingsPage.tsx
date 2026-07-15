import { useEffect, useState, type FormEvent } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { toast } from 'react-toastify';
import { Alert, Button, Form, FormGroup, Input, Label, Spinner } from 'reactstrap';
import { routingService } from '../services/routingService';
import { useOrganization } from '../contexts/OrganizationContext';
import { PERMISSIONS } from '../types';

const STRATEGIES = [
  'LowestCost',
  'LowestLatency',
  'HighestAccuracy',
  'Balanced',
  'ProviderPriority',
  'CustomRules',
  'OrganizationRules',
];

export const RoutingPolicySettingsPage = () => {
  const queryClient = useQueryClient();
  const { currentOrganization, hasPermission } = useOrganization();
  const canRead = hasPermission(PERMISSIONS.RoutingRead);
  const canManage = hasPermission(PERMISSIONS.RoutingManage);

  const { data, isLoading, error } = useQuery({
    queryKey: ['routing-policy', currentOrganization?.id],
    queryFn: routingService.getPolicy,
    enabled: !!currentOrganization?.id && canRead,
  });

  const [strategy, setStrategy] = useState('Balanced');
  const [costWeight, setCostWeight] = useState(0.25);
  const [latencyWeight, setLatencyWeight] = useState(0.25);
  const [reliabilityWeight, setReliabilityWeight] = useState(0.2);
  const [contextWeight, setContextWeight] = useState(0.1);
  const [featuresWeight, setFeaturesWeight] = useState(0.1);
  const [availabilityWeight, setAvailabilityWeight] = useState(0.1);
  const [maxRetries, setMaxRetries] = useState(2);

  useEffect(() => {
    if (!data) return;
    setStrategy(data.strategy);
    setCostWeight(data.costWeight);
    setLatencyWeight(data.latencyWeight);
    setReliabilityWeight(data.reliabilityWeight);
    setContextWeight(data.contextWeight);
    setFeaturesWeight(data.featuresWeight);
    setAvailabilityWeight(data.availabilityWeight);
    setMaxRetries(data.maxRetries);
  }, [data]);

  const mutation = useMutation({
    mutationFn: routingService.updatePolicy,
    onSuccess: () => {
      toast.success('Routing policy updated');
      queryClient.invalidateQueries({ queryKey: ['routing-policy', currentOrganization?.id] });
      queryClient.invalidateQueries({ queryKey: ['routing', currentOrganization?.id] });
    },
    onError: (err: Error) => toast.error(err.message),
  });

  if (!canRead) return <Alert color="warning">Permission denied.</Alert>;
  if (isLoading) return <Spinner />;
  if (error) return <Alert color="danger">{(error as Error).message}</Alert>;

  const onSubmit = (event: FormEvent) => {
    event.preventDefault();
    mutation.mutate({
      strategy,
      costWeight,
      latencyWeight,
      reliabilityWeight,
      contextWeight,
      featuresWeight,
      availabilityWeight,
      maxRetries,
      failoverStrategy: data?.failoverStrategy || 'RetryThenFailover',
      primaryProviderId: data?.primaryProviderId,
      fallbackProviderIds: data?.fallbackProviderIds || [],
      preferredTaskTypes: data?.preferredTaskTypes || [],
    });
  };

  return (
    <div style={{ maxWidth: 640 }}>
      <h1 className="page-title mb-3">Routing Policies</h1>
      <p className="text-muted">Choose how PodPilot selects models and providers for your organization.</p>
      <Form onSubmit={onSubmit}>
        <FormGroup>
          <Label>Strategy</Label>
          <Input type="select" value={strategy} onChange={(e) => setStrategy(e.target.value)} disabled={!canManage}>
            {STRATEGIES.map((s) => (
              <option key={s} value={s}>{s}</option>
            ))}
          </Input>
        </FormGroup>
        {[
          ['Cost', costWeight, setCostWeight],
          ['Latency', latencyWeight, setLatencyWeight],
          ['Reliability', reliabilityWeight, setReliabilityWeight],
          ['Context', contextWeight, setContextWeight],
          ['Features', featuresWeight, setFeaturesWeight],
          ['Availability', availabilityWeight, setAvailabilityWeight],
        ].map(([label, value, setter]) => (
          <FormGroup key={label as string}>
            <Label>{label as string} weight</Label>
            <Input
              type="number"
              step="0.01"
              min="0"
              max="1"
              value={value as number}
              onChange={(e) => (setter as (v: number) => void)(Number(e.target.value))}
              disabled={!canManage}
            />
          </FormGroup>
        ))}
        <FormGroup>
          <Label>Max retries</Label>
          <Input
            type="number"
            min={0}
            max={10}
            value={maxRetries}
            onChange={(e) => setMaxRetries(Number(e.target.value))}
            disabled={!canManage}
          />
        </FormGroup>
        {canManage && (
          <Button color="primary" type="submit" disabled={mutation.isPending}>
            {mutation.isPending ? <Spinner size="sm" /> : 'Save policy'}
          </Button>
        )}
      </Form>
    </div>
  );
};
