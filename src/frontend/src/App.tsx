import { useState } from 'react';
import type { Product } from './types';
import { ProductList } from './components/ProductList';
import { useProducts } from './hooks/useProducts';
import { addToCart } from './api';
import './App.css';

function App() {
  const { products, loading, error } = useProducts();
  const [cartMessage, setCartMessage] = useState<string | null>(null);

  async function handleAddToCart(product: Product) {
    try {
      await addToCart({ productId: product.id, quantity: 1 });
      setCartMessage(`"${product.name}" added to cart!`);
      setTimeout(() => setCartMessage(null), 3000);
    } catch {
      setCartMessage('Failed to add item to cart.');
    }
  }

  return (
    <div className="app">
      <header className="app__header">
        <h1 className="app__title">Mock Shop</h1>
      </header>

      <main className="app__main">
        {cartMessage && (
          <div className="app__notification" role="status">
            {cartMessage}
          </div>
        )}

        {loading && <p className="app__loading">Loading products…</p>}
        {error && <p className="app__error">Error: {error}</p>}
        {!loading && !error && (
          <ProductList products={products} onAddToCart={handleAddToCart} />
        )}
      </main>
    </div>
  );
}

export default App;
