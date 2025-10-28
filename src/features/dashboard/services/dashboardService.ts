import type { Order } from '../../orders/types';
import type { DashboardStats } from '../types';

export const dashboardService = {
  calculateStats: (orders: Order[]): DashboardStats => {
    const totalOrders = orders.length;
    const totalRevenue = orders
      .filter(o => o.status !== 'cancelled')
      .reduce((sum, order) => sum + order.total, 0);

    const pendingOrders = orders.filter(
      o => o.status === 'pending' || o.status === 'preparing' || o.status === 'ready'
    ).length;

    const deliveredOrders = orders.filter(o => o.status === 'delivered').length;

    const averageOrderValue = totalOrders > 0 ? totalRevenue / totalOrders : 0;

    return {
      totalOrders,
      totalRevenue,
      pendingOrders,
      deliveredOrders,
      averageOrderValue,
    };
  },

  getOrdersByStatus: (orders: Order[]) => {
    return {
      pending: orders.filter(o => o.status === 'pending').length,
      preparing: orders.filter(o => o.status === 'preparing').length,
      ready: orders.filter(o => o.status === 'ready').length,
      delivered: orders.filter(o => o.status === 'delivered').length,
      cancelled: orders.filter(o => o.status === 'cancelled').length,
    };
  },
};
