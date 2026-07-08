import { Button, Modal, ModalBody, ModalFooter, ModalHeader } from 'reactstrap';
import type { AiModel } from '../../types';
import { DefaultBadge } from './DefaultBadge';

interface DeleteConfirmationProps {
  model: AiModel | null;
  isOpen: boolean;
  onConfirm: (forceDefault: boolean) => void;
  onCancel: () => void;
  isPending?: boolean;
}

export const DeleteConfirmation = ({
  model,
  isOpen,
  onConfirm,
  onCancel,
  isPending,
}: DeleteConfirmationProps) => (
  <Modal isOpen={isOpen} toggle={onCancel}>
    <ModalHeader toggle={onCancel}>Delete Model</ModalHeader>
    <ModalBody>
      {model && (
        <>
          <p>
            Delete <strong>{model.fullName}</strong> from pod <strong>{model.podName}</strong>?
          </p>
          {model.isDefault && (
            <p className="text-warning mb-0">
              This is the default model <DefaultBadge />. Confirming will remove it from Ollama and
              clear the default flag.
            </p>
          )}
        </>
      )}
    </ModalBody>
    <ModalFooter>
      <Button color="secondary" onClick={onCancel} disabled={isPending}>
        Cancel
      </Button>
      <Button
        color="danger"
        onClick={() => onConfirm(model?.isDefault ?? false)}
        disabled={isPending}
      >
        Delete
      </Button>
    </ModalFooter>
  </Modal>
);
