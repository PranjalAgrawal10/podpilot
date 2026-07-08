import { Table } from 'reactstrap';
import type { ProviderTemplate } from '../../types';

interface TemplateSelectorProps {
  templates: ProviderTemplate[];
  selectedId?: string | null;
  onSelect?: (templateId: string) => void;
  compact?: boolean;
}

export const TemplateSelector = ({
  templates,
  selectedId,
  onSelect,
  compact = false,
}: TemplateSelectorProps) => {
  if (templates.length === 0) {
    return <p className="text-muted mb-0">No templates available.</p>;
  }

  return (
    <Table responsive hover size={compact ? 'sm' : undefined} className="template-table mb-0">
      <thead>
        <tr>
          {onSelect && <th style={{ width: 40 }} />}
          <th>Template</th>
          <th>Image</th>
        </tr>
      </thead>
      <tbody>
        {templates.map((template) => (
          <tr
            key={template.templateId}
            className={onSelect ? 'template-row-selectable' : undefined}
            onClick={onSelect ? () => onSelect(template.templateId) : undefined}
          >
            {onSelect && (
              <td>
                <input
                  type="radio"
                  name="template"
                  checked={selectedId === template.templateId}
                  onChange={() => onSelect(template.templateId)}
                  onClick={(e) => e.stopPropagation()}
                />
              </td>
            )}
            <td>
              <div className="fw-semibold">{template.name}</div>
              {template.description && (
                <small className="text-muted">{template.description}</small>
              )}
            </td>
            <td>
              <code className="template-image-name">{template.imageName ?? '—'}</code>
            </td>
          </tr>
        ))}
      </tbody>
    </Table>
  );
};
