'use strict';

angular.module('kenseeApp', [
  'ngCookies',
  'ngResource',
  'ngSanitize',
  'ui.router',
  'ui.bootstrap',
  'ui.grid',
  'ui.grid.selection',
  'ui.grid.resizeColumns',
  'ui.grid.autoResize',
  'ui.grid.exporter',
  'ui.grid.pagination',
  'checklist-model',
  'angularMoment',
  'datePicker',
  'duScroll',
  'ngAnimate',
  'cgBusy'
])
  .config(function ($stateProvider, $urlRouterProvider, $locationProvider) {
    $stateProvider
      .state('articles', {
        url: '/NewsRoom',
        controller: 'ArticlesCtrl',
        templateUrl: 'app/articles/articles.html'
      })

      .state('kensee', {
        url: '/Kensee',
        controller: 'KenseeCtrl',
        templateUrl: 'app/kensee/kensee.html'
      })

      .state('login', {
        url: '/login',
        controller: 'LoginCtrl',
        templateUrl: 'app/login/login.html'
      });

    $urlRouterProvider.otherwise('/login');

    $locationProvider.html5Mode(true);

  })
  //.constant('SERVER_URL','http://localhost:9000/')  //production
  .constant('SERVER_URL','http://50.22.216.6/KenseeAPI/api/')  //production
 // .constant('SERVER_URL',"http://158.85.229.36/KenseeAPI/api/")     // development server
//  .constant('SERVER_URL',"http://173.193.211.219/KenseeAPI/api/")     // testing
//  .constant('SERVER_URL', "http://localhost/KenseeAPI/api/")     // local
  .run(function ($rootScope, $location, $cookieStore, $http) {
    // keep user logged in after page refresh
    $rootScope.globals = $cookieStore.get('globals') || {};
    if ($rootScope.globals.currentUser) {
      //server need to support OPTIONS and CORS for this one - omit for now
      //$http.defaults.headers.common['Authorization'] = 'Basic ' + $rootScope.globals.currentUser.authdata; // jshint ignore:line
    }

    $rootScope.$on('$locationChangeStart', function (event, next, current) {
      // redirect to login page if not logged in
      if ($location.path() !== '/login' && !$rootScope.globals.currentUser) {
        $location.path('/login');
      }
    });

  });
