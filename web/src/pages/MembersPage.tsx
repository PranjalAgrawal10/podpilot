import { useState } from 'react';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { Button, Alert, Spinner } from 'reactstrap';
import { toast } from 'react-toastify';
import { memberService } from '../services/memberService';
import { invitationService } from '../services/invitationService';
import { MemberTable } from '../components/members/MemberTable';
import { InvitationModal } from '../components/invitations/InvitationModal';
import { useOrganization } from '../contexts/OrganizationContext';
import { PERMISSIONS, type InviteMemberRequest } from '../types';

export const MembersPage = () => {
  const { currentOrganization, hasPermission } = useOrganization();
  const queryClient = useQueryClient();
  const [inviteModalOpen, setInviteModalOpen] = useState(false);

  const canInvite = hasPermission(PERMISSIONS.InvitationCreate);
  const canRead = hasPermission(PERMISSIONS.MemberRead);

  const { data: members = [], isLoading, error } = useQuery({
    queryKey: ['members', currentOrganization?.id],
    queryFn: () => memberService.list(currentOrganization!.id),
    enabled: !!currentOrganization?.id && canRead,
  });

  const handleInvite = async (data: InviteMemberRequest) => {
    if (!currentOrganization) return;
    await invitationService.invite(currentOrganization.id, data);
    toast.success('Invitation sent');
    await queryClient.invalidateQueries({ queryKey: ['members', currentOrganization.id] });
  };

  const handleUpdateRole = async (memberId: string, role: string) => {
    if (!currentOrganization) return;
    await memberService.updateRole(currentOrganization.id, memberId, { role });
    toast.success('Role updated');
    await queryClient.invalidateQueries({ queryKey: ['members', currentOrganization.id] });
  };

  const handleRemove = async (memberId: string) => {
    if (!currentOrganization) return;
    await memberService.remove(currentOrganization.id, memberId);
    toast.success('Member removed');
    await queryClient.invalidateQueries({ queryKey: ['members', currentOrganization.id] });
  };

  if (!currentOrganization) {
    return (
      <Alert color="info">
        Select an organization to view members, or create one from the Organizations page.
      </Alert>
    );
  }

  if (!canRead) {
    return (
      <Alert color="warning">
        You don&apos;t have permission to view members in this organization.
      </Alert>
    );
  }

  return (
    <div>
      <div className="d-flex justify-content-between align-items-center mb-4">
        <div>
          <h1 className="page-title mb-1">Members</h1>
          <p className="text-muted mb-0">{currentOrganization.name}</p>
        </div>
        {canInvite && (
          <Button color="primary" onClick={() => setInviteModalOpen(true)}>
            Invite Member
          </Button>
        )}
      </div>

      {error && (
        <Alert color="danger">
          {error instanceof Error ? error.message : 'Failed to load members'}
        </Alert>
      )}

      {isLoading ? (
        <div className="text-center py-5">
          <Spinner />
        </div>
      ) : (
        <MemberTable
          members={members}
          onUpdateRole={handleUpdateRole}
          onRemove={handleRemove}
        />
      )}

      <InvitationModal
        isOpen={inviteModalOpen}
        toggle={() => setInviteModalOpen(false)}
        onSubmit={handleInvite}
      />
    </div>
  );
};
