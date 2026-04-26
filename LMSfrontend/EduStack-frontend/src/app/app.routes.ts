import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () => import('./features/landing/landing').then(m => m.Landing),
    pathMatch: 'full',
  },
  {
    path: 'auth',
    children: [
      { path: 'login', loadComponent: () => import('./features/auth/login/login').then(m => m.Login) },
      { path: 'register', loadComponent: () => import('./features/auth/register/register').then(m => m.Register) },
      { path: 'forgot-password', loadComponent: () => import('./features/auth/forgot-password/forgot-password').then(m => m.ForgotPassword) },
      { path: 'reset-password', loadComponent: () => import('./features/auth/reset-password/reset-password').then(m => m.ResetPassword) },
    ]
  },
  {
    path: 'courses',
    children: [
      { path: '', loadComponent: () => import('./features/courses/course-catalog/course-catalog').then(m => m.CourseCatalog), pathMatch: 'full' },
      { path: ':id', loadComponent: () => import('./features/courses/course-detail/course-detail').then(m => m.CourseDetail) },
    ]
  },
  {
    path: 'instructor',
    children: [
      { path: 'my-courses', loadComponent: () => import('./features/courses/my-courses/my-courses').then(m => m.MyCourses) },
      { path: 'courses/new', loadComponent: () => import('./features/courses/course-form/course-form').then(m => m.CourseForm) },
      { path: 'courses/edit/:id', loadComponent: () => import('./features/courses/course-form/course-form').then(m => m.CourseForm) },
      { path: 'courses/manage/:id', loadComponent: () => import('./features/courses/course-manage/course-manage').then(m => m.CourseManage) },
    ]
  },
  {
    path: 'student',
    children: [
      { path: 'my-learning', loadComponent: () => import('./features/student/my-learning/my-learning').then(m => m.MyLearning) },
      { path: 'learning/:enrollmentId', loadComponent: () => import('./features/student/course-player/course-player').then(m => m.CoursePlayer) },
    ]
  },
];
