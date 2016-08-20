(function() {

    'use strict';

    var baseUrl = "http://localhost:57916";
    var tokenUrl = baseUrl + "/token";
    $('#errorMessage').hide();

    // Handle the standard login.
    $("#loginButton").click(function (event) {
        event.preventDefault();
        $('#errorMessage').hide();
        var userName = $('#userName').val();
        var password = $('#password').val();

        $.ajax({
            url: tokenUrl,
            method: "POST",
            headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
            data: { grant_type: 'password', username: userName, password: password, client_id: 'ngAuthApp', client_secret: 'VN5/YG8lI8uo76wXP6tC+39Z1Wzv+XTI/bc0LPLP40U=' },
        }).done(function (resp) {
            var responseString = JSON.stringify(resp);
            $('#accessToken').val(resp.access_token);
            $('#refreshToken').val(resp.refresh_token);
            $('#tokenExpiry').val(resp['.expires']);
            $('#loginResults').val(responseString);
        }).fail(function (resp) {
            $('#errorMessage').html('Error refreshing access token.');
            $('#errorMessage').show();
        });
    });

    // Use the refresh token to get a new access token.
    $("#refreshButton").click(function (event) {
        $('#errorMessage').hide();
        event.preventDefault();
        var token = $('#refreshTokenInput').val();

        $.ajax({
            url: tokenUrl,
            method: "POST",
            headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
            data: { grant_type: 'refresh_token', refresh_token: token, client_id: 'ngAuthApp' },
        }).done(function (resp) {
            var responseString = JSON.stringify(resp);
            $('#accessTokenRefresh').val(resp.access_token);
            $('#refreshTokenRefresh').val(resp.refresh_token);

            $('#resultsRefresh').val(responseString);
        }).fail(function (resp) {
            $('#errorMessage').html('Error refreshing access token.');
            $('#errorMessage').show();
        });
    });

    // Call a secure API
    $("#callAPIButtom").click(function (event) {
        $('#errorMessage').hide();
        event.preventDefault();
        var apiUrl = $('#apiUrl').val();
        var token = $('#secureApiAccessToken').val();

        $.ajax({
            url: baseUrl + apiUrl,
            method: "GET",
            headers: { 'Content-Type': 'application/json', 'Accept': 'application/json', 'Authorization' : 'Bearer ' + token}
        }).done(function (resp) {
            var responseString = JSON.stringify(resp);
            $('#apiResults').val(responseString);
        }).fail(function (resp) {
            $('#errorMessage').html('Error calling secure api.');
            $('#errorMessage').show();
        });
    });


})();