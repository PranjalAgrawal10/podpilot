import { Link } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { Alert, Badge, Button, Card, CardBody, Spinner, Table } from 'reactstrap';
import { deploymentService } from '../services/deploymentService';
import { useOrganization } from '../contexts/OrganizationContext';
import { getApiErrorMessage } from '../utils/getApiErrorMessage';
import { PERMISSIONS } from '../types';

export const GpuCatalogPage = () => {
  const { currentOrganization, hasPermission } = useOrganization();
  const canRead = hasPermission(PERMISSIONS.DeploymentRead);

  const { data: gpus = [], isLoading, error } = useQuery({
    queryKey: ['gpu-catalog', currentOrganization?.id],
    queryFn: deploymentService.listGpus,
    enabled: !!currentOrganization?.id && canRead,
  });

  if (!currentOrganization) {
    return <Alert color="info">Select an organization to browse the GPU catalog.</Alert>;
  }

  if (!canRead) {
    return <Alert color="warning">You don&apos;t have permission to view the GPU catalog.</Alert>;
  }

  return (
    <div>
      <div className="d-flex flex-wrap justify-content-between align-items-start gap-3 mb-4">
        <div>
          <h1 className="page-title mb-1">GPU catalog</h1>
          <p className="text-muted mb-0">Recommended GPUs with VRAM and estimated hourly cost.</p>
        </div>
        <Button tag={Link} to="/deployments" color="secondary" outline size="sm">
          Deployments
        </Button>
      </div>

      {isLoading && <div className="text-center py-5"><Spinner /></div>}
      {error && <Alert color="danger">{getApiErrorMessage(error, 'Failed to load GPU catalog')}</Alert>}

      {!isLoading && gpus.length > 0 && (
        <Card>
          <CardBody className="p-0">
            <Table responsive hover className="mb-0">
              <thead>
                <tr>
                  <th>Name</th>
                  <th>Code</th>
                  <th>Type</th>
                  <th>VRAM</th>
                  <th>CUDA</th>
                  <th>$/hr</th>
                  <th>Providers</th>
                </tr>
              </thead>
              <tbody>
                {gpus.map((g) => (
                  <tr key={g.id}>
                    <td>
                      {g.name}
                      {g.isCustom && <Badge color="warning" className="ms-2">Custom</Badge>}
                    </td>
                    <td><code>{g.code}</code></td>
                    <td>{g.gpuType}</td>
                    <td>{g.vramGb} GB</td>
                    <td>{g.cudaCapability}</td>
                    <td>${g.estimatedHourlyCostUsd.toFixed(2)}</td>
                    <td>
                      {g.providerAvailability.length === 0
                        ? '—'
                        : g.providerAvailability.map((p) => (
                          <Badge key={p} color="secondary" className="me-1">{p}</Badge>
                        ))}
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
