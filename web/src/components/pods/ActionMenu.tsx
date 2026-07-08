import { useState } from 'react';
import {
  Dropdown,
  DropdownToggle,
  DropdownMenu,
  DropdownItem,
  Modal,
  ModalHeader,
  ModalBody,
  ModalFooter,
  Button,
} from 'reactstrap';
import type { Pod } from '../../types';

interface ActionMenuProps {
  pod: Pod;
  canUpdate: boolean;
  canDelete: boolean;
  onStart: (pod: Pod) => void;
  onStop: (pod: Pod) => void;
  onRestart: (pod: Pod) => void;
  onDelete: (pod: Pod, force: boolean) => void;
  onSync: (pod: Pod) => void;
}

export const ActionMenu = ({
  pod,
  canUpdate,
  canDelete,
  onStart,
  onStop,
  onRestart,
  onDelete,
  onSync,
}: ActionMenuProps) => {
  const [open, setOpen] = useState(false);
  const [deleteOpen, setDeleteOpen] = useState(false);
  const isRunning = pod.status === 'Running' || pod.status === 'Starting';

  if (!canUpdate && !canDelete) {
    return null;
  }

  return (
    <>
      <Dropdown isOpen={open} toggle={() => setOpen(!open)} direction="down">
        <DropdownToggle caret color="light" size="sm">
          Actions
        </DropdownToggle>
        <DropdownMenu end>
          {canUpdate && (
            <>
              <DropdownItem onClick={() => onSync(pod)}>Sync Status</DropdownItem>
              <DropdownItem divider />
              <DropdownItem disabled={pod.status === 'Running'} onClick={() => onStart(pod)}>
                Start
              </DropdownItem>
              <DropdownItem disabled={pod.status === 'Stopped'} onClick={() => onStop(pod)}>
                Stop
              </DropdownItem>
              <DropdownItem onClick={() => onRestart(pod)}>Restart</DropdownItem>
            </>
          )}
          {canDelete && (
            <>
              <DropdownItem divider />
              <DropdownItem className="text-danger" onClick={() => setDeleteOpen(true)}>
                Delete
              </DropdownItem>
            </>
          )}
        </DropdownMenu>
      </Dropdown>

      <Modal isOpen={deleteOpen} toggle={() => setDeleteOpen(false)}>
        <ModalHeader toggle={() => setDeleteOpen(false)}>Delete Pod</ModalHeader>
        <ModalBody>
          {isRunning ? (
            <p>
              <strong>{pod.name}</strong> is currently running. Deleting it will terminate the
              workload and stop billing. Are you sure?
            </p>
          ) : (
            <p>
              Are you sure you want to delete <strong>{pod.name}</strong>?
            </p>
          )}
        </ModalBody>
        <ModalFooter>
          <Button color="secondary" onClick={() => setDeleteOpen(false)}>
            Cancel
          </Button>
          <Button color="danger" onClick={() => { onDelete(pod, isRunning); setDeleteOpen(false); }}>
            Delete
          </Button>
        </ModalFooter>
      </Modal>
    </>
  );
};
