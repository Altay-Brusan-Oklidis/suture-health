export const NewUrls = {
  INBOX: '/request/sign',
} as const;

export const OldUrls = {
  ACCOUNT: '/account',
  HISTORY: '/request/history',
  COMPANY: '/company',
  LOGOUT: '/identity/account/logout',
  NETWORK: '/network',
  REVENUE: '/revenue',
  TEAM: '/team',
  VIDEO: '/visit',
  SEND: '/request/send',
} as const;

export const ApplicationUrls = {
  ...NewUrls,
  ...OldUrls,
} as const;

export const IframeUrls = {
  SEND: '/request/send-iframe',
  HISTORY: '/request/history-iframe',
  NETWORK: '/network-iframe',
  VIDEO: '/visit-iframe',
  REVENUE: '/revenue-iframe',
} as const;

export const UserRole = {
  SENDER: 'Sender',
  SIGNER: 'Signer',
  COLLABORATOR: 'Collaborator',
  COLLAB_SIGNER: 'CollaboratorSigner',
  ASSISTANT: 'Assistant',
} as const;

type ObjectValues<T> = T[keyof T];

export type UrlValues = ObjectValues<typeof ApplicationUrls>;
export type IframeUrlValues = ObjectValues<typeof IframeUrls>;
export type UserRoleValues = ObjectValues<typeof UserRole>;

export const BASE_SCALE = 1;
export const BASE_SCALE_RECOVERY = 1 / BASE_SCALE;

export const STACK_ANIM_DURATION = 400; // 100ms + transitionDuration of the ChakraBox in the ImageViewer
export const FADE_OUT_ANIM_DURATION = 600; // 100ms + transitionDuration of the ChakraBox in the ImageViewer
