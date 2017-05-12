// Write your Javascript code.

$(document).ready(function () {
    var container = $("#datacontainer");
    if (container.length > 0) {
        for (var i = 0; i < 4; i++) {
            $.getJSON("/api/Sample/GetUserData", function (data) {
                container.append(JSON.stringify(data));
            });
        }
    }
});