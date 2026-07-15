import { Link, useParams } from 'react-router-dom';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { toast } from 'react-toastify';
import { Alert, Badge, Button, Card, CardBody, Spinner } from 'reactstrap';
import { aiProviderService } from '../services/aiProviderService';
import { useOrganization } from '../contexts/OrganizationContext';
import { PERMISSIONS } from '../types';

export const AiProviderDetailsPage = () => {
  const { id } = useParams<{ id: string }>();
  const queryClient = useQueryClient();
  const { currentOrganization, hasPermission } = useOrganization();
  const canRead = hasPermission(PERMISSIONS.AiProviderRead);
  const canDelete = hasPermission(PERMISSIONS.AiProviderDelete);
  const canUpdate = hasPermission(PERMISSIONS.AiProviderUpdate);

  const { data: provider, isLoading, error } = useQuery({
    queryKey: ['ai-providers', currentOrganization?.id, id],
    queryFn: () => aiProviderService.getById(id!),
    enabled: !!currentOrganization?.id && !!id && canRead,
  });

  const { data: health } = useQuery({
    queryKey: ['ai-provider-health', currentOrganization?.id, id],
    queryFn: () => aiProviderService.getHealth(id!),
    enabled: !!currentOrganization?.id && !!id && canRead,
  });

  const validateMutation = useMutation({
    mutationFn: () => aiProviderService.validate(id!),
    onSuccess: (result) => {
      toast[result.isValid ? 'success' : 'error'](result.message || (result.isValid ? 'Valid' : 'Invalid'));
      queryClient.invalidateQueries({ queryKey: ['ai-providers', currentOrganization?.id, id] });
    },
    onError: (err: Error) => toast.error(err.message),
  });

  const deleteMutation = useMutation({
    mutationFn: () => aiProviderService.delete(id!),
    onSuccess: () => {
      toast.success('AI provider deleted');
      queryClient.invalidateQueries({ queryKey: ['ai-providers', currentOrganization?.id] });
      window.location.href = '/ai/providers';
    },
    onError: (err: Error) => toast.error(err.message),
  });

  if (!canRead) return <Alert color="warning">Permission denied.</Alert>;
  if (isLoading) return <div className="text-center py-5"><Spinner /></div>;
  if (error || !provider) return <Alert color="danger">{error instanceof Error ? error.message : 'Not found'}</Alert>;

  return (
    <div>
      <div className="d-flex justify-content-between align-items-start mb-4">
        <div>
          <h1 className="page-title mb-1">{provider.displayName}</h1>
          <p className="text-muted mb-0">{provider.name} · {provider.providerKind}</p>
        </div>
        <div className="d-flex gap-2">
          <Button tag={Link} to="/ai/providers" color="secondary" outline>Back</Button>
          {canUpdate && (
            <Button color="primary" outline onClick={() => validateMutation.mutate()} disabled={validateMutation.isPending}>
              Validate
            </Button>
          )}
          {canDelete && (
            <Button color="danger" outline onClick={() => deleteMutation.mutate()} disabled={deleteMutation.isPending}>
              Delete
            </Button>
          )}
        </div>
      </div>

      <Card className="mb-3">
        <CardBody>
          <p><strong>Status:</strong> <Badge color={provider.isEnabled ? 'success' : 'secondary'}>{provider.isEnabled ? 'Enabled' : 'Disabled'}</Badge></p>
          <p><strong>Validated:</strong> {provider.isValidated ? 'Yes' : 'No'}</p>
          <p><strong>Base URL:</strong> {provider.baseUrl || 'Default'}</p>
          <p><strong>Priority:</strong> {provider.priority}</p>
          {health && (
            <p>
              <strong>Health:</strong> {health.status}
              {health.latencyMs != null ? ` · ${health.latencyMs} ms` : ''}
            </p>
          )}
        </CardBody>
      </Card>
    </div>
  );
};
