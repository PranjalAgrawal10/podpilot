import { FormGroup, Input, Label } from 'reactstrap';
import type { Pod } from '../../types';

interface ModelSelectorProps {
  pods: Pod[];
  value: string;
  onChange: (podId: string) => void;
  label?: string;
  required?: boolean;
}

export const ModelSelector = ({
  pods,
  value,
  onChange,
  label = 'GPU Pod',
  required = true,
}: ModelSelectorProps) => (
  <FormGroup>
    <Label for="podSelector">{label}</Label>
    <Input
      id="podSelector"
      type="select"
      value={value}
      onChange={(event) => onChange(event.target.value)}
      required={required}
    >
      <option value="">Select a pod</option>
      {pods.map((pod) => (
        <option key={pod.id} value={pod.id}>
          {pod.name} ({pod.status})
        </option>
      ))}
    </Input>
  </FormGroup>
);
