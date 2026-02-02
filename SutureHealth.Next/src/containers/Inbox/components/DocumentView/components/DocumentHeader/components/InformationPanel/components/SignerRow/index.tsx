import { useEffect, useMemo, useState } from 'react';
import {
  IconButton,
  Text,
  Flex,
  Skeleton,
  Tooltip,
  useToast,
} from '@chakra-ui/react';
import {
  Tag,
  TagLabel,
  UserEditIconFA,
  SignIconFA,
  TruncatedText,
  MenuWithAvatar,
  FileCheckIconFA,
  FileCircleCheckIconFA,
  Toast,
  Slider,
  AngleLeftIconFA,
  AngleRightIconFA,
} from 'suture-theme';
import {
  RequestDocumentType,
  MemberListItemType,
  MemberType,
} from '@utils/zodModels';
import { useSelector } from '@redux/hooks';
import {
  useGetOrganizationsQuery,
  useGetAssociatesQuery,
  useReassignDocumentMutation,
} from '@containers/Inbox/apiReducer';
import { useInboxActions } from '@containers/Inbox/localReducer';
import { skipToken } from '@reduxjs/toolkit/dist/query';
import useErrorToast from '@hooks/useErrorToast';
import useRefetchViewedDoc from '@hooks/useRefetchViewedDoc';
import fullNameWithSuffix from '@utils/fullNameWithSuffix';

interface Props {
  document: RequestDocumentType;
  openReassigndModal: () => void;
}

export default function SignerRow({
  document: {
    document: {
      signerOrganization,
      signer,
      collaborator,
      assistant,
      sutureSignRequestId,
    },
    template,
  },
  openReassigndModal,
}: Props) {
  const [selectedSigner, setSelectedSigner] = useState<
    MemberListItemType | undefined | null
  >(undefined);
  const [selectedCollaborator, setSelectedCollaborator] = useState<
    MemberListItemType | undefined | null
  >(undefined);
  const [selectedAssistant, setSelectedAssistant] = useState<
    MemberListItemType | undefined | null
  >(undefined);
  const { data: organizations } = useGetOrganizationsQuery();
  const [reassignDocument] = useReassignDocumentMutation();
  const toast = useToast({ render: Toast });
  const showErrorToast = useErrorToast();
  const refetchViewedDoc = useRefetchViewedDoc();
  const { setViewedDocument } = useInboxActions();
  const selectedOrganizationId = useSelector(
    (state) => state.mainLayout.selectedOrganizationId
  );
  const viewedDocument = useSelector((state) => state.inbox.viewedDocument);
  const { data: associates, isLoading } = useGetAssociatesQuery(
    sutureSignRequestId || skipToken
  );
  const signers = useMemo(() => {
    if (associates) {
      return associates?.reduce<MemberListItemType[]>(
        (acc, cur) => [...acc, cur.signer],
        []
      );
    }

    return [];
  }, [associates]);
  const collaborators = useMemo(() => {
    if (associates && selectedSigner) {
      return (
        associates
          .find((i) => i.signer.memberId === selectedSigner.memberId)
          ?.collaborators.filter(
            (i) => i.organizationId === signerOrganization.organizationId
          ) || []
      );
    }

    return [];
  }, [associates, signerOrganization, selectedSigner]);
  const assistants = useMemo(() => {
    if (associates && selectedSigner) {
      return (
        associates
          .find((i) => i.signer.memberId === selectedSigner.memberId)
          ?.assistants.filter(
            (i) => i.organizationId === signerOrganization.organizationId
          ) || []
      );
    }

    return [];
  }, [associates, signerOrganization, selectedSigner]);

  useEffect(() => {
    setSelectedSigner(signer);
  }, [signer?.memberId]);

  useEffect(() => {
    setSelectedCollaborator(collaborator);
  }, [collaborator?.memberId]);

  useEffect(() => {
    setSelectedAssistant(assistant);
  }, [assistant?.memberId]);

  const showReassignToast = () =>
    toast({
      title: 'You are unable to reassign this document type.',
      description:
        'If this document was sent to you in error, reject the document.',
      status: 'error',
      duration: 8000,
    });

  const reassignAssistantCollaborator = async (
    target: 'assistant' | 'collaborator',
    member: MemberListItemType | undefined
  ) => {
    try {
      const toastId = `${target}-${member?.memberId || sutureSignRequestId}`;
      const reassignedToastId = `${target}-reassigned-${toastId}`;
      const newAssigners =
        target === 'assistant'
          ? {
              assistant: (member as MemberType) || null,
            }
          : { collaborator: member || null };
      const prevAssigners = {
        assistant,
        collaborator,
      };

      if (target === 'assistant') {
        setSelectedAssistant(member);
      } else {
        setSelectedCollaborator(member);
      }

      toast({
        id: toastId,
        title: 'Loading...',
        status: 'loading',
        duration: null,
      });

      await reassignDocument({
        id: sutureSignRequestId,
        organizationId: signerOrganization.organizationId,
        signerMemberId: signer.memberId,
        assistantMemberId:
          target === 'assistant'
            ? newAssigners.assistant?.memberId || 0
            : undefined,
        collaboratorMemberId:
          target === 'collaborator'
            ? newAssigners.collaborator?.memberId || 0
            : undefined,
      }).unwrap();
      toast.close(toastId);
      setViewedDocument({
        ...viewedDocument!,
        ...newAssigners,
      });

      setTimeout(() => refetchViewedDoc(sutureSignRequestId), 2300);
      toast({
        id: reassignedToastId,
        title: member
          ? `${
              target === 'assistant' ? 'Assistant' : 'Collaborator'
            } Reassigned to ${fullNameWithSuffix({
              ...member,
              withSuffix: true,
            })}`
          : `${target === 'assistant' ? 'Assistant' : 'Collaborator'} removed`,
        status: 'success',
        duration: 2000,
        render: (props) => (
          <Toast
            {...props}
            buttonProps={{
              onClick: async () => {
                try {
                  toast.close(reassignedToastId);

                  if (target === 'assistant') {
                    setSelectedAssistant(prevAssigners.assistant);
                  } else {
                    setSelectedCollaborator(prevAssigners.collaborator);
                  }

                  toast({
                    id: toastId,
                    title: 'Loading...',
                    status: 'loading',
                    duration: null,
                  });

                  await reassignDocument({
                    id: sutureSignRequestId,
                    organizationId: signerOrganization.organizationId,
                    signerMemberId: signer.memberId,
                    assistantMemberId:
                      target === 'assistant'
                        ? prevAssigners.assistant?.memberId || 0
                        : undefined,
                    collaboratorMemberId:
                      target === 'collaborator'
                        ? prevAssigners.collaborator?.memberId || 0
                        : undefined,
                  });
                  toast.close(toastId);
                  setViewedDocument({
                    ...viewedDocument!,
                    ...prevAssigners,
                  });
                  refetchViewedDoc(sutureSignRequestId);

                  toast({
                    id: reassignedToastId,
                    title: `${
                      target === 'assistant' ? 'Assistant' : 'Collaborator'
                    } re-assignment reverted`,
                    status: 'success',
                    duration: 2000,
                  });
                } catch (error) {
                  toast.closeAll();
                  if (target === 'assistant') {
                    setSelectedAssistant(prevAssigners.assistant);
                  } else {
                    setSelectedCollaborator(prevAssigners.collaborator);
                  }
                  showErrorToast(error);
                }
              },
            }}
            buttonText="Undo"
          />
        ),
      });
    } catch (error) {
      toast.closeAll();
      if (target === 'assistant') {
        setSelectedAssistant(assistant);
      } else {
        setSelectedCollaborator(collaborator);
      }
      showErrorToast(error);
    }
  };

  return (
    <Flex direction="row" alignItems="center">
      <Text mr={1} color="gray.600" fontSize="14px">
        Signer
      </Text>
      {!selectedOrganizationId &&
        organizations?.organizationOptions?.length !== 1 && (
          <Tag
            variant="outline"
            color="blue.400"
            borderColor="blue.400"
            borderRadius="full"
            mr={1}
            minW="50px"
          >
            <TagLabel>
              <TruncatedText text={signerOrganization?.name} />
            </TagLabel>
          </Tag>
        )}
      <Slider
        spacing={4}
        height="30px"
        internalOffsetRight={30}
        prevIcon={<AngleLeftIconFA />}
        nextIcon={<AngleRightIconFA />}
        navigationBtnProps={{
          color: 'blackAlpha.600',
        }}
        minWidth={
          !isLoading
            ? `${
                150 +
                (collaborators.length ? 75 : 0) +
                (assistants.length ? 75 : 0)
              }px`
            : '300px'
        }
      >
        <Skeleton isLoaded={!isLoading} minW="100px">
          <MenuWithAvatar
            width="100%"
            options={signers}
            value={selectedSigner || undefined}
            onChange={async (i) => {
              if (i) {
                if (template.templateType.signerChangeAllowed) {
                  try {
                    setSelectedSigner(i);

                    toast({
                      id: i.memberId,
                      title: 'Loading...',
                      status: 'loading',
                      duration: null,
                    });

                    await reassignDocument({
                      id: sutureSignRequestId,
                      organizationId: signerOrganization.organizationId,
                      signerMemberId: i.memberId,
                    }).unwrap();
                    setViewedDocument({
                      ...viewedDocument!,
                      signer: i,
                    });
                    refetchViewedDoc(sutureSignRequestId);

                    toast.update(i.memberId, {
                      title: 'Document Assigned Successfully',
                      status: 'success',
                      duration: 2000,
                    });
                  } catch (error) {
                    toast.closeAll();
                    showErrorToast(error);
                    setSelectedSigner(signer);
                  }
                } else {
                  showReassignToast();
                }
              }
            }}
            selectedLabelFormat={(i) => (
              <Flex alignItems="center">
                <SignIconFA mr="4px" />
                <TruncatedText
                  text={fullNameWithSuffix({ ...i, withSuffix: true })}
                />
              </Flex>
            )}
            labelFormat={(i) => fullNameWithSuffix({ ...i, withSuffix: true })}
            tooltipLabel="Change Signer"
            withConfirm
            data-cy="signer-menu"
          />
        </Skeleton>
        <Skeleton
          isLoaded={!isLoading}
          minW="100px"
          display={!isLoading && collaborators.length === 0 ? 'none' : 'block'}
        >
          <MenuWithAvatar
            width="100%"
            options={collaborators}
            value={selectedCollaborator || undefined}
            onChange={(i) => {
              reassignAssistantCollaborator('collaborator', i);
            }}
            selectedLabelFormat={(i) => (
              <Flex alignItems="center" lineHeight="14px">
                <FileCheckIconFA mr="4px" />
                <TruncatedText
                  text={fullNameWithSuffix({ ...i, withSuffix: true })}
                />
              </Flex>
            )}
            labelFormat={(i) => fullNameWithSuffix({ ...i, withSuffix: true })}
            emptyText="Add Collaborator"
            colorScheme="teal"
            avatarBg="teal.300"
            tooltipLabel="Change Collaborator"
            data-cy="collaborator-menu"
            withNone
            withNoneConfirm
            approvals={viewedDocument?.approvals}
          />
        </Skeleton>
        <Skeleton
          isLoaded={!isLoading}
          minW="100px"
          display={!isLoading && assistants.length === 0 ? 'none' : 'block'}
        >
          <MenuWithAvatar
            width="100%"
            options={assistants}
            value={selectedAssistant || undefined}
            onChange={(i) => {
              reassignAssistantCollaborator('assistant', i);
            }}
            selectedLabelFormat={(i) => (
              <Flex alignItems="center" lineHeight="14px">
                <FileCircleCheckIconFA mr="4px" />
                <TruncatedText
                  text={fullNameWithSuffix({ ...i, withSuffix: true })}
                />
              </Flex>
            )}
            labelFormat={(i) => fullNameWithSuffix({ ...i, withSuffix: true })}
            emptyText="Add Assistant"
            colorScheme="gray"
            avatarBg="gray.500"
            tooltipLabel="Change Assistant"
            borderColor="gray.500"
            borderWidth="1px"
            background="none"
            color="gray.500"
            data-cy="assistant-menu"
            withNone
            withNoneConfirm
          />
        </Skeleton>
      </Slider>
      <Tooltip label="Reassign">
        <IconButton
          variant="ghost"
          size="xs"
          isRound
          aria-label="signature"
          icon={<UserEditIconFA fontSize="12px" />}
          onClick={openReassigndModal}
          data-cy="forward-open-btn"
          className="reassign-icon"
        />
      </Tooltip>
    </Flex>
  );
}
