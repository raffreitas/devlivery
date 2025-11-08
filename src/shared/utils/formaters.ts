/**
 * @param d
 * @returns `YYYY-MM-DD`
 */
export const formatDate = (d: Date) => d.toISOString().slice(0, 10);
