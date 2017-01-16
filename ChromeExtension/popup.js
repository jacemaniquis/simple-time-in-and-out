$(function () {

    $('#frame').attr('src', "http://localhost:8089/Chrome");

    $('#frame').load(function() {
        $('#loading').css('display', 'none');
    });

    window.addEventListener("message", function (e) {
        var datus = e.data;
        if (datus.command === "frame:close") {
           
        } else if (datus.command === "frame:resize") {
            $('#frame').height(datus.height);
            $('#frame').width(datus.width);
        } else if (datus.command === "frame:redirect") {
            chrome.tabs.create({
                url: datus.url
            });
        }
    });
});
