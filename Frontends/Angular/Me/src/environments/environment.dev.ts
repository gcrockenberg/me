import { InteractionType } from "@azure/msal-browser";
import { IEnvironment, AuthRequestType } from "./interfaces/IEnvironment";

export const environment: IEnvironment = {
  production: false,
  stripePublishableKey: 'pk_test_51NYuSRA1eK3epo4cHtiKhkZJpfXpAPWqrIwa2EfqBw2m7GUz1hc6UvOFagLiwxvSKugdtQiUS62gUHDvzBOCTVEF00o5LaW9EH',
  apiConfigs: [
    {
      uri: 'http://localhost/o/api/v1/order*',
      scopes: [
        'https://meauth.onmicrosoft.com/cart/cart.read',
        'https://meauth.onmicrosoft.com/cart/cart.write'
      ]
    }
  ],
  b2cPolicies: {
    names: {
      signUpSignIn: 'B2C_1_susi_v2',
      resetPassword: 'B2C_1_reset_v3',
      editProfile: 'B2C_1_edit_profile_v2',
    },
    authorities: {
      signUpSignIn: {
        authority:
          'https://meauth.b2clogin.com/meauth.onmicrosoft.com/b2c_1_susi_v2',
      },
      resetPassword: {
        authority:
          'https://meauth.b2clogin.com/meauth.onmicrosoft.com/B2C_1_reset_v3',
      },
      editProfile: {
        authority:
          'https://meauth.b2clogin.com/meauth.onmicrosoft.com/b2c_1_edit_profile_v2',
      },
    },
    authorityDomain: 'meauth.b2clogin.com',
  },
  msalGuardConfig: {
    // Popup detects when user cancels the flow, Redirect does not which leaves inconsistent state.
    interactionType: InteractionType.Redirect,
    authRequest: AuthRequestType.SilentRequest
  }
};