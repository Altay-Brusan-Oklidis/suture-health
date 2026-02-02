import { useMemo } from 'react';
import {
  bindActionCreators,
  createSlice,
  PayloadAction,
} from '@reduxjs/toolkit';
import { useDispatch } from '@redux/hooks';
import round from '@utils/round';
import {
  DocumentSummaryItemType,
  RequestDocumentType,
  AnnotationItem,
  PageType,
} from '@utils/zodModels';
import sortBy, { SortOrder } from '@utils/sortBy';
import uniqBy from '@utils/uniqBy';
import type { Option } from '@formAdapters/Checkbox';
import type { ModalType } from './components/DocumentActions';
import { docsSortHandler, SortVariant } from './utils';
import type { TransformedFilterOptions } from './apiReducer';

type ActionsType =
  | ModalType
  | 'approve'
  | 'submitCPOFaceToFace'
  | 'submitFaceToFace'
  | 'signSelected'
  | 'saveForLater'
  | null;

export type DocumentSparseListItem = RequestDocumentType | null | 'loading';

export type CurrentPageData = {
  pageNumber: number;
  documentReqId?: number;
  dimensions?: PageType['dimensions'];
};

export interface SetDocumentPayload {
  id: number;
  document: DocumentSparseListItem;
}

interface DocumentsObj {
  [key: string]: DocumentSparseListItem;
}

type AnimationType = 'success' | 'rejected';

export interface Animation {
  type: AnimationType;
  documentId: number;
}

type MedicalCondition = {
  value: string;
  label: string;
};

export interface FormValues {
  encounterDate: Date;
  medicalCondition: MedicalCondition[];
  skilledServices: string[];
  clinicalReason: string;
  homeboundReason: string;
  treatmentPlan?: string | null;
}

export type F2FPageStatusFormValues = Omit<FormValues, 'encounterDate'> & {
  encounterDate: string;
};

export interface CurrentF2FPageStatus {
  id: number;
  templateTypeId: number;
  formValues: F2FPageStatusFormValues;
  doSave: boolean;
  willBeSaved: boolean;
}

type StackAnimation =
  | undefined
  | 'fanning'
  | 'fanningForSelectedDocs'
  | 'stackedLeftDown'
  | 'fadeOut';

interface PageState {
  documents?: DocumentSummaryItemType[];
  viewedDocument?: DocumentSummaryItemType;
  selectedDocuments: string[];
  sortOrder: SortOrder;
  isEditing: boolean;
  currentPageData: CurrentPageData;
  documentAnnotations: AnnotationItem[];
  scale: number;
  rvuAmount: { value: number; unit: number };
  documentScroll: boolean;
  currentAction: ActionsType;
  documentsObj: DocumentsObj;
  sortBy: SortVariant;
  animation?: Animation;
  viewedDocIds: number[];
  isTabletDrawerOpen: boolean;
  isFilterSidebarOpen: boolean;
  isFilterMenuOpen: boolean;
  filterOptions?: TransformedFilterOptions;
  globalDocuments?: DocumentSummaryItemType[];
  isDocumentsLoading: boolean;
  patientOptions?: Option[];
  patientOptionsPage: number;
  noMorePatientOptions: boolean;
  stackAnimation?: StackAnimation;
  filterMenuSearchValue: string;
  allPatientOptions: Option[];
  totalViewedDocsLength: number;
  isSavedUntilMenuOpen: boolean;
  isAnnotationsUpdating: boolean;
  currentF2FPageStatus: CurrentF2FPageStatus;
  scrollHintVisible: boolean;
  annotationsDocumentId?: number;
}

const initialState: PageState = {
  documentsObj: {},
  selectedDocuments: [],
  documentAnnotations: [],
  currentPageData: { pageNumber: 0 },
  sortOrder: 1,
  isEditing: false,
  scale: 1,
  rvuAmount: { value: 0, unit: 0 },
  documentScroll: true,
  currentAction: null,
  sortBy: 'date',
  viewedDocIds: [],
  isTabletDrawerOpen: false,
  isFilterSidebarOpen: false,
  isFilterMenuOpen: false,
  isDocumentsLoading: true,
  patientOptionsPage: 0,
  noMorePatientOptions: false,
  filterMenuSearchValue: '',
  allPatientOptions: [],
  totalViewedDocsLength: 0,
  isSavedUntilMenuOpen: false,
  isAnnotationsUpdating: false,
  currentF2FPageStatus: {
    id: 0,
    templateTypeId: 0,
    doSave: false,
    willBeSaved: false,
    formValues: {
      clinicalReason: '',
      encounterDate: new Date().toString(),
      homeboundReason: '',
      medicalCondition: [],
      skilledServices: [],
      treatmentPlan: '',
    },
  },
  scrollHintVisible: false,
};

export const inboxLocalSlice = createSlice({
  name: 'inbox',
  initialState,
  reducers: {
    calculateRVU(
      state,
      action: PayloadAction<{ value: number; unit: number }>
    ) {
      state.rvuAmount = {
        value: round(action.payload.value),
        unit: round(action.payload.unit),
      };
    },
    setDocumentAt(state, action: PayloadAction<SetDocumentPayload>) {
      const { id, document } = action.payload;

      state.documentsObj[id] = document;
    },
    setDocumentsObj(state, action: PayloadAction<DocumentsObj>) {
      state.documentsObj = action.payload;
    },
    setViewedDocument(
      state,
      action: PayloadAction<DocumentSummaryItemType | undefined>
    ) {
      state.viewedDocument = action.payload;
      state.scale = 1;
    },
    setDocumentScroll(state, action: PayloadAction<boolean>) {
      state.documentScroll = action.payload;
    },
    setIsEditing(state, action: PayloadAction<boolean>) {
      state.isEditing = action.payload;
    },
    setCurrentF2FPageStatus(
      state,
      action: PayloadAction<Partial<CurrentF2FPageStatus>>
    ) {
      state.currentF2FPageStatus = {
        ...state.currentF2FPageStatus,
        ...action.payload,
      };
    },
    setCurrentPageData(state, action: PayloadAction<CurrentPageData>) {
      state.currentPageData = action.payload;
    },
    updateAnnotationMap(
      state,
      action: PayloadAction<{
        documentAnnotations: AnnotationItem[];
        annotationsDocumentId: number;
      }>
    ) {
      state.documentAnnotations = action.payload.documentAnnotations;
      state.annotationsDocumentId = action.payload.annotationsDocumentId;
    },
    updateAnnotationItem(state, action: PayloadAction<AnnotationItem>) {
      state.documentAnnotations = state.documentAnnotations.map((i) =>
        i.id === action.payload.id ? action.payload : i
      );
    },
    addAnnotationItem(state, action: PayloadAction<AnnotationItem>) {
      state.documentAnnotations.push(action.payload);
    },
    deleteAnnotationItem(state, action: PayloadAction<string>) {
      state.documentAnnotations = state.documentAnnotations.filter(
        (i) => i.id !== action.payload
      );
    },
    setDocuments(state, action: PayloadAction<DocumentSummaryItemType[]>) {
      state.documents = docsSortHandler(
        action.payload,
        state.sortBy,
        state.sortOrder
      );
      state.isDocumentsLoading = false;
    },
    setSelectedDocuments(state, action: PayloadAction<string[]>) {
      state.selectedDocuments = sortBy(
        action.payload,
        [(i) => parseInt(i, 10)],
        [1]
      );
    },
    toggleOrder(state) {
      state.viewedDocument = undefined;
      state.sortOrder = state.sortOrder === 1 ? -1 : 1;
      state.documents = docsSortHandler(
        state.documents!,
        state.sortBy,
        state.sortOrder
      );
    },
    setSortBy(state, action: PayloadAction<SortVariant>) {
      state.viewedDocument = undefined;
      state.sortBy = action.payload;
      state.documents = docsSortHandler(
        state.documents!,
        action.payload,
        state.sortOrder
      );
    },
    zoomIn(state) {
      state.scale = state.scale >= 1.8 ? 1.8 : round(state.scale + 0.2);
    },
    zoomOut(state) {
      state.scale = state.scale <= 0.4 ? 0.4 : round(state.scale - 0.2);
    },
    setCurrentAction(state, action: PayloadAction<ActionsType>) {
      state.currentAction = action.payload;
    },
    setAnimation(state, action: PayloadAction<Animation | undefined>) {
      state.animation = action.payload;
    },
    setViewedDocIds(state, action: PayloadAction<number[]>) {
      state.viewedDocIds = action.payload;
    },
    addViewedDocId(state, action: PayloadAction<number>) {
      state.viewedDocIds = [
        ...new Set([...state.viewedDocIds, action.payload]),
      ];
    },
    removeViewedDocIds(state, action: PayloadAction<number[]>) {
      state.viewedDocIds = state.viewedDocIds.filter(
        (i) => !action.payload.includes(i)
      );
    },
    resetViewedDocIds(state) {
      state.viewedDocIds = [];
    },
    setIsTabletDrawerOpen(state, action: PayloadAction<boolean>) {
      state.isTabletDrawerOpen = action.payload;
    },
    toggleIsFilterSidebarOpen(state) {
      state.isFilterSidebarOpen = !state.isFilterSidebarOpen;
    },
    toggleIsFilterMenuOpen(state) {
      state.isFilterMenuOpen = !state.isFilterMenuOpen;
    },
    setFilterOptions(state, action: PayloadAction<TransformedFilterOptions>) {
      state.filterOptions = action.payload;
    },
    setPatientOptions(state, action: PayloadAction<Option[]>) {
      state.patientOptions = action.payload;
      state.allPatientOptions = uniqBy(
        [...state.allPatientOptions, ...action.payload],
        'value'
      );
    },
    increasePatientOptionsPage(state) {
      state.patientOptionsPage += 1;
    },
    setNoMorePatientOptions(state, action: PayloadAction<boolean>) {
      state.noMorePatientOptions = action.payload;
    },
    resetPatientOptionsPage(state) {
      state.patientOptionsPage = 0;
      state.patientOptions = [];
    },
    setGlobalDocuments(
      state,
      action: PayloadAction<DocumentSummaryItemType[]>
    ) {
      state.globalDocuments = action.payload;
    },
    setIsDocumentsLoading(state, action: PayloadAction<boolean>) {
      state.isDocumentsLoading = action.payload;
    },
    setStackAnimation(state, action: PayloadAction<StackAnimation>) {
      state.stackAnimation = action.payload;
    },
    setFilterMenuSearchValue(state, action: PayloadAction<string>) {
      state.filterMenuSearchValue = action.payload;
    },
    setAllPatientOptions(state, action: PayloadAction<Option[]>) {
      state.allPatientOptions = uniqBy(
        [...state.allPatientOptions, ...action.payload],
        'value'
      );
    },
    setTotalViewedDocsLength(state, action: PayloadAction<number>) {
      state.totalViewedDocsLength = action.payload;
    },
    setIsSavedUntilMenuOpen(state, action: PayloadAction<boolean>) {
      state.isSavedUntilMenuOpen = action.payload;
    },
    setIsAnnotationsUpdating(state, action: PayloadAction<boolean>) {
      state.isAnnotationsUpdating = action.payload;
    },
    setScrollHintVisible(state, action: PayloadAction<boolean>) {
      state.scrollHintVisible = action.payload;
    },
  },
});

export const inboxActions = inboxLocalSlice.actions;

export const useInboxActions = () => {
  const dispatch = useDispatch();
  const actions = useMemo(
    () => bindActionCreators(inboxActions, dispatch),
    [dispatch]
  );

  return actions;
};
