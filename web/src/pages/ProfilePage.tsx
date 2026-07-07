import { Card, CardBody, CardTitle, ListGroup, ListGroupItem, Badge } from 'reactstrap';
import { useAuth } from '../contexts/AuthContext';

export const ProfilePage = () => {
  const { user } = useAuth();

  if (!user) {
    return null;
  }

  return (
    <div>
      <h1 className="page-title">Profile</h1>

      <Card className="mb-4">
        <CardBody>
          <CardTitle tag="h5">Account Information</CardTitle>
          <ListGroup flush>
            <ListGroupItem className="d-flex justify-content-between">
              <span>Name</span>
              <strong>{user.firstName} {user.lastName}</strong>
            </ListGroupItem>
            <ListGroupItem className="d-flex justify-content-between">
              <span>Email</span>
              <strong>{user.email}</strong>
            </ListGroupItem>
            <ListGroupItem className="d-flex justify-content-between">
              <span>Roles</span>
              <span>
                {user.roles.map((role) => (
                  <Badge key={role} color="info" className="ms-1">
                    {role}
                  </Badge>
                ))}
              </span>
            </ListGroupItem>
          </ListGroup>
        </CardBody>
      </Card>

      <Card>
        <CardBody>
          <CardTitle tag="h5">Organizations</CardTitle>
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
                  <Badge color="secondary">{org.role}</Badge>
                </ListGroupItem>
              ))}
            </ListGroup>
          )}
        </CardBody>
      </Card>
    </div>
  );
};
