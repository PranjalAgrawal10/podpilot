import { Card, CardBody, CardTitle, ListGroup, ListGroupItem, Badge } from 'reactstrap';
import { Link } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import { Avatar } from '../components/common/Avatar';
import { RoleBadge } from '../components/common/RoleBadge';

export const ProfilePage = () => {
  const { user } = useAuth();

  if (!user) {
    return null;
  }

  const fullName = `${user.firstName} ${user.lastName}`;

  return (
    <div>
      <h1 className="page-title">Profile</h1>

      <Card className="mb-4">
        <CardBody>
          <div className="d-flex align-items-center gap-3 mb-4">
            <Avatar name={fullName} size="lg" />
            <div>
              <h4 className="mb-1">{fullName}</h4>
              <p className="text-muted mb-0">{user.email}</p>
            </div>
          </div>

          <CardTitle tag="h5">Account Information</CardTitle>
          <ListGroup flush>
            <ListGroupItem className="d-flex justify-content-between">
              <span>Name</span>
              <strong>{fullName}</strong>
            </ListGroupItem>
            <ListGroupItem className="d-flex justify-content-between">
              <span>Email</span>
              <strong>{user.email}</strong>
            </ListGroupItem>
            <ListGroupItem className="d-flex justify-content-between">
              <span>System Roles</span>
              <span>
                {user.roles.map((role) => (
                  <Badge key={role} color="info" className="ms-1">
                    {role}
                  </Badge>
                ))}
              </span>
            </ListGroupItem>
            <ListGroupItem className="d-flex justify-content-between align-items-center">
              <span>API Keys</span>
              <Link to="/gateway" className="btn btn-sm btn-outline-primary">
                Manage on Gateway
              </Link>
            </ListGroupItem>
            <ListGroupItem className="d-flex justify-content-between align-items-center">
              <span>Billing</span>
              <Link to="/billing" className="btn btn-sm btn-outline-primary">
                Open billing
              </Link>
            </ListGroupItem>
          </ListGroup>
        </CardBody>
      </Card>

      <Card>
        <CardBody>
          <div className="d-flex justify-content-between align-items-center mb-3">
            <CardTitle tag="h5" className="mb-0">Organization Memberships</CardTitle>
            <Link to="/organizations" className="btn btn-sm btn-outline-primary">
              View All
            </Link>
          </div>
          {user.organizations.length === 0 ? (
            <p className="text-muted mb-0">No organizations found.</p>
          ) : (
            <ListGroup flush>
              {user.organizations.map((org) => (
                <ListGroupItem key={org.id} className="d-flex justify-content-between align-items-center">
                  <div>
                    <strong>{org.name}</strong>
                    <br />
                    <small className="text-muted">{org.slug}</small>
                  </div>
                  <RoleBadge role={org.role} />
                </ListGroupItem>
              ))}
            </ListGroup>
          )}
        </CardBody>
      </Card>
    </div>
  );
};
