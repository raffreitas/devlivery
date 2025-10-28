import type { ReactNode } from "react";
import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useState,
} from "react";
import { orderService } from "../../features/orders/services/orderService";
import type { Order, OrderFormData } from "../../features/orders/types";

interface OrderContextData {
  orders: Order[];
  loading: boolean;
  fetchOrders: () => void;
  createOrder: (data: OrderFormData) => void;
  updateOrderStatus: (id: string, status: Order["status"]) => void;
  deleteOrder: (id: string) => void;
  getTodayOrders: () => Order[];
}

const OrderContext = createContext<OrderContextData | undefined>(undefined);

export const OrderProvider: React.FC<{ children: ReactNode }> = ({
  children,
}) => {
  const [orders, setOrders] = useState<Order[]>([]);
  const [loading, setLoading] = useState(false);

  const fetchOrders = useCallback(() => {
    setLoading(true);
    try {
      const data = orderService.getAll();
      setOrders(data);
    } catch (error) {
      console.error("Error fetching orders:", error);
    } finally {
      setLoading(false);
    }
  }, []);

  const createOrder = useCallback((data: OrderFormData) => {
    try {
      const newOrder = orderService.create(data);
      setOrders((prev) => [...prev, newOrder]);
    } catch (error) {
      console.error("Error creating order:", error);
      throw error;
    }
  }, []);

  const updateOrderStatus = useCallback(
    (id: string, status: Order["status"]) => {
      try {
        const updatedOrder = orderService.updateStatus(id, status);
        setOrders((prev) => prev.map((o) => (o.id === id ? updatedOrder : o)));
      } catch (error) {
        console.error("Error updating order status:", error);
        throw error;
      }
    },
    [],
  );

  const deleteOrder = useCallback((id: string) => {
    try {
      orderService.delete(id);
      setOrders((prev) => prev.filter((o) => o.id !== id));
    } catch (error) {
      console.error("Error deleting order:", error);
      throw error;
    }
  }, []);

  const getTodayOrders = useCallback(() => {
    return orderService.getTodayOrders();
  }, []);

  useEffect(() => {
    fetchOrders();
  }, [fetchOrders]);

  return (
    <OrderContext.Provider
      value={{
        orders,
        loading,
        fetchOrders,
        createOrder,
        updateOrderStatus,
        deleteOrder,
        getTodayOrders,
      }}
    >
      {children}
    </OrderContext.Provider>
  );
};

export const useOrders = () => {
  const context = useContext(OrderContext);
  if (!context) {
    throw new Error("useOrders must be used within an OrderProvider");
  }
  return context;
};
