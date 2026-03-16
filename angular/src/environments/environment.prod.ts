import { Environment } from '@abp/ng.core';

const baseUrl = 'http://localhost:4200';

const oAuthConfig = {
  issuer: 'https://localhost:44378/',
  redirectUri: baseUrl,
  clientId: 'PharmacySystem_App',
  responseType: 'code',
  scope: 'offline_access PharmacySystem',
  requireHttps: true,
};

export const environment = {
  production: true,
  application: {
    baseUrl,
    name: 'PharmacySystem',
  },
  oAuthConfig,
  apis: {
    default: {
      url: 'https://localhost:44378',
      rootNamespace: 'PharmacySystem',
    },
    AbpAccountPublic: {
      url: oAuthConfig.issuer,
      rootNamespace: 'AbpAccountPublic',
    },
  },
  remoteEnv: {
    url: '/getEnvConfig',
    mergeStrategy: 'deepmerge'
  }
} as Environment;
