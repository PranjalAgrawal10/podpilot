import { Outlet } from 'react-router-dom';
import { Sidebar } from '../components/Sidebar';
import { Topbar } from '../components/Topbar';

export const MainLayout = () => (
  <div className="app-layout">
    <Sidebar />
    <div className="main-content">
      <Topbar />
      <main className="page-content">
        <Outlet />
      </main>
    </div>
  </div>
);
