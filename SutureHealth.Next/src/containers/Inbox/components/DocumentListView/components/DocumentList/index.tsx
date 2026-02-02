import { useRef, useEffect, useCallback, useMemo, useState } from 'react';
import { Flex, Text, VStack } from '@chakra-ui/react';
import { CheckboxGroup, InboxIconFA, Card, CardProps } from 'suture-theme';
import { Virtuoso, VirtuosoHandle } from 'react-virtuoso';
import { useWatch, useFormContext } from 'react-hook-form';
import dayjs from 'dayjs';
import { useSelector } from '@redux/hooks';
import useRefetchViewedDoc from '@hooks/useRefetchViewedDoc';
import useUpdateAnnotations from '@hooks/useUpdateAnnotations';
import { useInboxActions } from '@containers/Inbox/localReducer';
import {
  useMoveToInboxMutation,
  useGetMemberIdentityQuery,
  useRegisterDocumentViewMutation,
  useGetMiscSettingsQuery,
} from '@containers/Inbox/apiReducer';
import type { FilterData } from '@containers/Inbox';
import { useSetDocumentsRef, useDocViewRef } from '@containers/Inbox/context';
import { DocumentSummaryItemType } from '@utils/zodModels';
import fullNameWithSuffix from '@utils/fullNameWithSuffix';
import {
  sortByApprovedAt,
  isAutomaticallyMovedDoc,
} from '@containers/Inbox/utils';
import SaveModal from '@containers/Inbox/components/SaveModal';
import useAnnotationValidation from '@hooks/useAnnotationValidation';
import Skeleton from './components/Skeleton';
import ResentToast from './components/ResentToast';

export default function DocumentList() {
  const selectedDocuments = useSelector(
    (state) => state.inbox.selectedDocuments
  );
  const [modalOpen, setModalOpen] = useState(false);
  const [moveToInboxMutation] = useMoveToInboxMutation();
  const viewedDocument = useSelector((state) => state.inbox.viewedDocument);
  const { setSelectedDocuments, setViewedDocument, addViewedDocId } =
    useInboxActions();
  const { data: member } = useGetMemberIdentityQuery();
  const documents = useSelector((state) => state.inbox.documents);
  const { isMovedTo } = useAnnotationValidation();
  const currentAction = useSelector((state) => state.inbox.currentAction);
  const isDocumentsLoading = useSelector(
    (state) => state.inbox.isDocumentsLoading
  );
  const hasDocs = documents && documents.length > 0;
  const virtuosoRef = useRef<VirtuosoHandle>(null);
  const { control } = useFormContext<FilterData>();
  const documentProcessStatus = useWatch({
    control,
    name: 'documentProcessStatus',
  });
  const setDocumentsRef = useSetDocumentsRef();
  const docViewRef = useDocViewRef();
  const [registerView] = useRegisterDocumentViewMutation();
  const { data: settings } = useGetMiscSettingsQuery();
  const refetchViewedDoc = useRefetchViewedDoc();
  const documentsObj = useSelector((state) => state.inbox.documentsObj);
  const { syncAnnotations } = useUpdateAnnotations();
  const isLoaded = useMemo(
    () =>
      Boolean(
        viewedDocument &&
          documentsObj[viewedDocument.sutureSignRequestId] &&
          documentsObj[viewedDocument.sutureSignRequestId] !== 'loading' &&
          currentAction !== 'saveForLater'
      ),
    [documentsObj, viewedDocument, currentAction]
  );

  useEffect(() => {
    if (virtuosoRef.current && setDocumentsRef) {
      setDocumentsRef(virtuosoRef.current);
    }
  }, [virtuosoRef.current, documents]);

  const moveToInbox = async (doc: DocumentSummaryItemType) => {
    await moveToInboxMutation({
      id: doc.sutureSignRequestId,
      documentProcessStatus,
    }).unwrap();
    refetchViewedDoc();
  };

  const renderCard = useCallback(
    (
      doc: DocumentSummaryItemType,
      index: number,
      cardProps?: Partial<CardProps>
    ) => {
      const daysDiff = dayjs().diff(dayjs(doc.submittedAt), 'days');
      const approvals = [...doc.approvals].sort(sortByApprovedAt);

      return (
        <Card
          savedTime={
            doc.memberBacklog?.closedAt
              ? undefined
              : {
                  time: doc.memberBacklog?.expiresAt || '',
                  isReturned: isAutomaticallyMovedDoc(doc),
                  dismissFunc: () => moveToInbox(doc),
                }
          }
          cardTitle={`${doc.patient.firstName}, ${doc.patient.lastName}`}
          supportingTitle={dayjs(doc.patient.birthdate).format('(MM/DD/YYYY)')}
          category={doc.template.templateType.shortName}
          subtitle={doc.submitterOrganization.name}
          cardStatus={`${dayjs(doc.submittedAt).format('MM/DD')} ${
            daysDiff > 0 ? `(${daysDiff} days)` : ''
          }`}
          detail={
            doc.signer.memberId !== member?.memberId
              ? fullNameWithSuffix({ ...doc.signer, withSuffix: true })
              : 'You'
          }
          approvals={approvals}
          currentUserId={member?.memberId}
          onClick={(e) => {
            if (isMovedTo !== null) {
              setModalOpen(true);
            } else {
              syncAnnotations(true);

              docViewRef?.scrollToIndex({
                align: 'start',
                index,
              });
              setViewedDocument(doc);
              e.stopPropagation();
            }
          }}
          statusDisplay={doc.statusDisplay}
          isSelected={
            viewedDocument?.sutureSignRequestId === doc.sutureSignRequestId
          }
          isRead={member && doc.viewedByMemberIds.includes(member.memberId)}
          timerCallback={() => {
            registerView(doc.sutureSignRequestId);
            addViewedDocId(doc.sutureSignRequestId);
          }}
          isLoaded={isLoaded}
          checkboxValue={doc.sutureSignRequestId.toString()}
          timerDelay={settings?.documentViewDuration}
          {...cardProps}
        />
      );
    },
    [
      docViewRef,
      member,
      viewedDocument,
      registerView,
      setViewedDocument,
      settings?.documentViewDuration,
      isLoaded,
      syncAnnotations,
    ]
  );

  if (isDocumentsLoading) {
    return (
      <VStack width="100%">
        {Array(30)
          .fill(null)
          .map((_, index) => (
            <Skeleton key={index} />
          ))}
      </VStack>
    );
  }

  return (
    <>
      <SaveModal onClose={() => setModalOpen(false)} isOpen={modalOpen} />
      <ResentToast isLoaded={isLoaded} />
      <VStack
        overflowY="auto"
        width="100%"
        alignItems="normal"
        height="100%"
        pb={0}
        spacing={0}
      >
        {!hasDocs && documentProcessStatus ? (
          <VStack
            overflowY="auto"
            width="100%"
            alignItems="normal"
            height="100%"
          >
            <Flex
              h="100%"
              justifyContent="center"
              alignItems="center"
              px={2}
              pt="8px"
              bg="gray.200"
              direction="column"
              color="gray.500"
            >
              <Text fontSize="lg">Nothing based on selected filters</Text>
              <InboxIconFA width="24px" mt="12px" />
            </Flex>
          </VStack>
        ) : (
          <CheckboxGroup
            onChange={(selectedDocs: string[]) => {
              if (
                selectedDocs[0] &&
                (!documentsObj[selectedDocs[0]] ||
                  documentsObj[selectedDocs[0]] === 'loading')
              ) {
                refetchViewedDoc(parseInt(selectedDocs[0], 10)); // we should fetch the selected document if it isn't in the "load" range to show the stack animation
              }
              setSelectedDocuments(selectedDocs);
            }}
            value={selectedDocuments}
          >
            <Virtuoso
              id="document-summaries-virtuoso-container"
              ref={virtuosoRef}
              style={{ height: '100%', scrollbarGutter: 'stable' }}
              data={documents}
              defaultItemHeight={72}
              itemContent={(index, doc) => renderCard(doc, index)}
              computeItemKey={(_, i) => i.sutureSignRequestId}
            />
          </CheckboxGroup>
        )}
      </VStack>
    </>
  );
}
