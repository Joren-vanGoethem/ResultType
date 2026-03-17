import { useState, useEffect } from "react";
import * as api from "../api.js";
import ValidationErrors from "./ValidationErrors.jsx";

const STATUSES = ["Pending", "Confirmed", "Shipped", "Delivered", "Cancelled"];

export default function Orders({ lang }) {
  const [products, setProducts] = useState([]);
  const [errors, setErrors] = useState(null);
  const [successMsg, setSuccessMsg] = useState("");
  const [createdOrder, setCreatedOrder] = useState(null);
  const [lookupId, setLookupId] = useState("");
  const [lookedUpOrder, setLookedUpOrder] = useState(null);
  const [statusForm, setStatusForm] = useState({ status: "Confirmed", trackingUri: "" });

  const [form, setForm] = useState({
    customerName: "",
    customerEmail: "",
    customerPhone: "",
    shippingDeadline: "",
    preferredDeliveryTime: "10:00",
    isPriority: false,
    notes: "",
    lines: [{ productId: "", quantity: "1" }],
  });

  useEffect(() => {
    api.getProducts(lang).then((res) => {
      if (res.ok) setProducts(res.data);
    });
  }, [lang]);

  const flash = (msg) => {
    setSuccessMsg(msg);
    setTimeout(() => setSuccessMsg(""), 3000);
  };

  const addLine = () => setForm({ ...form, lines: [...form.lines, { productId: "", quantity: "1" }] });
  const removeLine = (i) => setForm({ ...form, lines: form.lines.filter((_, idx) => idx !== i) });
  const updateLine = (i, field, value) => {
    const lines = [...form.lines];
    lines[i] = { ...lines[i], [field]: value };
    setForm({ ...form, lines });
  };

  const handleCreate = async (e) => {
    e.preventDefault();
    setErrors(null);
    setCreatedOrder(null);

    const body = {
      ...form,
      lines: form.lines.map((l) => ({
        productId: l.productId,
        quantity: parseInt(l.quantity) || 0,
      })),
    };

    const res = await api.createOrder(body, lang);
    if (res.ok) {
      setCreatedOrder(res.data);
      flash("Order created!");
    } else {
      setErrors(res.errors);
    }
  };

  const handleLookup = async () => {
    setErrors(null);
    setLookedUpOrder(null);
    const res = await api.getOrder(lookupId, lang);
    if (res.ok) setLookedUpOrder(res.data);
    else setErrors(res.errors);
  };

  const handleStatusUpdate = async () => {
    if (!lookedUpOrder) return;
    setErrors(null);
    const body = {
      status: statusForm.status,
      trackingUri: statusForm.trackingUri || null,
    };
    const res = await api.updateOrderStatus(lookedUpOrder.id, body, lang);
    if (res.ok) {
      setLookedUpOrder(res.data);
      flash("Status updated!");
    } else {
      setErrors(res.errors);
    }
  };

  const handleCancel = async () => {
    if (!lookedUpOrder) return;
    setErrors(null);
    const res = await api.cancelOrder(lookedUpOrder.id, lang);
    if (res.ok) {
      setLookedUpOrder(res.data);
      flash("Order cancelled!");
    } else {
      setErrors(res.errors);
    }
  };

  const handleValidateIntegrity = async () => {
    if (!lookedUpOrder) return;
    setErrors(null);
    const res = await api.validateOrderIntegrity(lookedUpOrder.id, lang);
    if (res.ok) flash("Order integrity OK!");
    else setErrors(res.errors);
  };

  const handleTestBadOrder = async () => {
    setErrors(null);
    setCreatedOrder(null);
    // Intentionally bad data to trigger many validation errors
    const res = await api.createOrder({
      customerName: "",
      customerEmail: "not-an-email",
      customerPhone: "123",
      shippingDeadline: "2020-01-01",
      preferredDeliveryTime: "03:00",
      isPriority: false,
      notes: "",
      lines: [],
    }, lang);
    if (res.ok) flash("Created (unexpected)");
    else setErrors(res.errors);
  };

  return (
    <div>
      <h2>Orders</h2>

      {successMsg && <div className="success">{successMsg}</div>}
      <ValidationErrors errors={errors} />

      <div className="two-columns">
        <div>
          <form onSubmit={handleCreate} className="form">
            <h3>Create Order</h3>
            <div className="form-grid">
              <label>
                Customer Name
                <input value={form.customerName} onChange={(e) => setForm({ ...form, customerName: e.target.value })} />
              </label>
              <label>
                Email
                <input value={form.customerEmail} onChange={(e) => setForm({ ...form, customerEmail: e.target.value })} />
              </label>
              <label>
                Phone
                <input value={form.customerPhone} onChange={(e) => setForm({ ...form, customerPhone: e.target.value })} />
              </label>
              <label>
                Shipping Deadline
                <input type="date" value={form.shippingDeadline} onChange={(e) => setForm({ ...form, shippingDeadline: e.target.value })} />
              </label>
              <label>
                Preferred Delivery Time
                <input type="time" value={form.preferredDeliveryTime} onChange={(e) => setForm({ ...form, preferredDeliveryTime: e.target.value })} />
              </label>
              <label className="checkbox-label">
                <input type="checkbox" checked={form.isPriority} onChange={(e) => setForm({ ...form, isPriority: e.target.checked })} />
                Priority Order
              </label>
            </div>
            <label>
              Notes
              <input value={form.notes} onChange={(e) => setForm({ ...form, notes: e.target.value })} />
            </label>

            <h4>Order Lines</h4>
            {form.lines.map((line, i) => (
              <div key={i} className="order-line">
                <select value={line.productId} onChange={(e) => updateLine(i, "productId", e.target.value)}>
                  <option value="">Select product...</option>
                  {products.map((p) => (
                    <option key={p.id} value={p.id}>{p.name} (${p.price.toFixed(2)}, stock: {p.stockQuantity})</option>
                  ))}
                </select>
                <input type="number" value={line.quantity} onChange={(e) => updateLine(i, "quantity", e.target.value)} min="1" placeholder="Qty" style={{ width: 80 }} />
                <button type="button" onClick={() => removeLine(i)} className="btn-sm btn-danger">×</button>
              </div>
            ))}
            <button type="button" onClick={addLine} className="btn-secondary">+ Add Line</button>

            <div className="form-actions">
              <button type="submit">Create Order</button>
              <button type="button" onClick={handleTestBadOrder} className="btn-secondary">
                Test Bad Order (triggers errors)
              </button>
            </div>
          </form>
        </div>

        <div>
          <div className="form">
            <h3>Lookup Order</h3>
            <div className="lookup-row">
              <input value={lookupId} onChange={(e) => setLookupId(e.target.value)} placeholder="Order ID (GUID)" />
              <button onClick={handleLookup}>Lookup</button>
            </div>

            {createdOrder && !lookedUpOrder && (
              <div className="hint">
                Last created: <button className="btn-link" onClick={() => { setLookupId(createdOrder.id); }}>{createdOrder.id}</button>
              </div>
            )}
          </div>

          {lookedUpOrder && (
            <div className="order-detail">
              <h3>Order Detail</h3>
              <dl>
                <dt>ID</dt><dd><code>{lookedUpOrder.id}</code></dd>
                <dt>Customer</dt><dd>{lookedUpOrder.customerName} ({lookedUpOrder.customerEmail})</dd>
                <dt>Status</dt><dd><span className={`status status-${lookedUpOrder.status.toLowerCase()}`}>{lookedUpOrder.status}</span></dd>
                <dt>Total</dt><dd>${lookedUpOrder.totalAmount.toFixed(2)}</dd>
                <dt>Priority</dt><dd>{lookedUpOrder.isPriority ? "Yes" : "No"}</dd>
                <dt>Deadline</dt><dd>{lookedUpOrder.shippingDeadline}</dd>
              </dl>

              {lookedUpOrder.lines.length > 0 && (
                <table className="compact-table">
                  <thead>
                    <tr><th>Product</th><th>Qty</th><th>Unit Price</th><th>Total</th></tr>
                  </thead>
                  <tbody>
                    {lookedUpOrder.lines.map((l, i) => (
                      <tr key={i}>
                        <td>{l.productName}</td>
                        <td>{l.quantity}</td>
                        <td>${l.unitPrice.toFixed(2)}</td>
                        <td>${l.lineTotal.toFixed(2)}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              )}

              <div className="order-actions">
                <h4>Update Status</h4>
                <div className="status-row">
                  <select value={statusForm.status} onChange={(e) => setStatusForm({ ...statusForm, status: e.target.value })}>
                    {STATUSES.map((s) => <option key={s}>{s}</option>)}
                  </select>
                  <input value={statusForm.trackingUri} onChange={(e) => setStatusForm({ ...statusForm, trackingUri: e.target.value })} placeholder="Tracking URI (optional)" />
                  <button onClick={handleStatusUpdate}>Update</button>
                </div>
                <div className="status-row">
                  <button onClick={handleCancel} className="btn-danger">Cancel Order</button>
                  <button onClick={handleValidateIntegrity} className="btn-secondary">Validate Integrity</button>
                </div>
              </div>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}
