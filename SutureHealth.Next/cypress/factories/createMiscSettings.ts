import { MiscSettings } from '@containers/Inbox/apiReducer';
import merge from 'lodash.merge';

export default function createMiscSettings(
  override?: PartialDeep<MiscSettings>
) {
  const settings: MiscSettings = {
    allowOtherToSignFromScreen: true,
    documentViewDuration: 2000,
    inboxPreference: 0,
    lastSavedForLaterDate: '',
    showNewInbox: true,
  };

  return merge(settings, override);
}
