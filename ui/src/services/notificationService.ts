import { type Notification } from '../types/notification';
import type { PaginatedResult, QueryParameters } from '../types/pagination';

const API_BASE_URL = 'http://localhost:5294/api';

export const notificationApi = {
  async getAll(params?: QueryParameters, unreadOnly: boolean = false): Promise<PaginatedResult<Notification>> {
    const queryString = new URLSearchParams({
      unreadOnly: String(unreadOnly),
      ...(params?.searchTerm && { searchTerm: params.searchTerm }),
      pageNumber: String(params?.pageNumber || 1),
      pageSize: String(params?.pageSize || 10),
    }).toString();
    
    const response = await fetch(`${API_BASE_URL}/notifications?${queryString}`);
    if (!response.ok) throw new Error('Failed to fetch notifications');
    return response.json();
  },

  async getUnreadCount(): Promise<number> {
    const response = await fetch(`${API_BASE_URL}/notifications/count`);
    if (!response.ok) throw new Error('Failed to fetch unread count');
    const data = await response.json();
    return data.count;
  },

  async generateNotifications(): Promise<void> {
    const response = await fetch(`${API_BASE_URL}/notifications/generate`, {
      method: 'POST',
    });
    if (!response.ok) throw new Error('Failed to generate notifications');
  },

  async markAsRead(id: string): Promise<Notification> {
    const response = await fetch(`${API_BASE_URL}/notifications/${id}/read`, {
      method: 'PATCH',
    });
    if (!response.ok) throw new Error('Failed to mark as read');
    return response.json();
  },

  async markAllAsRead(): Promise<void> {
    const response = await fetch(`${API_BASE_URL}/notifications/mark-all-read`, {
      method: 'POST',
    });
    if (!response.ok) throw new Error('Failed to mark all as read');
  },

  async delete(id: string): Promise<void> {
    const response = await fetch(`${API_BASE_URL}/notifications/${id}`, {
      method: 'DELETE',
    });
    if (!response.ok) throw new Error('Failed to delete notification');
  },
};
