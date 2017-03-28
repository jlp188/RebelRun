'use strict';
app.controller('indexController', ['$scope', '$rootScope', '$location', 'authService', 'localStorageService', function ($scope, $rootScope, $location, authService, localStorageService) {

    $scope.logOut = function () {
        authService.logOut();
        $location.path('/scorecard');
    };

    $scope.authentication = authService.authentication;

    $scope.viUID = authService.authentication.vuName; //viUID; // localStorageService.get('viID');

    //$scope.isActive = function (viewLocation) {
    //    var active = ("'" + viewLocation + "'" === $location.path());
    //    return active;
    //};
    
    //$rootScope.special = $location.path();

}]);