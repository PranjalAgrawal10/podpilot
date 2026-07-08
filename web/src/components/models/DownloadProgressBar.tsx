import { Progress } from 'reactstrap';

export const DownloadProgressBar = ({
  progress,
  label,
}: {
  progress: number;
  label?: string;
}) => (
  <div>
    {label && <small className="text-muted d-block mb-1">{label}</small>}
    <Progress value={progress} color={progress >= 100 ? 'success' : 'info'}>
      {progress}%
    </Progress>
  </div>
);
