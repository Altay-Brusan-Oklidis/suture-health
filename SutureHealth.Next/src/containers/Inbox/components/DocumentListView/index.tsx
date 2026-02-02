import { memo } from 'react';
import DocumentFilters from './components/DocumentFilters';
import DocumentList from './components/DocumentList';
import SignSelected from './components/SignSelected';
import Drawer from './components/Drawer';

function DocumentListView() {
  return (
    <Drawer>
      <DocumentFilters />
      <DocumentList />
      <SignSelected />
    </Drawer>
  );
}

export default memo(DocumentListView);
