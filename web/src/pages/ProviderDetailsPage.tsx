import { useState } from 'react';
import { Link, useNavigate, useParams } from 'react-router-dom';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import {
  Button,
  Card,
  CardBody,
  CardHeader,
  Spinner,
  Alert,
  Badge,
  Modal,
  ModalHeader,
  ModalBody,
  ModalFooter,
} from 'reactstrap';
import { toast } from 'react-toastify';
import { providerService } from '../services/providerService';
import { HealthBadge } from '../components/providers/HealthBadge';
import { ConnectionStatus } from '../components/providers/ConnectionStatus';
import { GpuTable } from '../components/providers/GpuTable';
import { TemplateSelector } from '../components/providers/TemplateSelector';
import { useOrganization } from '../contexts/OrganizationContext';
import {
  PERMISSIONS,
  PROVIDER_TYPE_LABELS,
  type ProviderValidationResult,
} from '../types';

export const ProviderDetailsPage = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const { hasPermission } = useOrganization();
  const [isValidating, setIsValidating] = useState(false);
  const [validationResult, setValidationResult] = useState<ProviderValidationResult | null>(null);
  const [deleteModalOpen, setDeleteModalOpen] = useState(false);
  const [isDeleting, setIsDeleting] = useState(false);

  const canRead = hasPermission(PERMISSIONS.ProviderRead);
  const canUpdate = hasPermission(PERMISSIONS.ProviderUpdate);
  const canDelete = hasPermission(PERMISSIONS.ProviderDelete);

  const { data: provider, isLoading, error } = useQuery({
    queryKey: ['provider', id],
    queryFn: () => providerService.getById(id!),
    enabled: !!id && canRead,
  });

  const { data: health, isLoading: healthLoading, refetch: refetchHealth } = useQuery({
    queryKey: ['provider-health', id],
    queryFn: () => providerService.getHealth(id!),
    enabled: !!id && canRead,
    refetchInterval: 60_000,
  });

  const { data: regions = [], isLoading: regionsLoading } = useQuery({
    queryKey: ['provider-regions', id],
    queryFn: () => providerService.getRegions(id!),
    enabled: !!id && canRead,
  });

  const { data: gpus = [], isLoading: gpusLoading } = useQuery({
    queryKey: ['provider-gpus', id],
    queryFn: () => providerService.getGpus(id!),
    enabled: !!id && canRead,
  });

  const { data: templates = [], isLoading: templatesLoading } = useQuery({
    queryKey: ['provider-templates', id],
    queryFn: () => providerService.getTemplates(id!),
    enabled: !!id && canRead,
  });

  const handleTestConnection = async () => {
    if (!id) return;
    setIsValidating(true);
    setValidationResult(null);
    try {
      const result = await providerService.validate(id);
      setValidationResult(result);
      if (result.isValid) {
        toast.success('Connection test successful');
        await queryClient.invalidateQueries({ queryKey: ['provider', id] });
        await queryClient.invalidateQueries({ queryKey: ['provider-health', id] });
        await refetchHealth();
      } else {
        toast.error(result.message || 'Connection test failed');
      }
    } catch (err) {
      const message = err instanceof Error ? err.message : 'Connection test failed';
      toast.error(message);
    } finally {
      setIsValidating(false);
    }
  };

  const handleDelete = async () => {
    if (!id) return;
    setIsDeleting(true);
    try {
      await providerService.delete(id);
      await queryClient.invalidateQueries({ queryKey: ['providers'] });
      toast.success('Provider deleted');
      navigate('/providers');
    } catch (err) {
      const message = err instanceof Error ? err.message : 'Failed to delete provider';
      toast.error(message);
    } finally {
      setIsDeleting(false);
      setDeleteModalOpen(false);
    }
  };

  if (!canRead) {
    return (
      <Alert color="warning">
        You don&apos;t have permission to view provider details.
      </Alert>
    );
  }

  if (isLoading) {
    return (
      <div className="text-center py-5">
        <Spinner />
      </div>
    );
  }

  if (error || !provider) {
    return (
      <Alert color="danger">
        {error instanceof Error ? error.message : 'Provider not found'}
      </Alert>
    );
  }

  const displayStatus = health?.status ?? provider.connectionStatus ?? 'Unknown';

  return (
    <div>
      <div className="d-flex justify-content-between align-items-start mb-4">
        <div>
          <div className="d-flex align-items-center gap-2 mb-1">
            <h1 className="page-title mb-0">{provider.displayName}</h1>
            <HealthBadge status={displayStatus} />
          </div>
          <p className="text-muted mb-0">
            {PROVIDER_TYPE_LABELS[provider.providerType]} · {provider.name}
          </p>
        </div>
        <div className="d-flex gap-2">
          {canUpdate && (
            <Button tag={Link} to={`/providers/${id}/edit`} color="outline-primary" size="sm">
              Edit
            </Button>
          )}
          <Button
            color="info"
            outline
            size="sm"
            disabled={isValidating}
            onClick={() => void handleTestConnection()}
          >
            {isValidating ? 'Testing...' : 'Test Connection'}
          </Button>
        </div>
      </div>

      <div className="row">
        <div className="col-lg-4 mb-4">
          <Card className="provider-detail-card">
            <CardHeader tag="h6" className="mb-0">
              Provider Info
            </CardHeader>
            <CardBody>
              {provider.description && <p className="text-muted">{provider.description}</p>}
              <dl className="provider-details-list mb-0">
                <dt>Status</dt>
                <dd>
                  {provider.isEnabled ? (
                    <Badge color="success">Enabled</Badge>
                  ) : (
                    <Badge color="secondary">Disabled</Badge>
                  )}
                </dd>
                <dt>Validated</dt>
                <dd>{provider.isValidated ? 'Yes' : 'No'}</dd>
                {provider.defaultRegion && (
                  <>
                    <dt>Default Region</dt>
                    <dd>{provider.defaultRegion}</dd>
                  </>
                )}
                {provider.lastValidatedAt && (
                  <>
                    <dt>Last Validated</dt>
                    <dd>{new Date(provider.lastValidatedAt).toLocaleString()}</dd>
                  </>
                )}
                <dt>Created</dt>
                <dd>{new Date(provider.createdAt).toLocaleString()}</dd>
              </dl>
            </CardBody>
          </Card>

          <Card className="provider-detail-card mt-3">
            <CardHeader tag="h6" className="mb-0 d-flex justify-content-between align-items-center">
              <span>Health Status</span>
              {healthLoading && <Spinner size="sm" />}
            </CardHeader>
            <CardBody>
              {health ? (
                <div>
                  <div className="d-flex align-items-center gap-2 mb-2">
                    <HealthBadge status={health.status} />
                    {health.isHealthy ? (
                      <Badge color="success">Healthy</Badge>
                    ) : (
                      <Badge color="danger">Unhealthy</Badge>
                    )}
                  </div>
                  {health.message && <p className="text-muted small mb-2">{health.message}</p>}
                  <p className="text-muted small mb-0">
                    Last checked: {new Date(health.lastCheckedAt).toLocaleString()}
                    {health.responseTimeMs != null && ` · ${health.responseTimeMs}ms`}
                  </p>
                </div>
              ) : (
                <p className="text-muted mb-0">No health data available.</p>
              )}
            </CardBody>
          </Card>
        </div>

        <div className="col-lg-8">
          {validationResult && (
            <Card className="provider-detail-card mb-4">
              <CardHeader tag="h6" className="mb-0">
                Connection Test Result
              </CardHeader>
              <CardBody>
                <ConnectionStatus
                  status={validationResult.connectionStatus}
                  message={validationResult.message}
                  account={validationResult.account}
                  isValid={validationResult.isValid}
                />
              </CardBody>
            </Card>
          )}

          <Card className="provider-detail-card mb-4">
            <CardHeader tag="h6" className="mb-0">
              Regions
              {regionsLoading && <Spinner size="sm" className="ms-2" />}
            </CardHeader>
            <CardBody>
              {regions.length === 0 ? (
                <p className="text-muted mb-0">No regions found.</p>
              ) : (
                <ul className="provider-region-list mb-0">
                  {regions.map((region) => (
                    <li key={region.id}>
                      {region.displayName ?? region.name}
                      {!region.isAvailable && (
                        <Badge color="secondary" className="ms-2">
                          Unavailable
                        </Badge>
                      )}
                    </li>
                  ))}
                </ul>
              )}
            </CardBody>
          </Card>

          <Card className="provider-detail-card mb-4">
            <CardHeader tag="h6" className="mb-0">
              GPU Types
              {gpusLoading && <Spinner size="sm" className="ms-2" />}
            </CardHeader>
            <CardBody>
              <GpuTable gpus={gpus} />
            </CardBody>
          </Card>

          <Card className="provider-detail-card mb-4">
            <CardHeader tag="h6" className="mb-0">
              Templates
              {templatesLoading && <Spinner size="sm" className="ms-2" />}
            </CardHeader>
            <CardBody>
              <TemplateSelector templates={templates} />
            </CardBody>
          </Card>

          {canDelete && (
            <Card className="provider-detail-card border-danger">
              <CardBody>
                <h5 className="text-danger">Danger Zone</h5>
                <p className="text-muted">
                  Permanently delete this provider and its stored credentials.
                </p>
                <Button color="danger" outline onClick={() => setDeleteModalOpen(true)}>
                  Delete Provider
                </Button>
              </CardBody>
            </Card>
          )}
        </div>
      </div>

      <Modal isOpen={deleteModalOpen} toggle={() => setDeleteModalOpen(false)}>
        <ModalHeader toggle={() => setDeleteModalOpen(false)}>Delete Provider</ModalHeader>
        <ModalBody>
          Are you sure you want to delete <strong>{provider.displayName}</strong>? This action
          cannot be undone.
        </ModalBody>
        <ModalFooter>
          <Button color="secondary" onClick={() => setDeleteModalOpen(false)} disabled={isDeleting}>
            Cancel
          </Button>
          <Button color="danger" onClick={() => void handleDelete()} disabled={isDeleting}>
            {isDeleting ? 'Deleting...' : 'Delete'}
          </Button>
        </ModalFooter>
      </Modal>
    </div>
  );
};
