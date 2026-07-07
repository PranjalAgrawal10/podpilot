import { Button, Navbar, NavbarBrand } from 'reactstrap';
import { useAuth } from '../contexts/AuthContext';
import { useTheme } from '../contexts/ThemeContext';
import { useNavigate } from 'react-router-dom';

export const Topbar = () => {
  const { user, logout } = useAuth();
  const { theme, toggleTheme } = useTheme();
  const navigate = useNavigate();

  const handleLogout = async () => {
    await logout();
    navigate('/login');
  };

  return (
    <Navbar className="topbar px-4" expand="md">
      <NavbarBrand className="topbar-title">PodPilot</NavbarBrand>
      <div className="topbar-actions d-flex align-items-center gap-3">
        <Button color="link" className="theme-toggle" onClick={toggleTheme}>
          {theme === 'light' ? '🌙' : '☀️'}
        </Button>
        {user && (
          <span className="user-greeting">
            {user.firstName} {user.lastName}
          </span>
        )}
        <Button color="outline-secondary" size="sm" onClick={handleLogout}>
          Logout
        </Button>
      </div>
    </Navbar>
  );
};
