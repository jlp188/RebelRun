'use strict';
app.controller('enrollController', ['$scope', '$rootScope', '$location', 'localStorageService', 'enrollService', function ($scope, $rootScope, $location, localStorageService, enrollService) {
    
    //var serviceBase = ngAuthSettings.apiServiceBaseUri;

    var passdown = localStorageService.get('passdownID');
    var prntID = localStorageService.get('viID');
    var heldPassDown = localStorageService.get('heldPassdownID');

    var passdownData = Object;

    if (!heldPassDown) {
        localStorageService.set('heldPassdownID', passdown);
    } else {
        passdown = heldPassDown;
    }

    $(document).on('click', 'a', function (e) {
        //alert("clicked");
        e.preventDefault();
        localStorageService.set('heldPassdownID', passdown);
        passdownData.ID = prntID;
        passdownData.TokenID = passdown;//this.enrollData.heldPassDown = passdown;
        enrollService.holdPassdown(passdownData);
        //window.location.href = '/Home.html'
    });

    $scope.viLink = "http://" + passdown.toString() + ".bodybyvi.com/customer"

    window.open("http://" + passdown.toString() + ".bodybyvi.com/customer");

    $scope.message = "";

    $scope.enroll = function () {

        //$http.get(serviceBase + 'api/Customer/1105034').success(function (data) {
        //    alert("Added Successfully!!");
        //this.enrollData.PassPrnt = passdown;
        //this.enrollData.PrntID = prntID;


        //$http.post(serviceBase + 'api/Customer/Enroll', this.enrollData).success(function (data) {
        //        //alert("Added Successfully!!");
        //    //$scope.passdown = data[0];
        //    //var vid = $scope.passdown.id;
        //    //var id = data[0].id;
        //    //$scope.custData = data[0];
        //    //var exmp = $scope.custData.name;
        //    //localStorageService.set('customerData', { passdownID: $scope.custData.id, startDate: $scope.custData.startDate.toString(), kids: $scope.custData.kids.toString() });
            
        //    $location.path('/scorecard');

        //}).error(function (data) {
        //    $scope.error = "An Error has occured while Adding Customer! " + data;
        //    alert(data.message.toString());
            
        //});

        this.enrollData.PassPrnt = prntID;
        this.enrollData.PrntID = passdown;
        

        enrollService.enrollUser(this.enrollData).then(function (response) {

            localStorageService.remove('heldPassdownID');
            $location.path('/scorecard');

        },
        function (err) {
            $scope.message = err.error_description;
        });
    };
    
    $rootScope.special = $location.path();

}]);