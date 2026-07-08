import { Link } from 'react-router-dom';
import { Badge, Button, Card, CardBody, CardTitle } from 'reactstrap';
import type { AiModel } from '../../types';
import { formatBytes } from '../../utils/formatBytes';
import { DefaultBadge } from './DefaultBadge';

interface ModelCardProps {
  model: AiModel;
  canManage: boolean;
  canDelete: boolean;
  onSetDefault?: (model: AiModel) => void;
  onDelete?: (model: AiModel) => void;
}

export const ModelCard = ({
  model,
  canManage,
  canDelete,
  onSetDefault,
  onDelete,
}: ModelCardProps) => (
  <Card className="h-100">
    <CardBody>
      <div className="d-flex justify-content-between align-items-start mb-2">
        <CardTitle tag="h5" className="mb-0">
          <Link to={`/models/${model.id}`}>{model.fullName}</Link>
          {model.isDefault && <DefaultBadge />}
        </CardTitle>
        <Badge color={model.status === 'Available' ? 'success' : 'warning'}>{model.status}</Badge>
      </div>
      <p className="text-muted mb-2">{model.podName}</p>
      <div className="small text-muted mb-3">
        <div>{formatBytes(model.size)} · {model.parameters ?? '—'} · {model.quantization ?? '—'}</div>
        {model.family && <div>Family: {model.family}</div>}
      </div>
      <div className="d-flex gap-2 flex-wrap">
        {canManage && model.status === 'Available' && !model.isDefault && onSetDefault && (
          <Button size="sm" color="primary" outline onClick={() => onSetDefault(model)}>
            Set Default
          </Button>
        )}
        {canDelete && model.status !== 'Downloading' && onDelete && (
          <Button size="sm" color="danger" outline onClick={() => onDelete(model)}>
            Delete
          </Button>
        )}
      </div>
    </CardBody>
  </Card>
);
