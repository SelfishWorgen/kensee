'use strict';

angular.module('kenseeApp')
  .controller('NavbarCtrl', function ($scope, $location, $cookieStore, AuthenticationService, $state, $rootScope) {
    $scope.menu = [
      {
        'title': 'Kensee',
        'link': '/Kensee',
        'disabled': false
      },
      {
        'title': 'Reports',
        'link': '',
        'disabled': false
      },
      {
        'title': 'NewsRoom',
        'link': '/NewsRoom',
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
    $scope.checkReports = function(link){
      if(link == ''){
        $rootScope.showReports = true;
      }
      if($location.path() == link){
        $rootScope.showReports = false;
        event.preventDefault();
        event.stopPropagation();
        return false;
      }
    };
    $scope.logout = function() {
      AuthenticationService.ClearCredentials();
      $state.go('login');
    };
  });
