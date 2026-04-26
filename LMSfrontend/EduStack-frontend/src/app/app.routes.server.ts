import { RenderMode, ServerRoute } from '@angular/ssr';

export const serverRoutes: ServerRoute[] = [
  // Dynamic routes with parameters — render on the client
  { path: 'courses/:id', renderMode: RenderMode.Client },
  { path: 'instructor/courses/edit/:id', renderMode: RenderMode.Client },
  { path: 'instructor/courses/manage/:id', renderMode: RenderMode.Client },
  // All other routes — prerender as static pages
  {
    path: '**',
    renderMode: RenderMode.Prerender
  }
];
