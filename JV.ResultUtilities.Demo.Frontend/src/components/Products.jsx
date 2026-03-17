import { useState, useEffect } from "react";
import * as api from "../api.js";
import ValidationErrors from "./ValidationErrors.jsx";

const CATEGORIES = ["Electronics", "Clothing", "Books", "Food", "Other"];

const emptyProduct = {
  name: "",
  sku: "",
  description: "",
  price: "",
  stockQuantity: "",
  category: "Electronics",
};

export default function Products({ lang }) {
  const [products, setProducts] = useState([]);
  const [form, setForm] = useState(emptyProduct);
  const [editingId, setEditingId] = useState(null);
  const [errors, setErrors] = useState(null);
  const [successMsg, setSuccessMsg] = useState("");
  const [cacheStats, setCacheStats] = useState(null);

  const loadProducts = async () => {
    const res = await api.getProducts(lang);
    if (res.ok) setProducts(res.data);
    else setErrors(res.errors);
  };

  useEffect(() => { loadProducts(); }, [lang]);

  const flash = (msg) => {
    setSuccessMsg(msg);
    setTimeout(() => setSuccessMsg(""), 3000);
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setErrors(null);

    const body = {
      ...form,
      price: parseFloat(form.price) || 0,
      stockQuantity: parseInt(form.stockQuantity) || 0,
    };

    let res;
    if (editingId) {
      res = await api.updateProduct(editingId, { ...body, isActive: true }, lang);
    } else {
      res = await api.createProduct(body, lang);
    }

    if (res.ok) {
      setForm(emptyProduct);
      setEditingId(null);
      flash(editingId ? "Product updated!" : "Product created!");
      loadProducts();
    } else {
      setErrors(res.errors);
    }
  };

  const handleEdit = (p) => {
    setEditingId(p.id);
    setErrors(null);
    setForm({
      name: p.name,
      sku: p.sku,
      description: p.description,
      price: String(p.price),
      stockQuantity: String(p.stockQuantity),
      category: p.category,
    });
  };

  const handleDelete = async (id) => {
    setErrors(null);
    const res = await api.deleteProduct(id, lang);
    if (res.ok) {
      flash("Product deleted!");
      loadProducts();
    } else {
      setErrors(res.errors);
    }
  };

  const handleCancel = () => {
    setEditingId(null);
    setForm(emptyProduct);
    setErrors(null);
  };

  const loadCacheStats = async () => {
    const res = await api.getCacheStats();
    if (res.ok) setCacheStats(res.data);
  };

  const handleBulkValidate = async () => {
    setErrors(null);
    // Send intentionally bad data to trigger validation
    const badProducts = [
      { name: "", sku: "", description: "", price: -5, stockQuantity: -1, category: "Electronics" },
      { name: "AB", sku: "X", description: "ok", price: 0, stockQuantity: -10, category: "Books" },
    ];
    const res = await api.validateBatchProducts(badProducts, lang);
    if (!res.ok) setErrors(res.errors);
    else flash("All valid (unexpected)");
  };

  return (
    <div>
      <h2>Products</h2>

      {successMsg && <div className="success">{successMsg}</div>}
      <ValidationErrors errors={errors} />

      <form onSubmit={handleSubmit} className="form">
        <h3>{editingId ? "Edit Product" : "Create Product"}</h3>
        <div className="form-grid">
          <label>
            Name
            <input value={form.name} onChange={(e) => setForm({ ...form, name: e.target.value })} />
          </label>
          <label>
            SKU
            <input value={form.sku} onChange={(e) => setForm({ ...form, sku: e.target.value })} disabled={!!editingId} />
          </label>
          <label>
            Price
            <input type="number" step="0.01" value={form.price} onChange={(e) => setForm({ ...form, price: e.target.value })} />
          </label>
          <label>
            Stock
            <input type="number" value={form.stockQuantity} onChange={(e) => setForm({ ...form, stockQuantity: e.target.value })} />
          </label>
          <label>
            Category
            <select value={form.category} onChange={(e) => setForm({ ...form, category: e.target.value })}>
              {CATEGORIES.map((c) => <option key={c}>{c}</option>)}
            </select>
          </label>
          <label>
            Description
            <input value={form.description} onChange={(e) => setForm({ ...form, description: e.target.value })} />
          </label>
        </div>
        <div className="form-actions">
          <button type="submit">{editingId ? "Update" : "Create"}</button>
          {editingId && <button type="button" onClick={handleCancel}>Cancel</button>}
        </div>
      </form>

      <div className="toolbar">
        <button onClick={handleBulkValidate} className="btn-secondary">
          Test Batch Validation (sends bad data)
        </button>
        <button onClick={loadCacheStats} className="btn-secondary">
          Cache Stats
        </button>
        {cacheStats && (
          <span className="cache-stats">
            Hits: {cacheStats.hits} | Misses: {cacheStats.misses} | Ratio: {(cacheStats.hitRatio * 100).toFixed(0)}% | Size: {cacheStats.size}
          </span>
        )}
      </div>

      <table>
        <thead>
          <tr>
            <th>Name</th>
            <th>SKU</th>
            <th>Price</th>
            <th>Stock</th>
            <th>Category</th>
            <th>Active</th>
            <th>Actions</th>
          </tr>
        </thead>
        <tbody>
          {products.length === 0 && (
            <tr><td colSpan={7} className="empty">No products yet. Create one above.</td></tr>
          )}
          {products.map((p) => (
            <tr key={p.id}>
              <td>{p.name}</td>
              <td><code>{p.sku}</code></td>
              <td>${p.price.toFixed(2)}</td>
              <td>{p.stockQuantity}</td>
              <td>{p.category}</td>
              <td>{p.isActive ? "Yes" : "No"}</td>
              <td>
                <button onClick={() => handleEdit(p)} className="btn-sm">Edit</button>
                <button onClick={() => handleDelete(p.id)} className="btn-sm btn-danger">Delete</button>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
