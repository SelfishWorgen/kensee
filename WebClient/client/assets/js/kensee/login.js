$(document).ready(function(){
    $('.loginBtn').on('click', function(){
        var username = $('.login-name').val();
        var password = $('.login-password').val();
        $.ajax({
            method: "GET",
            url: 'http://50.22.216.6/KenseeAPI/api/login/' + username + '/' + password,
        }).done(function(res){
            console.log(res);
            if(res){
                window.localStorage.setItem('user', JSON.stringify({name: username, pass: password}));
                window.location.href = 'index.html';
            }
        })
    })
});