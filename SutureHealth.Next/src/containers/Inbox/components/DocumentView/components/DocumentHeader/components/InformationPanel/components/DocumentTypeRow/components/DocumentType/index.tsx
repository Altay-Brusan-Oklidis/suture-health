import { useEffect, useMemo, useState } from 'react';
import {
  IconButton,
  Tooltip,
  Menu,
  Button,
  MenuList,
  MenuItemOption,
  MenuButton,
  MenuOptionGroup,
  useToast,
  Text,
  Box,
  Portal,
  StyleProps,
} from '@chakra-ui/react';
import { ChevronDownIcon } from '@chakra-ui/icons';
import {
  EditDocumentIconFA,
  groupBy,
  Toast,
  CloseIconFA,
  TruncatedText,
} from 'suture-theme';
import { useSelector } from '@redux/hooks';
import { useInboxActions } from '@containers/Inbox/localReducer';
import {
  useLazyGetTemplateTypesQuery,
  useUpdateDocumentTemplateTypeMutation,
} from '@containers/Inbox/apiReducer';
import { TemplateTypesDataType } from '@utils/zodModels';
import useRefetchViewedDoc from '@hooks/useRefetchViewedDoc';
import isFaceToFaceDoc from '@utils/isFaceToFaceDoc';

interface Props extends StyleProps {
  templateType: TemplateTypesDataType[number];
}

export default function DocumentType({ templateType, ...rest }: Props) {
  const [isEditing, setIsEditing] = useState<boolean>(false);
  const [localTemplateType, setLocalTemplateType] =
    useState<TemplateTypesDataType[number]>(templateType);
  const viewedDocument = useSelector((state) => state.inbox.viewedDocument);
  const [fetchTemplateTypes, { data: templateTypes }] =
    useLazyGetTemplateTypesQuery();
  const [updateTemplateType] = useUpdateDocumentTemplateTypeMutation();
  const { setViewedDocument } = useInboxActions();
  const toast = useToast({ render: Toast });
  const refetchViewedDoc = useRefetchViewedDoc();
  const isFaceToFaceDocument = isFaceToFaceDoc(viewedDocument);

  const groupedTemplateTypes = useMemo(() => {
    if (templateTypes) return groupBy(templateTypes, 'category');

    return [];
  }, [templateTypes]);

  useEffect(() => {
    setLocalTemplateType(templateType);
  }, [templateType]);

  const onChooseTeamplateType = async (
    template: TemplateTypesDataType[number]
  ) => {
    try {
      if (viewedDocument?.sutureSignRequestId) {
        setLocalTemplateType(template);
        setViewedDocument({
          ...viewedDocument,
          template: {
            ...viewedDocument.template,
            templateType: template,
          },
        });
        setIsEditing(false);

        await updateTemplateType({
          id: viewedDocument.sutureSignRequestId,
          templateTypeId: template.templateTypeId,
        }).unwrap();
        refetchViewedDoc();
      }
    } catch (error) {
      setLocalTemplateType(templateType);
      toast({
        title: 'Error',
        description: 'Error changing document type',
        status: 'error',
        duration: 5000,
        isClosable: true,
      });
    }
  };

  return (
    <Box display="flex" alignItems="center" maxW="100%" minW="100px">
      {isEditing ? (
        <Menu>
          <MenuButton
            as={Button}
            rightIcon={<ChevronDownIcon />}
            size="sm"
            variant="outline"
            colorScheme="gray"
            color="gray.700"
            fontWeight="normal"
            data-cy="template-select"
            {...rest}
          >
            <Text whiteSpace="nowrap" overflow="hidden" textOverflow="ellipsis">
              {localTemplateType.name}
            </Text>
          </MenuButton>
          <Portal>
            <MenuList
              maxH="300px"
              overflow="auto"
              data-cy="template-list"
              zIndex="modal"
            >
              {Object.entries(groupedTemplateTypes).map(([title, types]) => (
                <MenuOptionGroup
                  title={title}
                  key={title}
                  value={localTemplateType.templateTypeId.toString()}
                >
                  {types.map((i) => (
                    <MenuItemOption
                      key={i.templateTypeId}
                      onClick={() => onChooseTeamplateType(i)}
                      value={i.templateTypeId.toString()}
                      cursor="pointer"
                    >
                      {i.name}
                    </MenuItemOption>
                  ))}
                </MenuOptionGroup>
              ))}
            </MenuList>
          </Portal>
        </Menu>
      ) : (
        <TruncatedText
          text={localTemplateType.name}
          fontWeight={700}
          color="gray.700"
          fontSize="20px"
          lineHeight="120%"
          data-cy="template-type"
          {...rest}
        />
      )}
      {!isFaceToFaceDocument ? (
        isEditing ? (
          <IconButton
            variant="ghost"
            size="xs"
            color="gray.500"
            isRound
            colorScheme="gray"
            ml={1}
            aria-label="signature"
            icon={<CloseIconFA fontSize="12px" />}
            onClick={() => setIsEditing(false)}
            data-cy="close-template-type"
          />
        ) : (
          <Tooltip label="Change Document Type">
            <IconButton
              variant="ghost"
              size="xs"
              color="gray.500"
              isRound
              colorScheme="gray"
              ml={1}
              aria-label="signature"
              icon={<EditDocumentIconFA fontSize="12px" />}
              onClick={() => setIsEditing(true)}
              onPointerMove={() => !templateTypes && fetchTemplateTypes()}
              data-cy="edit-template-type"
            />
          </Tooltip>
        )
      ) : null}
    </Box>
  );
}
