export const CostBadge = ({ hourlyCost }: { hourlyCost?: number | null }) => {
  if (hourlyCost == null) {
    return <span className="text-muted">—</span>;
  }

  return (
    <span className="badge bg-success-subtle text-success-emphasis border">
      ${hourlyCost.toFixed(2)}/hr
    </span>
  );
};
