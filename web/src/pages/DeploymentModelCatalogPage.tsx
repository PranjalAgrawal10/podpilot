import { Link } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { Alert, Badge, Button, Card, CardBody, Spinner, Table } from 'reactstrap';
import { deploymentService } from '../services/deploymentService';
import { useOrganization } from '../contexts/OrganizationContext';
import { getApiErrorMessage } from '../utils/getApiErrorMessage';
import { PERMISSIONS } from '../types';

export const DeploymentModelCatalogPage = () => {
  const { currentOrganization, hasPermission } = useOrganization();
  const canRead = hasPermission(PERMISSIONS.DeploymentRead);

  const { data: models = [], isLoading, error } = useQuery({
    queryKey: ['model-catalog', currentOrganization?.id],
    queryFn: deploymentService.listModelCatalog,
    enabled: !!currentOrganization?.id && canRead,
  });

  if (!currentOrganization) {
    return <Alert color="info">Select an organization to browse the model catalog.</Alert>;
  }

  if (!canRead) {
    return <Alert color="warning">You don&apos;t have permission to view the model catalog.</Alert>;
  }

  return (
    <div>
      <div className="d-flex flex-wrap justify-content-between align-items-start gap-3 mb-4">
        <div>
          <h1 className="page-title mb-1">Model catalog</h1>
          <p className="text-muted mb-0">Curated models for one-click AI pod deployments.</p>
        </div>
        <Button tag={Link} to="/deployments" color="secondary" outline size="sm">
          Deployments
        </Button>
      </div>

      {isLoading && <div className="text-center py-5"><Spinner /></div>}
      {error && <Alert color="danger">{getApiErrorMessage(error, 'Failed to load model catalog')}</Alert>}

      {!isLoading && models.length > 0 && (
        <Card>
          <CardBody className="p-0">
            <Table responsive hover className="mb-0">
              <thead>
                <tr>
                  <th>Name</th>
                  <th>Reference</th>
                  <th>Family</th>
                  <th>VRAM</th>
                  <th>Size</th>
                  <th>GPU</th>
                  <th>Capabilities</th>
                </tr>
              </thead>
              <tbody>
                {models.map((m) => (
                  <tr key={m.id}>
                    <td>
                      <div>{m.name}</div>
                      <div className="text-muted small">{m.code}</div>
                    </td>
                    <td><code>{m.modelReference}</code></td>
                    <td>{m.family} · {m.parameters}</td>
                    <td>{m.requiredVramGb} GB</td>
                    <td>{m.downloadSizeGb} GB</td>
                    <td>
                      <div>{m.recommendedGpuCode}</div>
                      <div className="text-muted small">min {m.minimumGpuCode}</div>
                    </td>
                    <td>
                      {m.supportsVision && <Badge color="info" className="me-1">Vision</Badge>}
                      {m.supportsTools && <Badge color="secondary" className="me-1">Tools</Badge>}
                      {m.supportsEmbeddings && <Badge color="dark">Embed</Badge>}
                      {!m.supportsVision && !m.supportsTools && !m.supportsEmbeddings && '—'}
                    </td>
                  </tr>
                ))}
              </tbody>
            </Table>
          </CardBody>
        </Card>
      )}
    </div>
  );
};
