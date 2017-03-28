'use strict';
app.controller('scorecardController', ['$scope', '$rootScope', '$location', 'validationService', 'ngAuthSettings', 'localStorageService', function ($scope, $rootScope, $location, validationService, ngAuthSettings, localStorageService) {

    $scope.lvid = localStorageService.get('viID');

    validationService.validateUser($scope.lvid).then(function (response) {

        $scope.kids = localStorageService.get('kids');
        $scope.rank = localStorageService.get('rank');
        var startDate = localStorageService.get('startDate');
        var ms = new Date(startDate).getTime() + (86400000 * 7);
        
        var expDate = new Date(ms);
        var nowDate = Date.now();

        $scope.expDate = expDate.toString();

        $scope.nowDate = function () {
            //return $scope.expDate < Date.now();
            return ms < nowDate;
        }

        $scope.kidCount = function () {
            return $scope.kids;
        }

    },
    function (err) {
        $scope.message = err.error_description;
    });

    $rootScope.special = $location.path();
    
}]);