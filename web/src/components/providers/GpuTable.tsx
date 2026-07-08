import { Table, Badge } from 'reactstrap';
import type { ProviderGpu } from '../../types';

interface GpuTableProps {
  gpus: ProviderGpu[];
  compact?: boolean;
}

export const GpuTable = ({ gpus, compact = false }: GpuTableProps) => {
  if (gpus.length === 0) {
    return <p className="text-muted mb-0">No GPU types available.</p>;
  }

  return (
    <Table responsive hover size={compact ? 'sm' : undefined} className="gpu-table mb-0">
      <thead>
        <tr>
          <th>GPU Type</th>
          <th>Name</th>
          <th>VRAM</th>
          <th>Price/hr</th>
          <th>Status</th>
        </tr>
      </thead>
      <tbody>
        {gpus.map((gpu) => (
          <tr key={gpu.gpuId}>
            <td>
              <Badge color="primary" className="gpu-type-badge">
                {gpu.gpuType}
              </Badge>
            </td>
            <td>{gpu.name}</td>
            <td>{gpu.memoryGb != null ? `${gpu.memoryGb} GB` : '—'}</td>
            <td>—</td>
            <td>
              <Badge color={gpu.isAvailable ? 'success' : 'secondary'}>
                {gpu.isAvailable ? 'Available' : 'Unavailable'}
              </Badge>
            </td>
          </tr>
        ))}
      </tbody>
    </Table>
  );
};
