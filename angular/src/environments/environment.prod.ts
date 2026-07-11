import { Environment } from '@abp/ng.core';

// In production the SPA serves itself from its own origin, so derive baseUrl
// from the browser instead of hardcoding localhost. The API/OAuth URLs below
// are placeholders: the authoritative values are pulled at runtime from the
// server's `/getEnvConfig` endpoint (see `remoteEnv` with deepmerge). Replace
// the fallbacks below, or configure /getEnvConfig, for your deployment.
const baseUrl =
  typeof window !== 'undefined' && window.location?.origin
    ? window.location.origin
    : 'http://localhost:4200';

const apiUrl = 'https://localhost:44378';

const oAuthConfig = {
  issuer: `${apiUrl}/`,
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
      url: apiUrl,
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
