import { useState, type FormEvent } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { toast } from 'react-toastify';
import { Alert, Button, Form, FormGroup, Input, Label, Spinner, Table } from 'reactstrap';
import { aiProviderService } from '../services/aiProviderService';
import { useOrganization } from '../contexts/OrganizationContext';
import { PERMISSIONS } from '../types';

export const RoutingPoliciesPage = () => {
  const queryClient = useQueryClient();
  const { currentOrganization, hasPermission } = useOrganization();
  const canRead = hasPermission(PERMISSIONS.AiProviderRead);
  const canCreate = hasPermission(PERMISSIONS.AiProviderCreate);
  const canDelete = hasPermission(PERMISSIONS.AiProviderDelete);

  const { data: policies = [], isLoading, error } = useQuery({
    queryKey: ['ai-routing', currentOrganization?.id],
    queryFn: aiProviderService.listRoutingPolicies,
    enabled: !!currentOrganization?.id && canRead,
  });

  const { data: providers = [] } = useQuery({
    queryKey: ['ai-providers', currentOrganization?.id],
    queryFn: aiProviderService.list,
    enabled: !!currentOrganization?.id && canCreate,
  });

  const [name, setName] = useState('');
  const [modelName, setModelName] = useState('');
  const [primaryProviderId, setPrimaryProviderId] = useState('');
  const [isDefault, setIsDefault] = useState(false);

  const createMutation = useMutation({
    mutationFn: aiProviderService.createRoutingPolicy,
    onSuccess: () => {
      toast.success('Routing policy created');
      queryClient.invalidateQueries({ queryKey: ['ai-routing', currentOrganization?.id] });
      setName('');
      setModelName('');
      setIsDefault(false);
    },
    onError: (err: Error) => toast.error(err.message),
  });

  const deleteMutation = useMutation({
    mutationFn: aiProviderService.deleteRoutingPolicy,
    onSuccess: () => {
      toast.success('Routing policy deleted');
      queryClient.invalidateQueries({ queryKey: ['ai-routing', currentOrganization?.id] });
    },
    onError: (err: Error) => toast.error(err.message),
  });

  if (!canRead) return <Alert color="warning">Permission denied.</Alert>;

  const onSubmit = (event: FormEvent) => {
    event.preventDefault();
    createMutation.mutate({
      name,
      modelName: modelName || undefined,
      primaryProviderId,
      failoverStrategy: 'RetryThenFailover',
      maxRetries: 2,
      isEnabled: true,
      isDefault,
      fallbackProviderIds: [],
    });
  };

  return (
    <div>
      <h1 className="page-title mb-3">Routing Policies</h1>
      {canCreate && (
        <Form onSubmit={onSubmit} className="mb-4" style={{ maxWidth: 640 }}>
          <FormGroup>
            <Label>Name</Label>
            <Input value={name} onChange={(e) => setName(e.target.value)} required />
          </FormGroup>
          <FormGroup>
            <Label>Model name (optional)</Label>
            <Input value={modelName} onChange={(e) => setModelName(e.target.value)} placeholder="Leave empty for default" />
          </FormGroup>
          <FormGroup>
            <Label>Primary provider</Label>
            <Input type="select" value={primaryProviderId} onChange={(e) => setPrimaryProviderId(e.target.value)} required>
              <option value="">Select provider</option>
              {providers.map((p) => (
                <option key={p.id} value={p.id}>{p.displayName}</option>
              ))}
            </Input>
          </FormGroup>
          <FormGroup check className="mb-3">
            <Input type="checkbox" checked={isDefault} onChange={(e) => setIsDefault(e.target.checked)} />
            <Label check>Default policy</Label>
          </FormGroup>
          <Button color="primary" type="submit" disabled={createMutation.isPending}>
            {createMutation.isPending ? <Spinner size="sm" /> : 'Create policy'}
          </Button>
        </Form>
      )}

      {isLoading && <Spinner />}
      {error && <Alert color="danger">{error instanceof Error ? error.message : 'Failed to load policies'}</Alert>}
      {policies.length > 0 && (
        <Table responsive hover>
          <thead>
            <tr>
              <th>Name</th>
              <th>Model</th>
              <th>Primary</th>
              <th>Strategy</th>
              <th />
            </tr>
          </thead>
          <tbody>
            {policies.map((policy) => (
              <tr key={policy.id}>
                <td>{policy.name}{policy.isDefault ? ' (default)' : ''}</td>
                <td>{policy.modelName || '—'}</td>
                <td>{policy.primaryProviderDisplayName || policy.primaryProviderId}</td>
                <td>{policy.failoverStrategy}</td>
                <td>
                  {canDelete && (
                    <Button color="link" className="text-danger p-0" onClick={() => deleteMutation.mutate(policy.id)}>
                      Delete
                    </Button>
                  )}
                </td>
              </tr>
            ))}
          </tbody>
        </Table>
      )}
    </div>
  );
};
