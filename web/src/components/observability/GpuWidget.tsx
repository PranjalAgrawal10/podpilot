import { Card, CardBody, CardTitle, Progress } from 'reactstrap';
import { formatBytes } from '../../utils/formatBytes';

interface GpuWidgetProps {
  utilizationPercent: number;
  memoryUsedBytes?: number | null;
  memoryTotalBytes?: number | null;
  temperatureCelsius?: number | null;
  title?: string;
}

export const GpuWidget = ({
  utilizationPercent,
  memoryUsedBytes,
  memoryTotalBytes,
  temperatureCelsius,
  title = 'GPU',
}: GpuWidgetProps) => {
  const vramPercent =
    memoryUsedBytes != null && memoryTotalBytes != null && memoryTotalBytes > 0
      ? Math.round((memoryUsedBytes / memoryTotalBytes) * 100)
      : null;

  const utilColor = utilizationPercent >= 90 ? 'danger' : utilizationPercent >= 70 ? 'warning' : 'success';
  const vramColor = vramPercent != null && vramPercent >= 90 ? 'danger' : vramPercent != null && vramPercent >= 70 ? 'warning' : 'info';

  return (
    <Card className="h-100">
      <CardBody>
        <CardTitle tag="h6" className="text-muted mb-3">
          {title}
        </CardTitle>
        <div className="mb-3">
          <div className="d-flex justify-content-between mb-1">
            <small>Utilization</small>
            <small>{utilizationPercent.toFixed(1)}%</small>
          </div>
          <Progress value={utilizationPercent} color={utilColor} />
        </div>
        {vramPercent != null && (
          <div className="mb-3">
            <div className="d-flex justify-content-between mb-1">
              <small>VRAM</small>
              <small>
                {formatBytes(memoryUsedBytes ?? 0)} / {formatBytes(memoryTotalBytes ?? 0)}
              </small>
            </div>
            <Progress value={vramPercent} color={vramColor} />
          </div>
        )}
        {temperatureCelsius != null && (
          <small className="text-muted">Temperature: {temperatureCelsius.toFixed(1)}°C</small>
        )}
      </CardBody>
    </Card>
  );
};
