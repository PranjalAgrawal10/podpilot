import { useEffect, useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import {
  Alert,
  Button,
  Card,
  CardBody,
  CardTitle,
  Form,
  FormGroup,
  Input,
  Label,
} from 'reactstrap';
import { toast } from 'react-toastify';
import { useOrganization } from '../contexts/OrganizationContext';
import { orchestratorService } from '../services/orchestratorService';
import { LoadingSpinner } from '../components/LoadingSpinner';
import { LOAD_BALANCING_STRATEGIES, PERMISSIONS, type LoadBalancerConfig } from '../types';

export const LoadBalancerPage = () => {
  const queryClient = useQueryClient();
  const { currentOrganization, hasPermission } = useOrganization();
  const canRead = hasPermission(PERMISSIONS.OrchestratorRead);
  const canManage = hasPermission(PERMISSIONS.OrchestratorManage);

  const [form, setForm] = useState<LoadBalancerConfig>({
    strategy: 'LeastBusy',
    stickySessionsEnabled: false,
    stickySessionTtlMinutes: 30,
  });

  const { data: config, isLoading, error } = useQuery({
    queryKey: ['load-balancer-config', currentOrganization?.id],
    queryFn: orchestratorService.getLoadBalancerConfig,
    enabled: !!currentOrganization?.id && canRead,
  });

  useEffect(() => {
    if (config) {
      setForm(config);
    }
  }, [config]);

  const updateMutation = useMutation({
    mutationFn: () => orchestratorService.updateLoadBalancerConfig(form),
    onSuccess: () => {
      toast.success('Load balancer configuration updated');
      queryClient.invalidateQueries({
        queryKey: ['load-balancer-config', currentOrganization?.id],
      });
    },
    onError: (err) =>
      toast.error(err instanceof Error ? err.message : 'Failed to update load balancer config'),
  });

  if (!currentOrganization) {
    return <Alert color="info">Select an organization to view load balancer settings.</Alert>;
  }

  if (!canRead) {
    return <Alert color="warning">You do not have permission to view load balancer settings.</Alert>;
  }

  return (
    <div>
      <h1 className="page-title">Load Balancer</h1>
      <p className="text-muted mb-4">
        Configure how requests are distributed across pods in your orchestration pools.
      </p>

      {isLoading && <LoadingSpinner />}
      {error && (
        <Alert color="danger">
          {error instanceof Error ? error.message : 'Failed to load load balancer config'}
        </Alert>
      )}

      {!isLoading && !error && (
        <Card>
          <CardBody>
            <CardTitle tag="h5">Routing Strategy</CardTitle>
            <Form
              onSubmit={(e) => {
                e.preventDefault();
                if (canManage) {
                  updateMutation.mutate();
                }
              }}
            >
              <FormGroup>
                <Label for="lbStrategy">Strategy</Label>
                <Input
                  id="lbStrategy"
                  type="select"
                  value={form.strategy}
                  disabled={!canManage}
                  onChange={(e) => setForm((prev) => ({ ...prev, strategy: e.target.value }))}
                >
                  {LOAD_BALANCING_STRATEGIES.map((strategy) => (
                    <option key={strategy} value={strategy}>
                      {strategy}
                    </option>
                  ))}
                </Input>
              </FormGroup>

              <FormGroup check>
                <Input
                  id="stickySessions"
                  type="checkbox"
                  checked={form.stickySessionsEnabled}
                  disabled={!canManage}
                  onChange={(e) =>
                    setForm((prev) => ({ ...prev, stickySessionsEnabled: e.target.checked }))
                  }
                />
                <Label for="stickySessions" check>
                  Enable sticky sessions
                </Label>
              </FormGroup>

              <FormGroup>
                <Label for="stickyTtl">Sticky session TTL (minutes)</Label>
                <Input
                  id="stickyTtl"
                  type="number"
                  min={1}
                  max={1440}
                  value={form.stickySessionTtlMinutes}
                  disabled={!canManage || !form.stickySessionsEnabled}
                  onChange={(e) =>
                    setForm((prev) => ({
                      ...prev,
                      stickySessionTtlMinutes: Number(e.target.value),
                    }))
                  }
                />
              </FormGroup>

              {canManage && (
                <Button color="primary" type="submit" disabled={updateMutation.isPending}>
                  Save Configuration
                </Button>
              )}
            </Form>
          </CardBody>
        </Card>
      )}
    </div>
  );
};
