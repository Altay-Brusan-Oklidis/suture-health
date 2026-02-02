import { useMemo } from 'react';
import { bindActionCreators, createSlice } from '@reduxjs/toolkit';
import type { PayloadAction } from '@reduxjs/toolkit';
import { useDispatch } from '@redux/hooks';

interface LayoutState {
  selectedOrganizationId?: string;
  isTourOpen: boolean;
}

const initialState: LayoutState = {
  isTourOpen: false,
};

export const mainLayoutSlice = createSlice({
  name: 'mainLayout',
  initialState,
  reducers: {
    setSelectedOrganizationId(
      state,
      action: PayloadAction<string | undefined>
    ) {
      if (action.payload === undefined) {
        sessionStorage.removeItem('selectedOrganizationId');
      } else {
        sessionStorage.setItem('selectedOrganizationId', action.payload);
      }
      state.selectedOrganizationId = action.payload;
    },
    setIsTourOpen(state, action: PayloadAction<boolean>) {
      state.isTourOpen = action.payload;
    },
  },
});

export const mainLayoutActions = mainLayoutSlice.actions;

export const useMainLayoutActions = () => {
  const dispatch = useDispatch();
  const actions = useMemo(
    () => bindActionCreators(mainLayoutActions, dispatch),
    [dispatch]
  );

  return actions;
};
