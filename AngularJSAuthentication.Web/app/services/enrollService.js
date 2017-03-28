'use strict';
app.factory('enrollService', ['$http', '$q', 'localStorageService', 'ngAuthSettings', function ($http, $q, localStorageService, ngAuthSettings) {

    var serviceBase = ngAuthSettings.apiServiceBaseUri;

    var enrollServiceFactory = {};

    //var viid = localStorageService.get('viid');

    var _enrollUser = function (enrollData) {

        var deferred = $q.defer();

        var data = JSON.stringify(enrollData);

        $http.post(serviceBase + 'api/Customer/Enroll', enrollData).success(function (response) {
        //return $http.post(serviceBase + 'api/Customer/', data, { headers: { 'Content-Type': 'application/x-www-form-urlencoded' } }).success(function (response) {
            deferred.resolve(response);

        }).error(function (err, status) {
            _logOut();
            deferred.reject(err);
        });

        return deferred.promise;
    };

    var _holdPassdown = function (passdownData) {

        var deferred = $q.defer();

        var data = JSON.stringify(passdownData);

        $http.post(serviceBase + 'api/Customer/Put', passdownData).success(function (response) {
            //return $http.post(serviceBase + 'api/Customer/', data, { headers: { 'Content-Type': 'application/x-www-form-urlencoded' } }).success(function (response) {
            deferred.resolve(response);

        }).error(function (err, status) {
            _logOut();
            deferred.reject(err);
        });

        return deferred.promise;
    };

    enrollServiceFactory.enrollUser = _enrollUser;
    enrollServiceFactory.holdPassdown = _holdPassdown;

    return enrollServiceFactory;

}]);