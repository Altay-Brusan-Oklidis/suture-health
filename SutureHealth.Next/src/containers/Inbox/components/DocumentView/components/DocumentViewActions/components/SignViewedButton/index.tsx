import { useEffect, useMemo, useState, useRef, useCallback } from 'react';
import {
  Badge,
  SignIconFA,
  CircularProgress,
  CheckCircleIcon,
} from 'suture-theme';
import { Box, Button, Modal, ModalOverlay } from '@chakra-ui/react';
import {
  useGetMemberIdentityQuery,
  useGetMiscSettingsQuery,
} from '@containers/Inbox/apiReducer';
import { useSelector } from '@redux/hooks';
import { useInboxActions } from '@containers/Inbox/localReducer';
import useMemberRole from '@hooks/useMemberRole';
import useUpdateAnnotations from '@hooks/useUpdateAnnotations';
import isSavedForLaterDoc from '@utils/isSavedForLaterDoc';
import { DocumentSummaryItemType } from '@utils/zodModels';
import { STACK_ANIM_DURATION } from '@lib/constants';
import BulkModal from './components/BulkModal';

export default function SignViewedButton() {
  const [modalVisible, setModalVisible] = useState<boolean>(false);
  const [loaderVisible, setLoaderVisible] = useState<boolean>(false);
  const documents = useSelector((state) => state.inbox.documents);
  const { setViewedDocIds, setStackAnimation, setTotalViewedDocsLength } =
    useInboxActions();
  const viewedDocIds = useSelector((state) => state.inbox.viewedDocIds);
  const { data: member } = useGetMemberIdentityQuery();
  const viewedDocument = useSelector((state) => state.inbox.viewedDocument);
  const totalViewedDocs = useSelector(
    (state) => state.inbox.totalViewedDocsLength
  );
  const timerId = useRef<NodeJS.Timeout>();
  const { isSigner, isCollaboratorSigner, isCollaborator, isAssistant } =
    useMemberRole();
  const { data: settings } = useGetMiscSettingsQuery();
  const documentsObj = useSelector((state) => state.inbox.documentsObj);
  const { syncAnnotations } = useUpdateAnnotations();
  const isViewedDocLoaded = useMemo(
    () =>
      Boolean(
        viewedDocument &&
          documentsObj[viewedDocument.sutureSignRequestId] &&
          documentsObj[viewedDocument.sutureSignRequestId] !== 'loading'
      ),
    [documentsObj, viewedDocument]
  );
  const viewedDocs = useMemo(() => {
    if (documents) {
      return documents.filter((i) =>
        viewedDocIds.includes(i.sutureSignRequestId)
      );
    }

    return [];
  }, [documents, viewedDocIds]);
  const isApprovableUser = useMemo(
    () => isAssistant || isCollaborator || isCollaboratorSigner,
    [isAssistant, isCollaborator, isCollaboratorSigner]
  );
  const isSignableUser = useMemo(
    () => isSigner || isCollaboratorSigner,
    [isSigner, isCollaboratorSigner]
  );
  const isReassignableUser = useMemo(
    () => isSigner || isCollaboratorSigner,
    [isSigner, isCollaboratorSigner]
  );
  const isOtherSignableUser = useMemo(
    () =>
      isSigner || isCollaboratorSigner || settings?.allowOtherToSignFromScreen,
    [isSigner, isCollaboratorSigner, settings?.allowOtherToSignFromScreen]
  );

  const isApprovableDoc = useCallback(
    (doc: DocumentSummaryItemType) =>
      member &&
      doc.signer.memberId !== member.memberId &&
      !doc.approvals.find(
        (approval) => approval.approver.memberId === member.memberId
      ),
    [member]
  );

  const isSignableDoc = useCallback(
    (doc: DocumentSummaryItemType) =>
      member &&
      doc.signer.memberId === member.memberId &&
      !doc.isIncomplete &&
      !isSavedForLaterDoc(doc),
    [member]
  );

  const isReassignableDoc = useCallback(
    (doc: DocumentSummaryItemType) =>
      member &&
      doc.signer.memberId !== member.memberId &&
      doc.template.templateType.signerChangeAllowed &&
      !doc.isIncomplete,
    [member]
  );

  const isOtherSignableDoc = useCallback(
    (doc: DocumentSummaryItemType) =>
      member && doc.signer.memberId !== member.memberId && !doc.isIncomplete,
    [member]
  );

  const approvableDocs = useMemo(() => {
    if (viewedDocs.length && isApprovableUser) {
      return viewedDocs.filter(isApprovableDoc);
    }

    return [];
  }, [viewedDocs, isApprovableUser, isApprovableDoc]);
  const signableDocs = useMemo(() => {
    if (viewedDocs.length && isSignableUser) {
      return viewedDocs.filter(isSignableDoc);
    }

    return [];
  }, [viewedDocs, isSignableUser, isSignableDoc]);
  const reassignableDocs = useMemo(() => {
    if (viewedDocs.length && isReassignableUser) {
      return viewedDocs.filter(isReassignableDoc);
    }

    return [];
  }, [viewedDocs, isReassignableUser, isReassignableDoc]);
  const otherSignableDocs = useMemo(() => {
    if (viewedDocs.length && isOtherSignableUser) {
      return viewedDocs.filter(isOtherSignableDoc);
    }

    return [];
  }, [viewedDocs, isOtherSignableUser, isOtherSignableDoc]);

  const totalViewedDocsLength = useMemo(
    () =>
      [
        ...new Set([
          ...approvableDocs.map((i) => i.sutureSignRequestId),
          ...signableDocs.map((i) => i.sutureSignRequestId),
          ...reassignableDocs.map((i) => i.sutureSignRequestId),
          ...otherSignableDocs.map((i) => i.sutureSignRequestId),
        ]),
      ].length,
    [approvableDocs, signableDocs, reassignableDocs, otherSignableDocs]
  );

  useEffect(() => {
    if (totalViewedDocs !== totalViewedDocsLength) {
      setTotalViewedDocsLength(totalViewedDocsLength);
    }
  }, [totalViewedDocs, totalViewedDocsLength, setTotalViewedDocsLength]);

  const noOtherSigners = useMemo(() => {
    if (viewedDocs.length && member) {
      return !viewedDocs.some((i) => i.signer.memberId !== member.memberId);
    }

    return true;
  }, [viewedDocs, member]);
  const btnText = useMemo(() => {
    if (documents) {
      const defaultText = 'Bulk Sign';

      if (isSigner) {
        return defaultText;
      }
      if (isCollaboratorSigner) {
        return noOtherSigners ? defaultText : `${defaultText} | Approve`;
      }
      if (isCollaborator || isAssistant) {
        return `Bulk Approve${
          settings?.allowOtherToSignFromScreen ? ' | Sign' : ''
        }`;
      }
    }

    return '';
  }, [
    isSigner,
    isCollaboratorSigner,
    isCollaborator,
    isAssistant,
    settings?.allowOtherToSignFromScreen,
    documents,
    noOtherSigners,
  ]);

  useEffect(() => {
    if (documents && documents.length > 0 && member) {
      const viewedDocsIds = documents
        .filter((i) => i.viewedByMemberIds.includes(member.memberId))
        .map((i) => i.sutureSignRequestId);

      if (viewedDocsIds.length > 0) {
        setViewedDocIds(viewedDocsIds);
      }
    }
  }, [documents, member, setViewedDocIds]);

  const stopLoader = () => {
    clearTimeout(timerId.current);
    timerId.current = undefined;
    setLoaderVisible(false);
  };

  useEffect(() => {
    stopLoader();
  }, [viewedDocument?.sutureSignRequestId]);

  useEffect(() => {
    if (
      isViewedDocLoaded &&
      !loaderVisible &&
      viewedDocument &&
      ![
        ...approvableDocs,
        ...signableDocs,
        ...reassignableDocs,
        ...otherSignableDocs,
      ].find(
        (i) => i.sutureSignRequestId === viewedDocument.sutureSignRequestId
      ) &&
      ((isSignableUser && isSignableDoc(viewedDocument)) ||
        (isApprovableUser && isApprovableDoc(viewedDocument)) ||
        (isReassignableUser && isReassignableDoc(viewedDocument)) ||
        (isOtherSignableUser && isOtherSignableDoc(viewedDocument)))
    ) {
      setLoaderVisible(true);

      timerId.current = setTimeout(() => {
        setLoaderVisible(false);
      }, settings?.documentViewDuration || 2000);
    }
  }, [
    isViewedDocLoaded,
    viewedDocument,
    approvableDocs,
    signableDocs,
    reassignableDocs,
    otherSignableDocs,
    settings?.documentViewDuration,
    settings?.allowOtherToSignFromScreen,
    isSignableUser,
    isSignableDoc,
    isApprovableUser,
    isApprovableDoc,
    isReassignableUser,
    isReassignableDoc,
    isOtherSignableDoc,
    isOtherSignableUser,
  ]);

  const onModalClose = () => {
    setStackAnimation(undefined);
    setModalVisible(false);
  };

  return (
    <>
      <Modal isOpen={modalVisible} onClose={onModalClose} size="xl" isCentered>
        <ModalOverlay />
        <BulkModal
          onModalClose={onModalClose}
          title={btnText}
          approvableDocs={approvableDocs}
          signableDocs={signableDocs}
          reassignableDocs={reassignableDocs}
          otherSignableDocs={otherSignableDocs}
        />
      </Modal>
      {btnText && documents && documents.length > 0 && (
        <Box
          ml="16px"
          p="12px"
          borderRadius="10px"
          backdropFilter="blur(1px)"
          background="rgba(247, 250, 252, 0.85)"
          boxShadow="0px 4px 4px rgba(0, 0, 0, 0.25), -6px 5px 6px -1px rgba(0, 0, 0, 0.1);
                  backdrop-filter: blur(1px)"
        >
          <Button
            isDisabled={totalViewedDocsLength === 0 || !viewedDocument}
            leftIcon={
              isAssistant || isCollaborator ? (
                <CheckCircleIcon checkColor="teal.500" />
              ) : (
                <SignIconFA />
              )
            }
            rightIcon={
              <Box position="relative">
                <Badge
                  count={totalViewedDocsLength}
                  isActive
                  isEllipse
                  activeColors={{
                    bg: 'white',
                    color:
                      isAssistant || isCollaborator ? 'teal.500' : 'blue.500',
                  }}
                  ml={0}
                  data-cy="viewed-docs-count"
                  max={99}
                  display="flex"
                  alignItems="center"
                  justifyContent="center"
                  height="24px"
                  width="24px"
                  fontSize={totalViewedDocsLength > 99 ? '10px' : '13px'}
                  showZero
                />
                {loaderVisible && (
                  <CircularProgress
                    percent={100}
                    animateOnMount
                    duration={settings?.documentViewDuration}
                    position="absolute"
                    strokeWidth={6}
                    trailWidth={6}
                    top="50%"
                    left="50%"
                    transform="translate(-50%, -50%)"
                  />
                )}
              </Box>
            }
            onClick={() => {
              setStackAnimation('fanning');
              setTimeout(() => setModalVisible(true), STACK_ANIM_DURATION);
              syncAnnotations(false);
            }}
            colorScheme={isAssistant || isCollaborator ? 'teal' : 'blue'}
            data-cy="sign-all-btn"
          >
            {btnText}
          </Button>
        </Box>
      )}
    </>
  );
}
