import { Link } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { Alert, Button, Card, CardBody, CardTitle, Table } from 'reactstrap';
import { useOrganization } from '../contexts/OrganizationContext';
import { useModelHub } from '../hooks/useModelHub';
import { modelService } from '../services/modelService';
import { DownloadProgressBar } from '../components/models/DownloadProgressBar';
import { LoadingSpinner } from '../components/LoadingSpinner';
import { PERMISSIONS } from '../types';
import { formatBytes } from '../utils/formatBytes';

export const ModelDownloadsPage = () => {
  const { currentOrganization, hasPermission } = useOrganization();
  const canRead = hasPermission(PERMISSIONS.ModelRead);

  useModelHub(currentOrganization?.id);

  const { data: activeDownloads = [], isLoading: activeLoading } = useQuery({
    queryKey: ['model-downloads', currentOrganization?.id, 'active'],
    queryFn: () => modelService.listDownloads(true),
    enabled: !!currentOrganization?.id && canRead,
    refetchInterval: 3000,
  });

  const { data: allDownloads = [], isLoading: allLoading } = useQuery({
    queryKey: ['model-downloads', currentOrganization?.id, 'all'],
    queryFn: () => modelService.listDownloads(false),
    enabled: !!currentOrganization?.id && canRead,
  });

  if (!canRead) {
    return <Alert color="warning">You do not have permission to view downloads.</Alert>;
  }

  return (
    <div>
      <Button tag={Link} to="/models" color="link" className="p-0 mb-2">
        ← Back to Models
      </Button>
      <h1 className="page-title mb-4">Model Downloads</h1>

      <Card className="mb-4">
        <CardBody>
          <CardTitle tag="h5">Active Downloads</CardTitle>
          {activeLoading ? (
            <LoadingSpinner />
          ) : activeDownloads.length === 0 ? (
            <p className="text-muted mb-0">No active downloads.</p>
          ) : (
            activeDownloads.map((download) => (
              <div key={download.id} className="mb-4">
                <div className="d-flex justify-content-between mb-2">
                  <strong>{download.modelName}</strong>
                  <span className="text-muted">{download.status}</span>
                </div>
                <DownloadProgressBar progress={download.progress} />
                {download.downloadSpeed && (
                  <small className="text-muted">{formatBytes(download.downloadSpeed)}/s</small>
                )}
              </div>
            ))
          )}
        </CardBody>
      </Card>

      <Card>
        <CardBody>
          <CardTitle tag="h5">Download History</CardTitle>
          {allLoading ? (
            <LoadingSpinner />
          ) : (
            <Table responsive hover>
              <thead>
                <tr>
                  <th>Model</th>
                  <th>Status</th>
                  <th>Progress</th>
                  <th>Started</th>
                  <th>Completed</th>
                  <th>Error</th>
                </tr>
              </thead>
              <tbody>
                {allDownloads.map((download) => (
                  <tr key={download.id}>
                    <td>{download.modelName}</td>
                    <td>{download.status}</td>
                    <td>{download.progress}%</td>
                    <td>{new Date(download.startedAt).toLocaleString()}</td>
                    <td>{download.completedAt ? new Date(download.completedAt).toLocaleString() : '—'}</td>
                    <td>{download.errorMessage ?? '—'}</td>
                  </tr>
                ))}
              </tbody>
            </Table>
          )}
        </CardBody>
      </Card>
    </div>
  );
};
