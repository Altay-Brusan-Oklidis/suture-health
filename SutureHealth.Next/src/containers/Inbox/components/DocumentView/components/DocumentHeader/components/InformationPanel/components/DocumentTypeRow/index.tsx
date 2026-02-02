import { Flex } from '@chakra-ui/react';
import { RequestDocumentType } from '@utils/zodModels';
import DocumentType from './components/DocumentType';
import DocumentTags from './components/DocumentTags';
import ReceivedDate from './components/ReceivedDate';
import History from './components/History';

interface Props {
  document: RequestDocumentType;
}

export default function DocumentTypeRow({ document: { document } }: Props) {
  return (
    <Flex direction="row" alignItems="center">
      <DocumentType templateType={document.template.templateType} />
      <DocumentTags document={document} />
      <ReceivedDate
        statusDisplay={document.statusDisplay}
        submittedAt={document.submittedAt}
        sutureSignRequestId={document.sutureSignRequestId}
      />
      <History />
    </Flex>
  );
}
