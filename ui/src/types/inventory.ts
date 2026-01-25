export const ItemStatus = {
  FRESH: 0,
  EXPIRING_SOON: 1,
  EXPIRED: 2
} as const;

export type ItemStatus = typeof ItemStatus[keyof typeof ItemStatus];

export interface InventoryItem {
  id: string;
  name: string;
  category: string;
  quantity: number;
  unit: string;
  location: string;
  expiryDate?: string;
  status: ItemStatus;
  daysUntilExpiry?: number;
}

export interface CreateInventoryItem {
  name: string;
  category: string;
  quantity: number;
  unit: string;
  location: string;
  expiryDate?: string;
  purchaseDate: string;
}
