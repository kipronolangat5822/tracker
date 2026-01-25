import { useState, useEffect } from 'react';
import { useSearchParams } from 'react-router-dom';
import { type InventoryItem, type CreateInventoryItem, ItemStatus } from '../types/inventory';
import { inventoryApi } from '../services/inventoryService';
import type { PaginatedResult } from '../types/pagination';
import { useDebounce } from '../hooks/useDebounce';
import './InventoryPage.css';

export default function InventoryPage() {
  const [searchParams, setSearchParams] = useSearchParams();

  const [paginatedData, setPaginatedData] = useState<PaginatedResult<InventoryItem>>({
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
  const [showAddForm, setShowAddForm] = useState(false);
  const [editingItem, setEditingItem] = useState<InventoryItem | null>(null);

  const [searchTerm, setSearchTerm] = useState(searchParams.get('search') || '');
  const [currentPage, setCurrentPage] = useState(
    parseInt(searchParams.get('page') || '1', 10)
  );

  const pageSize = 10;

  // 🔹 Debounce ONLY for API calls
  const debouncedSearchTerm = useDebounce(searchTerm, 300);

  const [formData, setFormData] = useState<CreateInventoryItem>({
    name: '',
    category: '',
    quantity: 0,
    unit: '',
    location: '',
    expiryDate: undefined,
    purchaseDate: new Date().toISOString().split('T')[0],
  });

  // 🔹 Fetch data when debounce settles or page changes
  useEffect(() => {
    fetchItems();
  }, [debouncedSearchTerm, currentPage]);

  // 🔹 Update URL AFTER debounce (no focus loss)
  useEffect(() => {
    const params: Record<string, string> = {};

    if (debouncedSearchTerm) params.search = debouncedSearchTerm;
    if (currentPage > 1) params.page = currentPage.toString();

    setSearchParams(params, { replace: true });
  }, [debouncedSearchTerm, currentPage, setSearchParams]);

  const fetchItems = async () => {
    try {
      setLoading(true);

      const data = await inventoryApi.getAll({
        searchTerm: debouncedSearchTerm || undefined,
        pageNumber: currentPage,
        pageSize,
      });

      setPaginatedData(data);
      setError(null);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to load inventory');
    } finally {
      setLoading(false);
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    try {
      if (editingItem) {
        await inventoryApi.update(editingItem.id, formData);
      } else {
        await inventoryApi.create(formData);
      }

      await fetchItems();
      resetForm();
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to save item');
    }
  };

  const handleEdit = (item: InventoryItem) => {
    setEditingItem(item);
    setFormData({
      name: item.name,
      category: item.category,
      quantity: item.quantity,
      unit: item.unit,
      location: item.location,
      expiryDate: item.expiryDate,
      purchaseDate: new Date().toISOString().split('T')[0],
    });
    setShowAddForm(true);
  };

  const handleDelete = async (id: string) => {
    if (!confirm('Are you sure you want to delete this item?')) return;

    try {
      await inventoryApi.delete(id);
      await fetchItems();
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to delete item');
    }
  };

  const resetForm = () => {
    setFormData({
      name: '',
      category: '',
      quantity: 0,
      unit: '',
      location: '',
      expiryDate: undefined,
      purchaseDate: new Date().toISOString().split('T')[0],
    });

    setShowAddForm(false);
    setEditingItem(null);
  };

  const getStatusBadge = (status: ItemStatus) => {
    const statusMap = {
      [ItemStatus.FRESH]: { label: 'Fresh', className: 'status-fresh' },
      [ItemStatus.EXPIRING_SOON]: { label: 'Expiring Soon', className: 'status-expiring' },
      [ItemStatus.EXPIRED]: { label: 'Expired', className: 'status-expired' },
    };

    const statusInfo = statusMap[status];
    return (
      <span className={`status-badge ${statusInfo.className}`}>
        {statusInfo.label}
      </span>
    );
  };

  if (loading) return <div className="loading">Loading inventory...</div>;

  return (
    <div className="inventory-page">
      {/* Header */}
      <div className="inventory-header">
        <h1>Inventory Management</h1>
        <button
          onClick={() => setShowAddForm(!showAddForm)}
          className="btn-primary"
        >
          {showAddForm ? 'Cancel' : '+ Add Item'}
        </button>
      </div>

      {/* Error */}
      {error && (
        <div className="error-message">
          {error}
          <button onClick={() => setError(null)}>×</button>
        </div>
      )}

      {/* Search */}
      <div className="search-pagination-container">
        <div className="search-box">
          <input
            type="text"
            placeholder="Search by name, category, or location..."
            value={searchTerm}
            onChange={(e) => {
              setSearchTerm(e.target.value);
              setCurrentPage(1);
            }}
            className="search-input"
          />
        </div>


        <div className="pagination-info">
          Showing {paginatedData.items.length} of {paginatedData.totalCount} items
        </div>
      </div>

      <div className="inventory-grid">
        {paginatedData.items.length === 0 ? (
          <div className="empty-state">
            <p>{searchTerm ? 'No items match your search.' : 'No inventory items found. Add your first item to get started!'}</p>
          </div>
        ) : (
          <table className="inventory-table">
            <thead>
              <tr>
                <th>Name</th>
                <th>Category</th>
                <th>Quantity</th>
                <th>Location</th>
                <th>Expiry Date</th>
                <th>Days Until Expiry</th>
                <th>Status</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              {paginatedData.items.map((item) => (
                <tr key={item.id}>
                  <td>{item.name}</td>
                  <td>{item.category}</td>
                  <td>
                    {item.quantity} {item.unit}
                  </td>
                  <td>{item.location}</td>
                  <td>
                    {item.expiryDate
                      ? new Date(item.expiryDate).toLocaleDateString()
                      : 'N/A'}
                  </td>
                  <td>
                    {item.daysUntilExpiry !== null && item.daysUntilExpiry !== undefined
                      ? `${item.daysUntilExpiry} days`
                      : 'N/A'}
                  </td>
                  <td>{getStatusBadge(item.status)}</td>
                  <td>
                    <div className="action-buttons">
                      <button onClick={() => handleEdit(item)} className="btn-edit" title="Edit">
                        ✏️
                      </button>
                      <button onClick={() => handleDelete(item.id)} className="btn-delete" title="Delete">
                        🗑️
                      </button>
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
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


