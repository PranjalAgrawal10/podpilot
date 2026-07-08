import { Link } from 'react-router-dom';
import { Card, CardBody, CardTitle } from 'reactstrap';
import type { Pod } from '../../types';
import { StatusBadge } from './StatusBadge';
import { GpuBadge } from './GpuBadge';
import { CostBadge } from './CostBadge';
import { RegionBadge } from './RegionBadge';
import { ActionMenu } from './ActionMenu';

interface PodCardProps {
  pod: Pod;
  canUpdate: boolean;
  canDelete: boolean;
  onStart: (pod: Pod) => void;
  onStop: (pod: Pod) => void;
  onRestart: (pod: Pod) => void;
  onDelete: (pod: Pod, force: boolean) => void;
  onSync: (pod: Pod) => void;
}

export const PodCard = ({
  pod,
  canUpdate,
  canDelete,
  onStart,
  onStop,
  onRestart,
  onDelete,
  onSync,
}: PodCardProps) => (
  <Card className="h-100 pod-card">
    <CardBody>
      <div className="d-flex justify-content-between align-items-start mb-2">
        <div>
          <CardTitle tag="h5" className="mb-1">
            <Link to={`/pods/${pod.id}`}>{pod.name}</Link>
          </CardTitle>
          <small className="text-muted">{pod.providerName}</small>
        </div>
        <StatusBadge status={pod.status} />
      </div>

      <div className="d-flex flex-wrap gap-2 mb-3">
        <GpuBadge gpuType={pod.gpuType} />
        <RegionBadge region={pod.region} />
        <CostBadge hourlyCost={pod.hourlyCost} />
      </div>

      <div className="small text-muted mb-3">
        <div>Provider: {pod.providerType}</div>
        <div>Last updated: {pod.lastSyncedAt ? new Date(pod.lastSyncedAt).toLocaleString() : '—'}</div>
      </div>

      <div className="d-flex gap-2 flex-wrap">
        {canUpdate && pod.status !== 'Running' && (
          <button type="button" className="btn btn-sm btn-success" onClick={() => onStart(pod)}>
            Start
          </button>
        )}
        {canUpdate && pod.status === 'Running' && (
          <button type="button" className="btn btn-sm btn-warning" onClick={() => onStop(pod)}>
            Stop
          </button>
        )}
        <ActionMenu
          pod={pod}
          canUpdate={canUpdate}
          canDelete={canDelete}
          onStart={onStart}
          onStop={onStop}
          onRestart={onRestart}
          onDelete={onDelete}
          onSync={onSync}
        />
      </div>
    </CardBody>
  </Card>
);
