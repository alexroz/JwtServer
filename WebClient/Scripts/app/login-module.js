var securityModule = angular.module('securityModule', ['rjpUtils']);
securityModule.service('loginservice', ['httpService', 'appConfig', function (httpService, appConfig) {
    this.register = function (userInfo) {
        return  httpService.httpPost('api/Account/Register', userInfo);
    };

    this.login = function (userlogin) {
        return httpService.login(userlogin.username, userlogin.password);
    };

    this.resetToken = function () {
        // This assumes that the refresh token is available.
        var token = localStorage.getItem('refreshToken');
        if (!token)
            alert('Error: no refresh token!');

        return httpService.httpPost('token', { grant_type: 'refresh_token', refresh_token: token, client_id: appConfig.clientId }).then(function(data) {
            localStorage.setItem('refreshToken', data.refresh_token);
            sessionStorage.setItem('accessToken', data.access_token);
        });
    };

}]);

/*
securityModule.factory('securityState', function (httpService, $http) {

    // Note change other code to use local storage.
    return {
        userName : localStorage.userName,
        accessToken: localStorage.accessToken
        sessionStorage.setItem('refreshToken', data.refresh_token);
        }
});
*/


securityModule.controller('loginController', ['$scope', '$location', 'loginservice', function ($scope, $location, loginservice) {
    $scope.responseData = "";
    $scope.userName = "";
    $scope.userRegistrationEmail = "";
    $scope.userRegistrationPassword = "";
    $scope.userRegistrationConfirmPassword = "";
    $scope.userLoginEmail = "";
    $scope.userLoginPassword = "";
    $scope.accessToken = "";
    $scope.refreshToken = "";
    //Ends Here

    //Function to register user
    $scope.registerUser = function () {

        $scope.responseData = "";

        //The User Registration Information
        var userRegistrationInfo = {
            Email: $scope.userRegistrationEmail,
            Password: $scope.userRegistrationPassword,
            ConfirmPassword: $scope.userRegistrationConfirmPassword
        };

        var promiseregister = loginservice.register(userRegistrationInfo);

        promiseregister.then(function (resp) {
            $scope.responseData = "User is Successfully logged in.";
            $scope.userRegistrationEmail = "";
            $scope.userRegistrationPassword = "";
            $scope.userRegistrationConfirmPassword = "";
        }, function (err) {
            $scope.responseData = "Error " + err.status;
        });
    };


    $scope.redirect = function () {
        $location.path('/');
    };

    //Function to Login. This will generate Token 
    $scope.login = function () {
        //This is the information to pass for token based authentication
        var userLogin = {
            username: $scope.userLoginEmail,
            password: $scope.userLoginPassword
        };

        var promiselogin = loginservice.login(userLogin);

        promiselogin.then(function (data) {
            //Store the token information in the SessionStorage
            //So that it can be accessed for other views
            if (data.success === true) {
                $location.path('/');
            } else {
                $scope.responseData = "Error " + err.status;
            }
        });

    };
}]);