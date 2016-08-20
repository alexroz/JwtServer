(function () {
    "use strict";
    angular.module('rjpUtils',[])
        .factory('httpService', ['$http', '$q', '$log', 'appConfig', function ($http, $q, $log, appConfig) {

            var baseApiUrl = appConfig.baseUrl; //'http://localhost:57916/';

            // Spinner handling....
            var numLoadings = 0;
            var loaderShow = function () {
/*                numLoadings++;
                $rootScope.$broadcast("loader_show");*/
            };

            var loaderHide = function () {
/*                if ((--numLoadings) === 0) {
                    $rootScope.$broadcast("loader_hide");
                }*/
            };

            var loaderError = function () {
/*                if (!--numLoadings) {
                    $rootScope.$broadcast("loader_hide");
                } */
            }

            var getFullUrl = function(targetUrl) {
                return baseApiUrl + targetUrl;
            };

            /* This is where the Bearer Token Magic happens! */
            var getAuthHeaders = function() {
                var accesstoken = sessionStorage.getItem('accessToken');
                var authHeaders = {};
                if (accesstoken) {
                    authHeaders.Authorization = 'Bearer ' + accesstoken;
                }
                return authHeaders;
            }

            var executeRequestWithLoader = function (args) {
                var deferred = $q.defer();
                loaderShow();
                $http(args).then(
                    function(response) {
                        loaderHide();
                        deferred.resolve(response.data);
                    },
                    function(response) {
                        loaderError();
                        deferred.reject(response);
                    });

/*                    .success(function (data) {
                        loaderHide();
                        deferred.resolve(data);
                    })
                    .error(function (data) {
                        loaderError();
                        deferred.reject(data);
                    });
*/
                return deferred.promise;
            };

            var _httpGet = function (targetUrl, data) {
                var args = {
                    url: getFullUrl(targetUrl),
                    method: 'GET',
                    headers: getAuthHeaders()
                };

                if (data) {
                    args.params = data;
                }
                return executeRequestWithLoader(args);
            };

            var _httpGetWithCache = function (targetUrl) {
                var args = {
                    url: getFullUrl(targetUrl),
                    method: 'GET',
                    headers: getAuthHeaders(),
                    cache: true
                };
                return executeRequestWithLoader(args);
            };

            var _httpPost = function (targetUrl, postData) {
                var url = getFullUrl(targetUrl);
                var dataParam = $.param(postData);
                //var dataParam2 = $.param({ grant_type: 'password', username: userlogin.username, password: userlogin.password });
                var args = {
                    url: url,
                    method: "POST",
                    data: dataParam,
                    headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
                };

                return executeRequestWithLoader(args);
            };

            var _httpPostStandard = function (targetUrl, postData) {
                var url = getFullUrl(targetUrl);
                var jsonData = JSON.stringify(postData);
                var args = {
                    url: url,
                    method: "POST",
                    data: jsonData,
                    headers: { 'Content-Type': 'application/json' },
                };

                return executeRequestWithLoader(args);
            };

            var _refreshToken = function () {
                // This assumes that the refresh token is available.
                $log.debug('Refreshing access token using refresh token');
                var deferred = $q.defer();
                var token = localStorage.getItem('refreshToken');
                if (!token) {
                    $log.debug('No refresh token available');
                }

                _httpPost('token', { grant_type: 'refresh_token', refresh_token: token, client_id: appConfig.clientId }).then(function (data) {
                    localStorage.setItem('refreshToken', data.refresh_token);
                    sessionStorage.setItem('accessToken', data.access_token);
                    $log.debug('Successfully refreshed access token.');
                    deferred.resolve({ success: true });
                }, function(response) {
                    $log.debug('Refresh failed with status:' + response.status);
                });
                return deferred.promise;
            }

            var _login = function (username, password) {
                $log.debug('Attempting login with user name and password.');
                var deferred = $q.defer();
                _httpPost('token', { grant_type: 'password', username: username, password: password, client_id: appConfig.clientId }).then(function(data) {
                    localStorage.setItem('refreshToken', data.refresh_token);
                    sessionStorage.setItem('accessToken', data.access_token);
                    $log.debug('Login successful!');
                    deferred.resolve({ success: true });
                }, function(response) {
                    $log.debug('Login failed with status:' + response.status);
                    deferred.resolve({ success: false });
                });

                return deferred.promise;
            }

            var _httpGetWithRetry = function (targetUrl, data) {
                var deferred = $q.defer();

                _httpGet(targetUrl, data).then(function (data) {
                    deferred.resolve(data);
                }, function (response) {
                    if (response.status === 401) {
                        $log.debug('API request failed try and refresh token.');
                        _refreshToken().then(
                            function() {
                                /* Now try again.*/
                                _httpGet(targetUrl, data).then(function (data) {
                                    $log.debug('API success post token refresh.');
                                    deferred.resolve(data);
                                }, function (response) {
                                    $log.error('API error post token refresh.');
                                    deferred.resolve(response);
                                });
                            }, function(data) {
                                deferred.reject(data);
                            });

                    } else {
                        deferred.reject(response);
                    }
                });

                return deferred.promise;
            };


            return {
                httpGet:_httpGetWithRetry /* _httpGet*/,
                httpPost: _httpPost,
                httpGetWithCache: _httpGetWithCache,
                login: _login,
                refreshToken: _refreshToken
            }
        }])
.directive("spinner", function () {
    return {
        restrict: "E",
        replace: true,
        template: "<div class=\"spinner-wrapper\" style='display: none;'><div class=\"spinner\"></div></div>",
        link: function ($scope, element, attrs) {
            $scope.$on("loader_show", function () {
                if (attrs.disabled != 'true') {
                    return element.show();
                }
            });
            $scope.$on("loader_hide", function () {
                return element.hide();
            });
        }
    };
})
})();
