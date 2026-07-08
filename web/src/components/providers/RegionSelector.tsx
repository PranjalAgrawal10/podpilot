import { FormGroup, Label, Input } from 'reactstrap';
import type { ProviderRegion } from '../../types';

interface RegionSelectorProps {
  regions: ProviderRegion[];
  value?: string | null;
  onChange: (regionId: string) => void;
  disabled?: boolean;
  id?: string;
  label?: string;
  required?: boolean;
  invalid?: boolean;
}

export const RegionSelector = ({
  regions,
  value,
  onChange,
  disabled = false,
  id = 'defaultRegion',
  label = 'Default Region',
  required = false,
  invalid = false,
}: RegionSelectorProps) => {
  const availableRegions = regions.filter((region) => region.isAvailable);

  return (
    <FormGroup>
      <Label for={id}>{label}</Label>
      <Input
        id={id}
        type="select"
        value={value ?? ''}
        disabled={disabled || availableRegions.length === 0}
        invalid={invalid}
        onChange={(e) => onChange(e.target.value)}
      >
        <option value="">
          {availableRegions.length === 0 ? 'No regions available' : 'Select a region'}
        </option>
        {availableRegions.map((region) => (
          <option key={region.id} value={region.regionId}>
            {region.displayName ?? region.name}
          </option>
        ))}
      </Input>
      {required && !value && (
        <small className="text-muted">Select a default region after validation.</small>
      )}
    </FormGroup>
  );
};
