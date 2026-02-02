/* eslint-disable @typescript-eslint/no-unused-expressions */
export const mixpanelEvent = (name: string, data: any) => {
  try {
    if ((window as any).mixpanel) {
      (window as any).mixpanel.track(name, data);
    }
  } catch (e) {
    console.log(e);
  }
};

export type SmartlookPayload = { name: string; id?: string; data?: any };

export const smartlookEvent = ({ name, id, data }: SmartlookPayload) => {
  try {
    if ((window as any).smartlook) {
      if (id) {
        (window as any).smartlook(name, id, data);
      } else {
        name === 'properties'
          ? (window as any).smartlook(name, data)
          : (window as any).smartlook('track', name, data);
      }
    }
  } catch (e) {
    console.log(e);
  }
};
