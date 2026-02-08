import { useQuery } from '@tanstack/react-query';
import { dashboardApi } from '../api';

export const useDashboard = () => {
  return useQuery({
    queryKey: ['dashboard'],
    queryFn: dashboardApi.getSummary,
    refetchInterval: 30000, // Refetch every 30 seconds
  });
};
