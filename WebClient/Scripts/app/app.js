var app = angular.module('secureWebApiApp', ['rjpUtils', 'securityModule', 'personModule', 'ngRoute'])
            .constant('appConfig', {
                baseUrl: 'http://localhost:57916/',
                clientId: 'ngAuthApp'
            })
            .config(['$routeProvider',
                    function ($routeProvider) {
                        $routeProvider.
                        when('/index', {
                            templateUrl: 'people.html',
                            controller: 'personController'
                        }).
                        when('/security', {
                            templateUrl: 'login.html',
                            controller: 'loginController'
                        }).
                        otherwise({
                            redirectTo: '/index'
                        });
                    }]);
