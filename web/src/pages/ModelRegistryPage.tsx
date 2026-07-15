import { useQuery } from '@tanstack/react-query';
import { Alert, Badge, Spinner, Table } from 'reactstrap';
import { aiProviderService } from '../services/aiProviderService';
import { useOrganization } from '../contexts/OrganizationContext';
import { useAiProviderHub } from '../hooks/useAiProviderHub';
import { PERMISSIONS } from '../types';

export const ModelRegistryPage = () => {
  const { currentOrganization, hasPermission } = useOrganization();
  const canRead = hasPermission(PERMISSIONS.AiProviderRead);
  useAiProviderHub(currentOrganization?.id);

  const { data: models = [], isLoading, error } = useQuery({
    queryKey: ['ai-models', currentOrganization?.id],
    queryFn: () => aiProviderService.listModels(),
    enabled: !!currentOrganization?.id && canRead,
  });

  if (!canRead) return <Alert color="warning">Permission denied.</Alert>;

  return (
    <div>
      <h1 className="page-title mb-3">Model Registry</h1>
      <p className="text-muted">Unified catalog across connected AI providers.</p>
      {isLoading && <Spinner />}
      {error && <Alert color="danger">{error instanceof Error ? error.message : 'Failed to load models'}</Alert>}
      {!isLoading && models.length === 0 && <Alert color="info">No models synced yet.</Alert>}
      {models.length > 0 && (
        <Table responsive hover>
          <thead>
            <tr>
              <th>Model</th>
              <th>Provider</th>
              <th>Capabilities</th>
              <th>Synced</th>
            </tr>
          </thead>
          <tbody>
            {models.map((model) => (
              <tr key={model.id}>
                <td>
                  <div>{model.displayName || model.modelName}</div>
                  <small className="text-muted">{model.modelName}</small>
                </td>
                <td>{model.providerDisplayName} <Badge color="light">{model.providerKind}</Badge></td>
                <td>
                  {model.supportsStreaming && <Badge color="info" className="me-1">stream</Badge>}
                  {model.supportsVision && <Badge color="info" className="me-1">vision</Badge>}
                  {model.supportsTools && <Badge color="info" className="me-1">tools</Badge>}
                  {model.supportsEmbeddings && <Badge color="info">embeddings</Badge>}
                </td>
                <td>{new Date(model.syncedAt).toLocaleString()}</td>
              </tr>
            ))}
          </tbody>
        </Table>
      )}
    </div>
  );
};
