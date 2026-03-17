const BASE = "/api";

async function request(method, path, body, lang) {
  const headers = { "Content-Type": "application/json" };
  if (lang) headers["Accept-Language"] = lang;

  const res = await fetch(`${BASE}${path}`, {
    method,
    headers,
    body: body ? JSON.stringify(body) : undefined,
  });

  if (res.status === 204) return { ok: true, data: null };
  if (res.status === 404) return { ok: false, errors: { "not_found": ["Resource not found."] } };

  const data = await res.json();

  if (res.ok) return { ok: true, data };

  // 422 ValidationProblemDetails
  return { ok: false, errors: data.errors || {}, title: data.title };
}

// Products
export const getProducts = (lang) => request("GET", "/products", null, lang);
export const getProduct = (id, lang) => request("GET", `/products/${id}`, null, lang);
export const createProduct = (product, lang) => request("POST", "/products", product, lang);
export const updateProduct = (id, product, lang) => request("PUT", `/products/${id}`, product, lang);
export const deleteProduct = (id, lang) => request("DELETE", `/products/${id}`, null, lang);
export const getProductsByCategory = (category, lang) => request("GET", `/products/categories/${category}`, null, lang);
export const getCacheStats = () => request("GET", "/products/cache-stats");
export const bulkImportProducts = (products, lang) => request("POST", "/products/import", products, lang);
export const validateBatchProducts = (products, lang) => request("POST", "/products/validate-batch", products, lang);

// Orders
export const getOrder = (id, lang) => request("GET", `/orders/${id}`, null, lang);
export const getOrderSummary = (id, lang) => request("GET", `/orders/${id}/summary`, null, lang);
export const createOrder = (order, lang) => request("POST", "/orders", order, lang);
export const updateOrderStatus = (id, body, lang) => request("PATCH", `/orders/${id}/status`, body, lang);
export const cancelOrder = (id, lang) => request("DELETE", `/orders/${id}`, null, lang);
export const checkAvailability = (lines, lang) => request("POST", "/orders/check-availability", lines, lang);
export const validateOrderIntegrity = (id, lang) => request("POST", `/orders/${id}/validate`, null, lang);
