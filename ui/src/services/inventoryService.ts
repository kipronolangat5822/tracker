import type { InventoryItem, CreateInventoryItem } from '../types/inventory';
import type { PaginatedResult, QueryParameters } from '../types/pagination';

const API_BASE_URL = 'http://localhost:5294/api';

export const inventoryApi = {
  async getAll(params?: QueryParameters): Promise<PaginatedResult<InventoryItem>> {
    const queryString = new URLSearchParams({
      ...(params?.searchTerm && { searchTerm: params.searchTerm }),
      pageNumber: String(params?.pageNumber || 1),
      pageSize: String(params?.pageSize || 10),
    }).toString();
    
    const response = await fetch(`${API_BASE_URL}/inventories?${queryString}`);
    if (!response.ok) throw new Error('Failed to fetch inventory items');
    return response.json();
  },

  async getById(id: string): Promise<InventoryItem> {
    const response = await fetch(`${API_BASE_URL}/inventories/${id}`);
    if (!response.ok) throw new Error('Failed to fetch inventory item');
    return response.json();
  },

  async create(item: CreateInventoryItem): Promise<InventoryItem> {
    const response = await fetch(`${API_BASE_URL}/inventories`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(item),
    });
    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.Error || 'Failed to create inventory item');
    }
    return response.json();
  },

  async update(id: string, item: CreateInventoryItem): Promise<InventoryItem> {
    const response = await fetch(`${API_BASE_URL}/inventories/${id}`, {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(item),
    });
    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.Error || 'Failed to update inventory item');
    }
    return response.json();
  },

  async delete(id: string): Promise<void> {
    const response = await fetch(`${API_BASE_URL}/inventories/${id}`, {
      method: 'DELETE',
    });
    if (!response.ok) throw new Error('Failed to delete inventory item');
  },
};
