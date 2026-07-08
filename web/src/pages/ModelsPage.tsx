import { useEffect, useMemo, useRef, useState } from 'react';
import { Link } from 'react-router-dom';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { Alert, Button, Card, CardBody, CardTitle, Col, Row } from 'reactstrap';
import { toast } from 'react-toastify';
import { useOrganization } from '../contexts/OrganizationContext';
import { useModelHub } from '../hooks/useModelHub';
import { modelService } from '../services/modelService';
import { podService } from '../services/podService';
import { ModelCard } from '../components/models/ModelCard';
import { ModelSelector } from '../components/models/ModelSelector';
import { DeleteConfirmation } from '../components/models/DeleteConfirmation';
import { DownloadProgressBar } from '../components/models/DownloadProgressBar';
import { LoadingSpinner } from '../components/LoadingSpinner';
import { PERMISSIONS, type AiModel } from '../types';
import { formatBytes } from '../utils/formatBytes';

export const ModelsPage = () => {
  const { currentOrganization, hasPermission } = useOrganization();
  const queryClient = useQueryClient();
  const canRead = hasPermission(PERMISSIONS.ModelRead);
  const canPull = hasPermission(PERMISSIONS.ModelPull);
  const canDelete = hasPermission(PERMISSIONS.ModelDelete);
  const canManage = hasPermission(PERMISSIONS.ModelManage);

  const [selectedPodId, setSelectedPodId] = useState('');
  const [deleteTarget, setDeleteTarget] = useState<AiModel | null>(null);
  const lastSyncedPodId = useRef('');

  useModelHub(currentOrganization?.id);

  const { data: pods = [] } = useQuery({
    queryKey: ['pods', currentOrganization?.id],
    queryFn: podService.list,
    enabled: !!currentOrganization?.id,
  });

  const runningPods = useMemo(
    () => pods.filter((pod) => pod.status === 'Running'),
    [pods],
  );

  useEffect(() => {
    if (selectedPodId || runningPods.length === 0) {
      return;
    }

    setSelectedPodId(runningPods[0].id);
  }, [runningPods, selectedPodId]);

  const podFilter = selectedPodId || undefined;

  const { data: dashboard, isLoading: dashboardLoading } = useQuery({
    queryKey: ['model-dashboard', currentOrganization?.id, podFilter],
    queryFn: () => modelService.getDashboard(podFilter),
    enabled: !!currentOrganization?.id && canRead,
    refetchInterval: 10000,
  });

  const { data: models = [], isLoading: modelsLoading } = useQuery({
    queryKey: ['models', currentOrganization?.id, podFilter],
    queryFn: () => modelService.list(podFilter),
    enabled: !!currentOrganization?.id && canRead,
    refetchInterval: 10000,
  });

  const { data: downloads = [] } = useQuery({
    queryKey: ['model-downloads', currentOrganization?.id],
    queryFn: () => modelService.listDownloads(true),
    enabled: !!currentOrganization?.id && canRead,
    refetchInterval: 5000,
  });

  const invalidate = () => {
    queryClient.invalidateQueries({ queryKey: ['models', currentOrganization?.id] });
    queryClient.invalidateQueries({ queryKey: ['model-dashboard', currentOrganization?.id] });
    queryClient.invalidateQueries({ queryKey: ['model-downloads', currentOrganization?.id] });
  };

  const refreshMutation = useMutation({
    mutationFn: () => modelService.refresh(selectedPodId),
    onSuccess: () => {
      invalidate();
    },
    onError: () => toast.error('Failed to sync models from Ollama'),
  });

  useEffect(() => {
    if (!selectedPodId || !canRead || lastSyncedPodId.current === selectedPodId) {
      return;
    }

    lastSyncedPodId.current = selectedPodId;
    refreshMutation.mutate();
  }, [selectedPodId, canRead]);

  const defaultMutation = useMutation({
    mutationFn: (modelId: string) => modelService.setDefault(modelId),
    onSuccess: () => {
      toast.success('Default model updated');
      invalidate();
    },
    onError: () => toast.error('Failed to set default model'),
  });

  const deleteMutation = useMutation({
    mutationFn: ({ modelId, forceDefault }: { modelId: string; forceDefault: boolean }) =>
      modelService.delete(modelId, forceDefault),
    onSuccess: () => {
      toast.success('Model deleted');
      setDeleteTarget(null);
      invalidate();
    },
    onError: () => toast.error('Failed to delete model'),
  });

  const filteredDownloads = useMemo(
    () => (podFilter ? downloads.filter((d) => d.podId === podFilter) : downloads),
    [downloads, podFilter],
  );

  if (!canRead) {
    return <Alert color="warning">You do not have permission to view models.</Alert>;
  }

  return (
    <div>
      <div className="d-flex justify-content-between align-items-center mb-4 flex-wrap gap-2">
        <div>
          <h1 className="page-title mb-1">Models</h1>
          <p className="text-muted mb-0">Manage Ollama models on your GPU pods.</p>
        </div>
        <div className="d-flex gap-2 flex-wrap">
          <Button tag={Link} to="/models/downloads" color="secondary" outline>
            Downloads
          </Button>
          {canPull && (
            <Button tag={Link} to="/models/pull" color="primary">
              Pull Model
            </Button>
          )}
        </div>
      </div>

      <Card className="mb-4">
        <CardBody>
          <Row className="g-3 align-items-end">
            <Col md={4}>
              <ModelSelector pods={pods} value={selectedPodId} onChange={setSelectedPodId} required={false} />
            </Col>
            <Col md="auto">
              {canRead && selectedPodId && (
                <Button
                  color="secondary"
                  outline
                  disabled={refreshMutation.isPending}
                  onClick={() => {
                    lastSyncedPodId.current = '';
                    refreshMutation.mutate();
                  }}
                >
                  {refreshMutation.isPending ? 'Syncing…' : 'Sync from Ollama'}
                </Button>
              )}
            </Col>
          </Row>
        </CardBody>
      </Card>

      {dashboardLoading ? (
        <LoadingSpinner />
      ) : dashboard && (
        <Row className="g-4 mb-4">
          <Col md={3}>
            <Card className="stat-card">
              <CardBody>
                <CardTitle tag="h6">Installed</CardTitle>
                <p className="stat-value">{dashboard.installedModels}</p>
              </CardBody>
            </Card>
          </Col>
          <Col md={3}>
            <Card className="stat-card">
              <CardBody>
                <CardTitle tag="h6">Downloading</CardTitle>
                <p className="stat-value">{dashboard.downloadingModels}</p>
              </CardBody>
            </Card>
          </Col>
          <Col md={3}>
            <Card className="stat-card">
              <CardBody>
                <CardTitle tag="h6">Storage Used</CardTitle>
                <p className="stat-value" style={{ fontSize: '1.25rem' }}>
                  {formatBytes(dashboard.storageUsedBytes)}
                </p>
              </CardBody>
            </Card>
          </Col>
          <Col md={3}>
            <Card className="stat-card">
              <CardBody>
                <CardTitle tag="h6">Ollama</CardTitle>
                <p className="stat-value" style={{ fontSize: '1.1rem' }}>
                  {dashboard.ollamaDetected ? dashboard.ollamaVersion ?? 'Detected' : 'Not detected'}
                </p>
              </CardBody>
            </Card>
          </Col>
        </Row>
      )}

      {filteredDownloads.length > 0 && (
        <Card className="mb-4">
          <CardBody>
            <CardTitle tag="h5">Active Downloads</CardTitle>
            {filteredDownloads.map((download) => (
              <div key={download.id} className="mb-3">
                <DownloadProgressBar progress={download.progress} label={download.modelName} />
              </div>
            ))}
          </CardBody>
        </Card>
      )}

      {dashboard?.defaultModel && (
        <Alert color="info" className="mb-4">
          Default model: <strong>{dashboard.defaultModel}</strong>
        </Alert>
      )}

      {modelsLoading ? (
        <LoadingSpinner />
      ) : models.length === 0 ? (
        <Alert color="secondary">
          No models found for the selected pod.
          {selectedPodId
            ? ' Ensure Ollama port 11434 is exposed on the pod, then use Sync from Ollama.'
            : ' Select a running GPU pod to discover installed models.'}
        </Alert>
      ) : (
        <Row className="g-4">
          {models.map((model) => (
            <Col md={6} lg={4} key={model.id}>
              <ModelCard
                model={model}
                canManage={canManage}
                canDelete={canDelete}
                onSetDefault={(item) => defaultMutation.mutate(item.id)}
                onDelete={setDeleteTarget}
              />
            </Col>
          ))}
        </Row>
      )}

      <DeleteConfirmation
        model={deleteTarget}
        isOpen={!!deleteTarget}
        onCancel={() => setDeleteTarget(null)}
        onConfirm={(forceDefault) =>
          deleteTarget &&
          deleteMutation.mutate({ modelId: deleteTarget.id, forceDefault })
        }
        isPending={deleteMutation.isPending}
      />
    </div>
  );
};
