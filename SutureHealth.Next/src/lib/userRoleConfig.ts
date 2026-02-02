import {
  ApplicationUrls,
  IframeUrlValues,
  IframeUrls,
  UrlValues,
  UserRole,
  UserRoleValues,
} from '@lib/constants';

export default function pathConfig(role: UserRoleValues | undefined) {
  if (!role) {
    return undefined;
  }

  const senderPathConfig = {
    navigation: new Map<UrlValues, string>([
      [ApplicationUrls.SEND, 'SEND'],
      [ApplicationUrls.HISTORY, 'TRACK'],
      [ApplicationUrls.VIDEO, 'VISIT'],
    ]),
    iframe: Array<IframeUrlValues>(
      IframeUrls.SEND,
      IframeUrls.HISTORY,
      IframeUrls.VIDEO
    ),
    default: ApplicationUrls.SEND,
  } as const;

  const signerPathConfig = {
    navigation: new Map<UrlValues, string>([
      [ApplicationUrls.INBOX, 'INBOX'],
      [ApplicationUrls.HISTORY, 'ARCHIVE'],
      [ApplicationUrls.NETWORK, 'NETWORK'],
      [ApplicationUrls.REVENUE, 'REVENUE'],
      [ApplicationUrls.VIDEO, 'VISIT'],
    ]),
    iframe: Array<IframeUrlValues>(
      IframeUrls.HISTORY,
      IframeUrls.NETWORK,
      IframeUrls.REVENUE,
      IframeUrls.VIDEO
    ),
    default: ApplicationUrls.INBOX,
  } as const;

  const roleToConfig = () => {
    switch (role) {
      case UserRole.SENDER:
        return senderPathConfig;
      case UserRole.SIGNER:
      case UserRole.ASSISTANT:
      case UserRole.COLLABORATOR:
      case UserRole.COLLAB_SIGNER:
        return signerPathConfig;
      default:
        return undefined;
    }
  };

  return roleToConfig();
}
