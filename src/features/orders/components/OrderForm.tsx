import { useState } from "react";
import { Button } from "../../../shared/components/Button";
import { Input } from "../../../shared/components/Input";
import { useProducts } from "../../../shared/contexts/ProductContext";
import type { OrderFormData, OrderItem } from "../types";

interface OrderFormProps {
  onSubmit: (data: OrderFormData) => void;
  onCancel: () => void;
}

export function OrderForm({ onSubmit, onCancel }: OrderFormProps) {
  const { products } = useProducts();
  const [customerName, setCustomerName] = useState("");
  const [customerPhone, setCustomerPhone] = useState("");
  const [deliveryAddress, setDeliveryAddress] = useState("");
  const [items, setItems] = useState<OrderItem[]>([]);
  const [selectedProductId, setSelectedProductId] = useState("");
  const [quantity, setQuantity] = useState(1);
  const [notes, setNotes] = useState("");

  const availableProducts = products.filter((p) => p.available);

  const handleAddItem = () => {
    if (!selectedProductId) return;

    const product = products.find((p) => p.id === selectedProductId);
    if (!product) return;

    const existingItemIndex = items.findIndex(
      (item) => item.product.id === selectedProductId,
    );

    if (existingItemIndex >= 0) {
      const newItems = [...items];
      newItems[existingItemIndex].quantity += quantity;
      if (notes) {
        newItems[existingItemIndex].notes = notes;
      }
      setItems(newItems);
    } else {
      setItems([...items, { product, quantity, notes: notes || undefined }]);
    }

    setSelectedProductId("");
    setQuantity(1);
    setNotes("");
  };

  const handleRemoveItem = (productId: string) => {
    setItems(items.filter((item) => item.product.id !== productId));
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (items.length === 0) {
      alert("Adicione pelo menos um produto ao pedido");
      return;
    }
    onSubmit({
      items,
      customerName,
      customerPhone,
      deliveryAddress,
    });
  };

  const total = items.reduce(
    (sum, item) => sum + item.product.price * item.quantity,
    0,
  );

  return (
    <form onSubmit={handleSubmit} className="space-y-6">
      <div className="space-y-4">
        <h3 className="text-lg font-semibold">Dados do Cliente</h3>

        <Input
          label="Nome do Cliente"
          type="text"
          value={customerName}
          onChange={(e) => setCustomerName(e.target.value)}
          required
        />

        <Input
          label="Telefone"
          type="tel"
          value={customerPhone}
          onChange={(e) => setCustomerPhone(e.target.value)}
          placeholder="(00) 00000-0000"
          required
        />

        <Input
          label="Endereço de Entrega"
          type="text"
          value={deliveryAddress}
          onChange={(e) => setDeliveryAddress(e.target.value)}
          required
        />
      </div>

      <div className="space-y-4">
        <h3 className="text-lg font-semibold">Itens do Pedido</h3>

        <div className="grid grid-cols-1 md:grid-cols-3 gap-3">
          <div className="md:col-span-1">
            <label
              htmlFor="product-select"
              className="block text-sm font-medium text-gray-700 mb-1"
            >
              Produto
            </label>
            <select
              id="product-select"
              value={selectedProductId}
              onChange={(e) => setSelectedProductId(e.target.value)}
              className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-orange-500"
            >
              <option value="">Selecione um produto</option>
              {availableProducts.map((product) => (
                <option key={product.id} value={product.id}>
                  {product.name} - R$ {product.price.toFixed(2)}
                </option>
              ))}
            </select>
          </div>

          <div>
            <Input
              label="Quantidade"
              type="number"
              min="1"
              value={quantity}
              onChange={(e) => setQuantity(Number.parseInt(e.target.value, 10))}
            />
          </div>

          <div>
            <Input
              label="Observações"
              type="text"
              value={notes}
              onChange={(e) => setNotes(e.target.value)}
              placeholder="Ex: sem cebola"
            />
          </div>
        </div>

        <Button
          type="button"
          variant="secondary"
          onClick={handleAddItem}
          disabled={!selectedProductId}
          className="w-full md:w-auto"
        >
          + Adicionar Item
        </Button>

        {items.length > 0 && (
          <div className="border border-gray-200 rounded-lg overflow-hidden">
            <table className="min-w-full divide-y divide-gray-200">
              <thead className="bg-gray-50">
                <tr>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">
                    Produto
                  </th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">
                    Qtd
                  </th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">
                    Preço Unit.
                  </th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">
                    Subtotal
                  </th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">
                    Ações
                  </th>
                </tr>
              </thead>
              <tbody className="bg-white divide-y divide-gray-200">
                {items.map((item) => (
                  <tr key={item.product.id}>
                    <td className="px-4 py-3 text-sm">
                      <div>
                        <div className="font-medium text-gray-900">
                          {item.product.name}
                        </div>
                        {item.notes && (
                          <div className="text-gray-500 text-xs">
                            Obs: {item.notes}
                          </div>
                        )}
                      </div>
                    </td>
                    <td className="px-4 py-3 text-sm text-gray-900">
                      {item.quantity}
                    </td>
                    <td className="px-4 py-3 text-sm text-gray-900">
                      R$ {item.product.price.toFixed(2)}
                    </td>
                    <td className="px-4 py-3 text-sm font-medium text-gray-900">
                      R$ {(item.product.price * item.quantity).toFixed(2)}
                    </td>
                    <td className="px-4 py-3 text-sm">
                      <Button
                        type="button"
                        size="sm"
                        variant="danger"
                        onClick={() => handleRemoveItem(item.product.id)}
                      >
                        Remover
                      </Button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
            <div className="bg-gray-50 px-4 py-3 flex justify-between items-center">
              <span className="text-lg font-semibold text-gray-900">
                Total:
              </span>
              <span className="text-2xl font-bold text-orange-600">
                R$ {total.toFixed(2)}
              </span>
            </div>
          </div>
        )}
      </div>

      <div className="flex justify-end space-x-3 pt-4 border-t">
        <Button type="button" variant="secondary" onClick={onCancel}>
          Cancelar
        </Button>
        <Button type="submit" variant="primary">
          Criar Pedido
        </Button>
      </div>
    </form>
  );
}
