export enum NotificationType {
  EXPIRY_WARNING = 0,
  EXPIRY_URGENT = 1,
  EXPIRED = 2,
  LOW_STOCK = 3,
  GENERAL = 4
}

export enum NotificationSeverity {
  INFO = 0,
  WARNING = 1,
  ERROR = 2,
  CRITICAL = 3
}

export type Notification = {
  id: string;
  title: string;
  message: string;
  type: NotificationType;
  severity: NotificationSeverity;
  createdDate: string;
  isRead: boolean;
  relatedItemId?: string;
  relatedItemType: string;
  timeAgo: string;
}
