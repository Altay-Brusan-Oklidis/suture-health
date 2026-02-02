import { configureStore, combineReducers } from '@reduxjs/toolkit';
import { createLogger } from 'redux-logger';
import { inboxLocalSlice } from '@containers/Inbox/localReducer';
import { inboxApiSlice } from '@containers/Inbox/apiReducer';
import { mainLayoutSlice } from '@layouts/MainLayout/reducer';

const combinedReducer = combineReducers({
  [inboxLocalSlice.name]: inboxLocalSlice.reducer,
  [inboxApiSlice.reducerPath]: inboxApiSlice.reducer,
  [mainLayoutSlice.name]: mainLayoutSlice.reducer,
});

const middleware = [inboxApiSlice.middleware];

if (
  process.env.NODE_ENV === 'development' &&
  process.env.NEXT_PUBLIC_REDUX_LOGGER_ENABLED === 'true'
) {
  const logger = createLogger({
    collapsed: true,
    predicate: (_, action) => !action.type.match(/^\w+Api/g), // avoid api actions
  });

  middleware.push(logger);
}

export const store = configureStore({
  reducer: combinedReducer,
  devTools: true,
  middleware: (getDefaultMiddleware) =>
    getDefaultMiddleware({
      immutableCheck: { warnAfter: 128 },
      serializableCheck: { warnAfter: 128 },
    }).concat(middleware),
});

export type AppStore = ReturnType<typeof store.getState>;
export type AppState = ReturnType<typeof combinedReducer>;
export type AppDispatch = typeof store.dispatch;
