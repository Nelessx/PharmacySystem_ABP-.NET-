import { RoutesService, eLayoutType } from '@abp/ng.core';
import { inject, provideAppInitializer } from '@angular/core';

export const APP_ROUTE_PROVIDER = [
  provideAppInitializer(() => {
    configureRoutes();
  }),
];

function configureRoutes() {
  const routes = inject(RoutesService);

  routes.add([
    {
      path: '/',
      name: '::Menu:Home',
      iconClass: 'fas fa-home',
      order: 1,
      layout: eLayoutType.application,
    },
    {
      path: '/categories',
      name: '::Menu:Categories',
      iconClass: 'fas fa-tags',
      order: 2,
      layout: eLayoutType.application,
      requiredPolicy: 'PharmacySystem.Categories',
    },
    {
      path: '/medicines',
      name: '::Menu:Medicines',
      iconClass: 'fas fa-capsules',
      order: 3,
      layout: eLayoutType.application,
      requiredPolicy: 'PharmacySystem.Medicines',
    },
  ]);
}