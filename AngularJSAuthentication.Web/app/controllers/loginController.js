'use strict';
app.controller('loginController', ['$scope', '$rootScope', '$location', 'authService', 'localStorageService', 'ngAuthSettings', function ($scope, $rootScope, $location, authService, localStorageService, ngAuthSettings) {

    $scope.loginData = {
        userName: "",
        password: "",
        useRefreshTokens: false
    };

    var parm = $location.search().viid;
    $scope.viid = parm;

    $scope.message = "";

    $scope.login = function () {

        authService.login($scope.loginData).then(function (response) {

            $location.path('/scorecard');

        },
         function (err) {
             $scope.message = err.error_description;
         });
    };

    $scope.authExternalProvider = function (provider) {

        var redirectUri = location.protocol + '//' + location.host + '/authcomplete.html';

        

        var externalProviderUrl = ngAuthSettings.apiServiceBaseUri + "api/Account/ExternalLogin?provider=" + provider
                                                                    + "&response_type=token&client_id=" + ngAuthSettings.clientId
                                                                    + "&redirect_uri=" + redirectUri + "&vi_id=" + $scope.viid;
        window.$windowScope = $scope;

        var oauthWindow = window.open(externalProviderUrl, "Authenticate Account", "location=0,status=0,width=600,height=750");
    };

    $scope.authCompletedCB = function (fragment) {

        $scope.$apply(function () {

            if (fragment.haslocalaccount === 'False') {

                authService.logOut();

                authService.externalAuthData = {
                    provider: fragment.provider,
                    userName: fragment.external_user_name,
                    externalAccessToken: fragment.external_access_token,
                    viid: $scope.viid,
                    pbEmail: fragment.pbEmail
                };

                $location.path('/associate');


            }
            else {
                localStorageService.set('viID', fragment.external_user_name);
                //Obtain access token and redirect to Scorecard
                var externalData = { provider: fragment.provider, externalAccessToken: fragment.external_access_token };
                authService.obtainAccessToken(externalData).then(function (response) {

                    //localStorageService.set('viID',$scope.viid);

                    $location.path('/scorecard');

                },
             function (err) {
                 $scope.message = err.error_description;
             });
            }

        });
    };

    $rootScope.special = $location.path();

}]);
