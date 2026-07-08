import { useState } from 'react';
import { Table, Button, Input, Spinner } from 'reactstrap';
import { RoleBadge } from '../common/RoleBadge';
import { Avatar } from '../common/Avatar';
import { PERMISSIONS, type Member, type OrganizationRole } from '../../types';
import { useOrganization } from '../../contexts/OrganizationContext';
import { useAuth } from '../../contexts/AuthContext';

interface MemberTableProps {
  members: Member[];
  isLoading?: boolean;
  onUpdateRole: (memberId: string, role: string) => Promise<void>;
  onRemove: (memberId: string) => Promise<void>;
}

const assignableRoles: OrganizationRole[] = ['Admin', 'Developer', 'Viewer'];

export const MemberTable = ({
  members,
  isLoading = false,
  onUpdateRole,
  onRemove,
}: MemberTableProps) => {
  const { hasPermission } = useOrganization();
  const { user } = useAuth();
  const [updatingId, setUpdatingId] = useState<string | null>(null);
  const [removingId, setRemovingId] = useState<string | null>(null);

  const canManage = hasPermission(PERMISSIONS.MemberManage);
  const canUpdateRole = hasPermission(PERMISSIONS.MemberRoleUpdate);

  const handleRoleChange = async (memberId: string, role: string) => {
    setUpdatingId(memberId);
    try {
      await onUpdateRole(memberId, role);
    } finally {
      setUpdatingId(null);
    }
  };

  const handleRemove = async (memberId: string) => {
    setRemovingId(memberId);
    try {
      await onRemove(memberId);
    } finally {
      setRemovingId(null);
    }
  };

  if (isLoading) {
    return (
      <div className="text-center py-4">
        <Spinner />
      </div>
    );
  }

  if (members.length === 0) {
    return <p className="text-muted mb-0">No members found.</p>;
  }

  return (
    <Table responsive hover className="member-table">
      <thead>
        <tr>
          <th>Member</th>
          <th>Email</th>
          <th>Role</th>
          <th>Status</th>
          <th>Joined</th>
          {(canManage || canUpdateRole) && <th>Actions</th>}
        </tr>
      </thead>
      <tbody>
        {members.map((member) => {
          const fullName = `${member.firstName} ${member.lastName}`;
          const isSelf = user?.id === member.userId;
          const isOwner = member.role === 'Owner';

          return (
            <tr key={member.id}>
              <td>
                <div className="d-flex align-items-center gap-2">
                  <Avatar name={fullName} size="sm" />
                  <span>{fullName}{isSelf ? ' (you)' : ''}</span>
                </div>
              </td>
              <td>{member.email}</td>
              <td>
                {canUpdateRole && !isOwner && !isSelf ? (
                  <Input
                    type="select"
                    bsSize="sm"
                    value={member.role}
                    disabled={updatingId === member.id}
                    onChange={(e) => void handleRoleChange(member.id, e.target.value)}
                  >
                    {assignableRoles.map((role) => (
                      <option key={role} value={role}>
                        {role}
                      </option>
                    ))}
                    {member.role === 'Owner' && <option value="Owner">Owner</option>}
                  </Input>
                ) : (
                  <RoleBadge role={member.role} />
                )}
              </td>
              <td>{member.status}</td>
              <td>{new Date(member.joinedAt).toLocaleDateString()}</td>
              {(canManage || canUpdateRole) && (
                <td>
                  {canManage && !isOwner && !isSelf && (
                    <Button
                      color="outline-danger"
                      size="sm"
                      disabled={removingId === member.id}
                      onClick={() => void handleRemove(member.id)}
                    >
                      {removingId === member.id ? 'Removing...' : 'Remove'}
                    </Button>
                  )}
                </td>
              )}
            </tr>
          );
        })}
      </tbody>
    </Table>
  );
};
