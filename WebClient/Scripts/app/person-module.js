var personModule = angular.module('personModule', ['rjpUtils']);
personModule.service('personService', ['httpService', '$log', function (httpService, $log) {
    this.get = function () {
        $log.debug('In people service get');
        return httpService.httpGet('api/people/get');
    };
    this.getClaims = function () {
        return httpService.httpGet('api/people/getclaims');
    };
}]);

personModule.controller('personController', ['$scope', '$location', 'personService', '$log', function ($scope, $location, personService, $log) {
    $scope.People = [];

    $scope.Message = "";
    $scope.ErrorMessage = "";
    $scope.userName = sessionStorage.getItem('userName');

    $scope.init = function() {
        loadPeople();
    };
    //loadClaims();

    function loadPeople() {
        $log.debug('In loadPeople');
        $scope.Message = "";
        $scope.ErrorMessage = "";
        personService.get().then(function (data) {
            $scope.People = data;
            $scope.Message = "Api call completed successfully";
        }, function (err) {
            $scope.ErrorMessage = "Error calling person api.";
        });
    };

    function loadClaims() {
        $scope.Message = "";
        $scope.ErrorMessage = "";
        personService.getClaims().then(function (data) {
            $scope.Claims = data;
        }, function (err) {
            $scope.ErrorMessage = "Error calling claims api.";
        });
    };

    $scope.logout = function () {
        sessionStorage.removeItem('accessToken');
        $location.path('/security');
    };
}]);