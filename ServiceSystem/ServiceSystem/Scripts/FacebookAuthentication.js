/// <reference path="jquery-1.10.2.js" />


function getAccessToken() {
    if (location.hash) {
        if (location.hash.split('access_token=')) {
            var accessToken = location.hash.split('access_token=')[1].split('&')[0];
            if (accessToken) {
                isUserRegistered(accessToken);
            }
        }
    }
}

function isUserRegistered(accessToken) {
    $.ajax({
        url: '/api/Account/UserInfo',
        method: 'GET',
        headers: {
            'content-type': 'application/json',
            'Authorization': 'Bearer ' + accessToken
        },
        success: function (response) {
            if (response.HasRegistered) {
                localStorage.setItem("access_token", accessToken);
                localStorage.setItem("user_name", response.Email);
                window.location.href = "http://localhost:49332/Service/Main";
            }
            else {
                signupExternalUser(accessToken, response.LoginProvider);
            }
        }
    });
}


function signupExternalUser(accessToken, provider) {
    $.ajax({
        url: '/api/Account/RegisterExternal',
        method: 'POST',
        headers: {
            'content-type': 'application/json',
            'Authorization': 'Bearer ' + accessToken
        },
        success: function () {
            window.location.href = "/api/Account/ExternalLogin?provider=Facebook&response_type=token&client_id=self&redirect_uri=http%3a%2f%2flocalhost%3a61358%2fLogin.html&state=K_MjqwnPFPi_LcRQM_zHKmsKm3ebqIrsc07MnsCsZV41";
        }
    });
}