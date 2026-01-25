import { useState, useEffect } from 'react';
import { useSearchParams } from 'react-router-dom';
import { type Notification, NotificationType, NotificationSeverity } from '../types/notification';
import { notificationApi } from '../services/notificationService';
import type { PaginatedResult } from '../types/pagination';
import { useDebounce } from '../hooks/useDebounce';
import './NotificationsPage.css';

export default function NotificationsPage() {
  const [searchParams, setSearchParams] = useSearchParams();
  
  const [paginatedData, setPaginatedData] = useState<PaginatedResult<Notification>>({
    items: [],
    totalCount: 0,
    pageNumber: 1,
    pageSize: 10,
    totalPages: 0,
    hasPreviousPage: false,
    hasNextPage: false,
  });
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [showUnreadOnly, setShowUnreadOnly] = useState(searchParams.get('unread') === 'true');
  const [searchTerm, setSearchTerm] = useState(searchParams.get('search') || '');
  const [currentPage, setCurrentPage] = useState(parseInt(searchParams.get('page') || '1', 10));
  const [pageSize] = useState(10);
  
  // Debounce search for API calls only to prevent excessive requests
  const debouncedSearchTerm = useDebounce(searchTerm, 300);

  // Update URL when debounced search changes (prevents input focus loss)
  useEffect(() => {
    const params: Record<string, string> = {};
    if (debouncedSearchTerm) params.search = debouncedSearchTerm;
    if (currentPage > 1) params.page = currentPage.toString();
    if (showUnreadOnly) params.unread = 'true';
    
    setSearchParams(params, { replace: true });
  }, [debouncedSearchTerm, currentPage, showUnreadOnly, setSearchParams]);

  // Fetch data with debounced search term
  useEffect(() => {
    fetchNotifications();
    // Auto-refresh every 30 seconds
    const interval = setInterval(fetchNotifications, 30000);
    return () => clearInterval(interval);
  }, [showUnreadOnly, debouncedSearchTerm, currentPage]);

  const fetchNotifications = async () => {
    try {
      setLoading(true);
      const data = await notificationApi.getAll({
        searchTerm: debouncedSearchTerm || undefined,
        pageNumber: currentPage,
        pageSize: pageSize,
      }, showUnreadOnly);
      setPaginatedData(data);
      setError(null);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to load notifications');
    } finally {
      setLoading(false);
    }
  };

  const handleGenerate = async () => {
    try {
      await notificationApi.generateNotifications();
      await fetchNotifications();
      setError(null);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to generate notifications');
    }
  };

  const handleMarkAsRead = async (id: string) => {
    try {
      await notificationApi.markAsRead(id);
      await fetchNotifications();
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to mark as read');
    }
  };

  const handleMarkAllAsRead = async () => {
    try {
      await notificationApi.markAllAsRead();
      await fetchNotifications();
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to mark all as read');
    }
  };

  const handleDelete = async (id: string) => {
    if (!confirm('Are you sure you want to delete this notification?')) return;
    try {
      await notificationApi.delete(id);
      await fetchNotifications();
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to delete notification');
    }
  };

  const getTypeIcon = (type: NotificationType) => {
    const icons = {
      [NotificationType.EXPIRY_WARNING]: '⚠️',
      [NotificationType.EXPIRY_URGENT]: '🚨',
      [NotificationType.EXPIRED]: '❌',
      [NotificationType.LOW_STOCK]: '📉',
      [NotificationType.GENERAL]: 'ℹ️',
    };
    return icons[type];
  };

  const getSeverityClass = (severity: NotificationSeverity) => {
    const classes = {
      [NotificationSeverity.INFO]: 'severity-info',
      [NotificationSeverity.WARNING]: 'severity-warning',
      [NotificationSeverity.ERROR]: 'severity-error',
      [NotificationSeverity.CRITICAL]: 'severity-critical',
    };
    return classes[severity];
  };

  const unreadCount = paginatedData.items.filter(n => !n.isRead).length;

  if (loading) return <div className="loading">Loading notifications...</div>;

  return (
    <div className="notifications-page">
      <div className="notifications-header">
        <div>
          <h1>Notifications</h1>
          {unreadCount > 0 && (
            <span className="unread-badge">{unreadCount} unread</span>
          )}
        </div>
        <div className="header-actions">
          <button onClick={handleGenerate} className="btn-secondary">
            🔄 Check Inventory
          </button>
          {unreadCount > 0 && (
            <button onClick={handleMarkAllAsRead} className="btn-secondary">
              ✓ Mark All Read
            </button>
          )}
        </div>
      </div>

      {error && (
        <div className="error-message">
          {error}
          <button onClick={() => setError(null)}>×</button>
        </div>
      )}

      <div className="search-filter-container">
        <div className="search-box">
          <input
            type="text"
            placeholder="Search notifications..."
            value={searchTerm}
            onChange={(e) => {
              setSearchTerm(e.target.value);
              setCurrentPage(1); // Reset to first page on new search
            }}
            className="search-input"
          />
        </div>
        <div className="filter-controls">
          <label className="checkbox-label">
            <input
              type="checkbox"
              checked={showUnreadOnly}
              onChange={(e) => {
                setShowUnreadOnly(e.target.checked);
                setCurrentPage(1); // Reset to first page on filter change
              }}
            />
            Show unread only
          </label>
          <span className="pagination-info">
            Showing {paginatedData.items.length} of {paginatedData.totalCount} notifications
          </span>
        </div>
      </div>

      <div className="notifications-list">
        {paginatedData.items.length === 0 ? (
          <div className="empty-state">
            <p>
              {searchTerm 
                ? 'No notifications match your search.'
                : showUnreadOnly 
                ? 'No unread notifications. Great job!' 
                : 'No notifications yet. Click "Check Inventory" to scan for expiring items.'}
            </p>
          </div>
        ) : (
          paginatedData.items.map((notification) => (
            <div
              key={notification.id}
              className={`notification-card ${getSeverityClass(notification.severity)} ${
                notification.isRead ? 'read' : 'unread'
              }`}
            >
              <div className="notification-icon">
                {getTypeIcon(notification.type)}
              </div>
              <div className="notification-content">
                <div className="notification-header-content">
                  <h3>{notification.title}</h3>
                  <span className="notification-time">{notification.timeAgo}</span>
                </div>
                <p>{notification.message}</p>
              </div>
              <div className="notification-actions">
                {!notification.isRead && (
                  <button
                    onClick={() => handleMarkAsRead(notification.id)}
                    className="btn-action"
                    title="Mark as read"
                  >
                    ✓
                  </button>
                )}
                <button
                  onClick={() => handleDelete(notification.id)}
                  className="btn-action btn-delete-action"
                  title="Delete"
                >
                  🗑️
                </button>
              </div>
            </div>
          ))
        )}
      </div>

      {paginatedData.totalPages > 1 && (
        <div className="pagination-controls">
          <button
            onClick={() => setCurrentPage(prev => prev - 1)}
            disabled={!paginatedData.hasPreviousPage}
            className="pagination-btn"
          >
            ← Previous
          </button>
          
          <div className="pagination-pages">
            {Array.from({ length: paginatedData.totalPages }, (_, i) => i + 1).map(page => (
              <button
                key={page}
                onClick={() => setCurrentPage(page)}
                className={`pagination-page ${page === currentPage ? 'active' : ''}`}
              >
                {page}
              </button>
            ))}
          </div>
          
          <button
            onClick={() => setCurrentPage(prev => prev + 1)}
            disabled={!paginatedData.hasNextPage}
            className="pagination-btn"
          >
            Next →
          </button>
        </div>
      )}
    </div>
  );
}
