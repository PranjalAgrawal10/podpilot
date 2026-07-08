import type { GpuType } from '../../types';

export const GpuBadge = ({ gpuType }: { gpuType: GpuType }) => (
  <span className="badge bg-primary-subtle text-primary-emphasis border">{gpuType}</span>
);
