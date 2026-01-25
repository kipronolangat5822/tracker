import { Routes, Route, Navigate } from 'react-router-dom'
import './App.css'
import Sidebar from './components/Sidebar'
import DashboardPage from './pages/DashboardPage'
import InventoryPage from './pages/InventoryPage'
import NotificationsPage from './pages/NotificationsPage'

function App() {
  return (
    <div className="app">
      <Sidebar />
      <main className="main-content">
        <Routes>
          <Route path="/" element={<Navigate to="/dashboard" replace />} />
          <Route path="/dashboard" element={<DashboardPage />} />
          <Route path="/inventory" element={<InventoryPage />} />
          <Route path="/notifications" element={<NotificationsPage />} />
          <Route path="/users" element={<div className="page-placeholder">Users - Coming Soon</div>} />
          <Route path="/transactions" element={<div className="page-placeholder">Transactions - Coming Soon</div>} />
          <Route path="/settings" element={<div className="page-placeholder">Settings - Coming Soon</div>} />
        </Routes>
      </main>
    </div>
  )
}

export default App
