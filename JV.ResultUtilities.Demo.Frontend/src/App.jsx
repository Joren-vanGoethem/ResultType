import { useState } from "react";
import Products from "./components/Products.jsx";
import Orders from "./components/Orders.jsx";

export default function App() {
  const [tab, setTab] = useState("products");
  const [lang, setLang] = useState("en");

  return (
    <div className="app">
      <header>
        <h1>ResultUtilities Demo</h1>
        <p className="subtitle">
          Test the API and see how Result types handle validation errors
        </p>
        <nav>
          <button className={tab === "products" ? "active" : ""} onClick={() => setTab("products")}>
            Products
          </button>
          <button className={tab === "orders" ? "active" : ""} onClick={() => setTab("orders")}>
            Orders
          </button>
          <div className="lang-picker">
            <label>Language:</label>
            <select value={lang} onChange={(e) => setLang(e.target.value)}>
              <option value="en">English</option>
              <option value="nl">Nederlands</option>
            </select>
          </div>
        </nav>
      </header>
      <main>
        {tab === "products" && <Products lang={lang} />}
        {tab === "orders" && <Orders lang={lang} />}
      </main>
    </div>
  );
}
