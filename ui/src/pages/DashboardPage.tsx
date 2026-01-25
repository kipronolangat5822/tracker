import { useState, useEffect } from 'react';
import { type InventoryItem, ItemStatus } from '../types/inventory';
import { type Notification, NotificationSeverity } from '../types/notification';
import { inventoryApi } from '../services/inventoryService';
import { notificationApi } from '../services/notificationService';
import './DashboardPage.css';

export default function DashboardPage() {
  const [inventory, setInventory] = useState<InventoryItem[]>([]);
  const [notifications, setNotifications] = useState<Notification[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    fetchDashboardData();
  }, []);

  const fetchDashboardData = async () => {
    try {
      setLoading(true);
      const [inventoryData, notificationData] = await Promise.all([
        inventoryApi.getAll({ pageSize: 100 }),
        notificationApi.getAll({ pageSize: 10 }, true) 
      ]);
      setInventory(inventoryData.items);
      setNotifications(notificationData.items);
      setError(null);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to load dashboard data');
    } finally {
      setLoading(false);
    }
  };

  const totalItems = inventory.length;
  const freshItems = inventory.filter(i => i.status === ItemStatus.FRESH).length;
  const expiringSoon = inventory.filter(i => i.status === ItemStatus.EXPIRING_SOON).length;
  const expired = inventory.filter(i => i.status === ItemStatus.EXPIRED).length;
  const unreadNotifications = notifications.length;

  const criticalNotifications = notifications.filter(
    n => n.severity === NotificationSeverity.CRITICAL || n.severity === NotificationSeverity.ERROR
  ).length;

  const categoryCounts = inventory.reduce((acc, item) => {
    acc[item.category] = (acc[item.category] || 0) + 1;
    return acc;
  }, {} as Record<string, number>);

  const topCategories = Object.entries(categoryCounts)
    .sort((a, b) => b[1] - a[1])
    .slice(0, 5);

  const recentNotifications = notifications.slice(0, 5);

  if (loading) return <div className="loading">Loading dashboard...</div>;

  return (
    <div className="dashboard-page">
      <div className="dashboard-header">
        <h1>Dashboard</h1>
        <button onClick={fetchDashboardData} className="btn-refresh">
          🔄 Refresh
        </button>
      </div>

      {error && (
        <div className="error-message">
          {error}
          <button onClick={() => setError(null)}>×</button>
        </div>
      )}

      <div className="stats-grid">
        <div className="stat-card total">
            <div className="stat-content">
            <div className="stat-value">{totalItems}</div>
            <div className="stat-label">Total Items</div>
          </div>
        </div>

        <div className="stat-card fresh">
          <div className="stat-content">
            <div className="stat-value">{freshItems}</div>
            <div className="stat-label">Fresh Items</div>
          </div>
        </div>

        <div className="stat-card expiring">
          <div className="stat-content">
            <div className="stat-value">{expiringSoon}</div>
            <div className="stat-label">Expiring Soon</div>
          </div>
        </div>

        <div className="stat-card expired">
          <div className="stat-content">
            <div className="stat-value">{expired}</div>
            <div className="stat-label">Expired</div>
          </div>
        </div>

        <div className="stat-card notifications">
          <div className="stat-content">
            <div className="stat-value">{unreadNotifications}</div>
            <div className="stat-label">Unread Alerts</div>
          </div>
        </div>

        <div className="stat-card critical">
          <div className="stat-content">
            <div className="stat-value">{criticalNotifications}</div>
            <div className="stat-label">Critical Alerts</div>
          </div>
        </div>
      </div>

      <div className="dashboard-grid">
        <div className="dashboard-card">
          <h2>Top Categories</h2>
          {topCategories.length === 0 ? (
            <p className="empty-message">No items in inventory</p>
          ) : (
            <div className="category-list">
              {topCategories.map(([category, count]) => (
                <div key={category} className="category-item">
                  <span className="category-name">{category}</span>
                  <div className="category-bar-container">
                    <div
                      className="category-bar"
                      style={{ width: `${(count / totalItems) * 100}%` }}
                    />
                  </div>
                  <span className="category-count">{count}</span>
                </div>
              ))}
            </div>
          )}
        </div>

        <div className="dashboard-card">
          <h2>Recent Notifications</h2>
          {recentNotifications.length === 0 ? (
            <p className="empty-message">No unread notifications</p>
          ) : (
            <div className="notification-list">
              {recentNotifications.map((notification) => (
                <div key={notification.id} className="notification-item">
                  <div className="notification-icon-small">
                    {notification.severity === NotificationSeverity.CRITICAL
                      ? '🚨'
                      : notification.severity === NotificationSeverity.ERROR
                      ? '❌'
                      : notification.severity === NotificationSeverity.WARNING
                      ? '⚠️'
                      : 'ℹ️'}
                  </div>
                  <div className="notification-text">
                    <div className="notification-title-small">{notification.title}</div>
                    <div className="notification-message-small">{notification.message}</div>
                  </div>
                  <div className="notification-time-small">{notification.timeAgo}</div>
                </div>
              ))}
            </div>
          )}
        </div>

        <div className="dashboard-card">
          <h2>Inventory Status</h2>
          <div className="status-chart">
            <div className="status-item">
              <div className="status-label">
                <span className="status-dot fresh-dot"></span>
                Fresh
              </div>
              <div className="status-value">{freshItems}</div>
              <div className="status-percentage">
                {totalItems > 0 ? ((freshItems / totalItems) * 100).toFixed(1) : 0}%
              </div>
            </div>
            <div className="status-item">
              <div className="status-label">
                <span className="status-dot expiring-dot"></span>
                Expiring Soon
              </div>
              <div className="status-value">{expiringSoon}</div>
              <div className="status-percentage">
                {totalItems > 0 ? ((expiringSoon / totalItems) * 100).toFixed(1) : 0}%
              </div>
            </div>
            <div className="status-item">
              <div className="status-label">
                <span className="status-dot expired-dot"></span>
                Expired
              </div>
              <div className="status-value">{expired}</div>
              <div className="status-percentage">
                {totalItems > 0 ? ((expired / totalItems) * 100).toFixed(1) : 0}%
              </div>
            </div>
          </div>
        </div>

        <div className="dashboard-card quick-actions">
          <h2>Quick Actions</h2>
          <div className="action-buttons">
            <button className="action-btn" onClick={() => window.location.href = '/inventory'}>
              📦 Manage Inventory
            </button>
            <button className="action-btn" onClick={() => window.location.href = '/notifications'}>
              🔔 View Notifications
            </button>
            <button className="action-btn" onClick={async () => {
              await notificationApi.generateNotifications();
              await fetchDashboardData();
            }}>
              🔄 Check Expiry
            </button>
          </div>
        </div>
      </div>
    </div>
  );
}
