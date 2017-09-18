/// <reference path="jquery-1.10.2.js" />

function getAccessToken() {
    if (location.hash) {
        if (location.hash.split('access_token=')) {
            var accessToken = location.hash.split('access_token=')[1].split('&')[0];
            localStorage.setItem("access_token", accessToken);
            document.cookie = 'access_token=;path=/;expires=Thu, 01 Jan 1970 00:00:01 GMT;';
            document.cookie = 'access_token=' + accessToken + ';path=/;';

            $.ajax({
                method: "POST",
                url: '/Service/GetToken',
                dataType: "json",
                contentType: "application/json; charset=utf-8",
                data: JSON.stringify({ token: localStorage.getItem("access_token") })
            });
        }
    }
}
