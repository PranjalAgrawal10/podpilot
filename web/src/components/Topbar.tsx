import { useState } from 'react';
import {
  Button,
  Navbar,
  NavbarBrand,
  Dropdown,
  DropdownToggle,
  DropdownMenu,
  DropdownItem,
} from 'reactstrap';
import { useAuth } from '../contexts/AuthContext';
import { useTheme } from '../contexts/ThemeContext';
import { useNavigate } from 'react-router-dom';
import { OrganizationSwitcher } from './organizations/OrganizationSwitcher';
import { Avatar } from './common/Avatar';

export const Topbar = () => {
  const { user, logout } = useAuth();
  const { theme, toggleTheme } = useTheme();
  const navigate = useNavigate();
  const [userMenuOpen, setUserMenuOpen] = useState(false);

  const handleLogout = async () => {
    await logout();
    navigate('/login');
  };

  const fullName = user ? `${user.firstName} ${user.lastName}` : '';

  return (
    <Navbar className="topbar px-4" expand="md">
      <NavbarBrand className="topbar-title">PodPilot</NavbarBrand>
      <div className="topbar-actions d-flex align-items-center gap-3">
        <OrganizationSwitcher />
        <Button color="link" className="theme-toggle" onClick={toggleTheme}>
          {theme === 'light' ? '🌙' : '☀️'}
        </Button>
        {user && (
          <Dropdown isOpen={userMenuOpen} toggle={() => setUserMenuOpen(!userMenuOpen)}>
            <DropdownToggle tag="button" className="user-menu-toggle btn btn-link">
              <Avatar name={fullName} size="sm" />
              <span className="user-greeting ms-2">{user.firstName}</span>
            </DropdownToggle>
            <DropdownMenu end>
              <DropdownItem onClick={() => navigate('/profile')}>Profile</DropdownItem>
              <DropdownItem divider />
              <DropdownItem onClick={() => void handleLogout()}>Logout</DropdownItem>
            </DropdownMenu>
          </Dropdown>
        )}
      </div>
    </Navbar>
  );
};
