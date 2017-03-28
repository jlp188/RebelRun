'use strict';
app.factory('validationService', ['$http', '$q', 'localStorageService', 'ngAuthSettings', function ($http, $q, localStorageService, ngAuthSettings) {

    var serviceBase = ngAuthSettings.apiServiceBaseUri;

    var validationServiceFactory = {};

    var viid = localStorageService.get('viID');

    var _validateUser = function (lvid) {

        //return $http.get(serviceBase + 'api/Customer/' + viid).then(function (results) {
        //    return results;
        //});
        var deferred = $q.defer();

        $http.get(serviceBase + 'api/Customer/' + lvid.toString()).success(function (data) {
            //alert("Added Successfully!!");

            //$http.post(serviceBase + 'api/Customer/Enroll', this.enrollData).success(function (data) {
            //        alert("Added Successfully!!");
            var custData = data[0];
            //var exmp = $scope.custData.name;
            //localStorageService.set('customerData', { passdownID: $scope.custData.id, startDate: $scope.custData.startDate.toString(), kids: $scope.custData.kids.toString() });

            localStorageService.set('passdownID', custData.id);
            localStorageService.set('rank', custData.rank);//, startDate: $scope.custData.startDate.toString(), kids: $scope.custData.kids.toString() });
            localStorageService.set('startDate', custData.startDate.toString());//, kids: $scope.custData.kids.toString() });
            if (custData.kids === null) {
                localStorageService.set('kids', "0");
            } else {
                localStorageService.set('kids', custData.kids.toString());
            }
            


            //$location.path('/scorecard');

            deferred.resolve(data); //response

        }).error(function (data) {
            $scope.error = "An Error has occured while Adding Customer! " + data;
            alert(data.message.toString());

        });

        return deferred.promise;
    };

    validationServiceFactory.validateUser = _validateUser;

    return validationServiceFactory;

}]);