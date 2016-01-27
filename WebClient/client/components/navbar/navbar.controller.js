'use strict';

angular.module('kenseeApp')
  .controller('NavbarCtrl', function ($scope, $location, $cookieStore, AuthenticationService, $state) {
    $scope.menu = [
      {
        'title': 'Dashboard',
        'link': '/',
        'disabled': false
      },
      {
        'title': 'Articles',
        'link': '/articles',
        'disabled': false
      }
    ];

    var globals = $cookieStore.get('globals');
    if (globals === undefined)
    {
      $scope.loggedIn = false;
    }
    else
    {
      $scope.username = $cookieStore.get('globals').currentUser.username;
      $scope.noUsername = ($scope.usernmame === undefined);
      $scope.loggedIn = true;
    }

    $scope.isCollapsed = true;

    $scope.isActive = function(route) {
      return route === $location.path();
    };
    $scope.logout = function() {
      AuthenticationService.ClearCredentials();
      $state.go('login');
    };
  });
