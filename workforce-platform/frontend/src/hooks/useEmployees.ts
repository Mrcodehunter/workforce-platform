import { useQuery, useMutation, useQueryClient, keepPreviousData } from '@tanstack/react-query';
import { employeesApi, type PagedResult } from '../api';
import type { Employee } from '../types';

export const useEmployees = () => {
  return useQuery({
    queryKey: ['employees'],
    queryFn: employeesApi.getAll,
  });
};

export const useEmployeesPaged = (page: number = 1, pageSize: number = 10) => {
  return useQuery<PagedResult<Employee>>({
    queryKey: ['employees', 'paged', page, pageSize],
    queryFn: () => employeesApi.getPaged(page, pageSize),
    placeholderData: keepPreviousData, // Keep previous data while loading new page (React Query v5)
  });
};

export const useEmployee = (id: string) => {
  return useQuery({
    queryKey: ['employees', id],
    queryFn: () => employeesApi.getById(id),
    enabled: !!id,
  });
};

export const useCreateEmployee = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (employee: Partial<Employee>) => employeesApi.create(employee),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['employees'] });
    },
  });
};

export const useUpdateEmployee = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, employee }: { id: string; employee: Partial<Employee> }) =>
      employeesApi.update(id, employee),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['employees'] });
      queryClient.invalidateQueries({ queryKey: ['employees', variables.id] });
    },
  });
};

export const useDeleteEmployee = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => employeesApi.delete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['employees'] });
    },
  });
};
