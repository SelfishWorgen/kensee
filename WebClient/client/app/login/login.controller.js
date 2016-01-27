'use strict';

angular.module('kenseeApp')
  .controller('LoginCtrl', function ($scope, $location, AuthenticationService) {
    $scope.showError = false;

    $scope.login = function() {
      $scope.showError = false;
      AuthenticationService.Login($scope.username, $scope.password, function (response) {
        if (response) {
          AuthenticationService.SetCredentials($scope.username, $scope.password);
          $location.path('/');
        } else {
          $scope.showError = true;
        }
      });
    };
  });
