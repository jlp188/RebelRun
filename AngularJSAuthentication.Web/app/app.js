/// <reference path="views/countdowntimer.html" />

var app = angular.module('AngularAuthApp', ['ngRoute', 'LocalStorageModule', 'angular-loading-bar', 'timer', 'countdownTimer', 'ngMessages', 'ngYoutubeEmbed']);

app.config(function ($routeProvider) {

    $routeProvider.when("/home", {
        controller: "homeController",
        templateUrl: "/app/views/login.html"
    });

    $routeProvider.when("/scorecard", {
        controller: "scorecardController",
        templateUrl: "/app/views/scorecard.html"
    });

    $routeProvider.when("/login", {
        controller: "loginController",
        templateUrl: "/app/views/login.html"
    });

    $routeProvider.when("/signup", {
        controller: "signupController",
        templateUrl: "/app/views/signup.html"
    });

    $routeProvider.when("/orders", {
        controller: "ordersController",
        templateUrl: "/app/views/orders.html"
    });

    $routeProvider.when("/enroll", {
        controller: "enrollController",
        templateUrl: "/app/views/enroll.html"
    });

    $routeProvider.when("/video1", {
        controller: "videoController",
        templateUrl: "/app/views/video1.html"
    });

    $routeProvider.when("/refresh", {
        controller: "refreshController",
        templateUrl: "/app/views/refresh.html"
    });

    $routeProvider.when("/tokens", {
        controller: "tokensManagerController",
        templateUrl: "/app/views/tokens.html"
    });

    $routeProvider.when("/associate", {
        controller: "associateController",
        templateUrl: "/app/views/associate.html"
    });

    $routeProvider.otherwise({ redirectTo: "/login" });

});

var serviceBase = 'http://localhost:62846/'; //'http://thevimob.com/'; //
app.constant('ngAuthSettings', {
    apiServiceBaseUri: serviceBase,
    clientId: 'RebelRun'//'ngAuthApp' //
});


//app.directive("countdownTimer", function () {
//    return {
//        restrict: 'AE',
//        templateUrl: '<timer countdown="189046188" max-time-unit="minute" interval="1000">{{mminutes}} minute{{minutesS}}, {{sseconds}} second{{secondsS}}</timer>',
//        replace: true
//    }
//});

app.config(function ($httpProvider) {
    $httpProvider.interceptors.push('authInterceptorService');
});

app.run(['authService', function (authService) {
    authService.fillAuthData();
}]);


