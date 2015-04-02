// Get all information that we need for the documentation from the DB
var publicKey  = "";
var privateKey = "";
var selectedKey = "";

// Environment constants, change these as of when switching between deployment and 
// local environments
var _HOST  = "http://xserve.uopnet.plymouth.ac.uk";
var _LOCAL = "http://localhost:50557";

var _BASE = "/modules/soft338/asims";

// Navigation scrolling effect
var lastId;
var menu;
var items;
var scrollItems;
var barShowing = false;

// Fire on ready requests
$(document).ready(function () {
    getKeys();

    // Set the cache selectors
    menu = $("#menuTop");
    items = menu.find("a");

    // Create a map of the positions of the items, use this as 
    // a reference to query where we have selected on the page
    scrollItems = items.map(function () {
        var item = $($(this).attr("href"));
        if (item.length) { return item; }
    });

    $('.code').hide();

    // Update the public api key
    $("#resetPublic").click(function () {
        resetKey(publicKey);
        return false;
    });

    // Update the private api key
    $("#resetPrivate").click(function () {
        resetKey(privateKey);
        return false;
    });

    // All users code click
    $('#btnGetAllPlayers').click(function () {
        getPlayers(selectedKey, $("#allPlayersCode"));
        return false;
    });

    // Get players by name
    $('#btnPlayersByName').click(function () {
        var pName = $('#gPlayerName').val();
        var pRegion = $('#gSelectRegion').find(":selected").text();

        if (typeof pName == 'undefined' || pName.length == 0) {
            pName = "Fromp";
        }
        if (typeof pRegion == 'undefined') {
            pRegion = "ALL";
        }

        getPlayers(selectedKey, $("#specificPlayersCode"), pName, pRegion);

        return false;
    });

    // Close the code snippet
    $('.close').click(function () {
        $(this).parent().slideUp();
        return false;
    });
    // Scroll to selected item
    items.click(function (e) {
        var href = $(this).attr("href");
        var offset = href === "#" ? 0 : $(href).offset().top;
        $('html, body').stop().animate({
            scrollTop: offset
        }, 300);
        e.preventDefault();
        return false;
    });

    // Post user
    $('#postPlayer').click(function () {
        var pName = $('#pName').val();
        var pRegion = $('#pRegion').find(":selected").text();
        var pWins = $('#pPoints').val();
        var pDraws = $('#pDraws').val();
        var pLosses = $('#pLosses').val();
        var pPoints = $('#pPoints').val();

        if (typeof pName == 'undefined' || pName.length == 0) {
            pName = "Fromp";
            $('#pName').val(pName);
        }
        if (typeof pRegion == 'undefined') {
            pRegion = "EUW";
        }
        if (pWins <= 0 || typeof pWins == 'undefined') {
            pWins = 0;
            $('#pWins').val(pWins);
        }
        if (pDraws <= 0 || typeof pDraws == 'undefined') {
            pDraws = 0;
            $('#pDraws').val(pWins);
        }
        if (pLosses <= 0 || typeof pLosses == 'undefined') {
            pLosses = 0;
            $('#pLosses').val(pWins);
        }
        if (pPoints <= 0 || typeof pPoints == 'undefined') {
            pPoints = 0;
            $('#pPoints').val(pWins);
        }

        postPlayer(selectedKey, pName, pRegion, pWins, pDraws, pLosses, pPoints);

        return false;
    });

    // Update user
    $('#updateButton').click(function () {
        var pName = $('#uName').val();
        var pRegion = $('#uRegion').find(":selected").text();
        var pWins = $('#uPoints').val();
        var pDraws = $('#uDraws').val();
        var pLosses = $('#uLosses').val();
        var pPoints = $('#uPoints').val();


        if (typeof pName == 'undefined' || pName.length == 0) {
            pName = "Fromp";
            $('#uName').val(pName);
        }
        if (typeof pRegion == 'undefined') {
            pRegion = "EUW";
        }
        if (pWins <= 0 || typeof pWins == 'undefined') {
            pWins = 0;
            $('#uWins').val(pWins);
        }
        if (pDraws <= 0 || typeof pDraws == 'undefined') {
            pDraws = 0;
            $('#uDraws').val(pWins);
        }
        if (pLosses <= 0 || typeof pLosses == 'undefined') {
            pLosses = 0;
            $('#uLosses').val(pWins);
        }
        if (pPoints <= 0 || typeof pPoints == 'undefined') {
            pPoints = 0;
            $('#uPoints').val(pWins);
        }

        updatePlayer(selectedKey, pName, pRegion, pWins, pDraws, pLosses, pPoints);

        return false;
    });

    // Delete player
    $('#btnDelete').click(function () {
        var pName = $('#dName').val();
        var pRegion = $('#dRegion').find(":selected").text();

        if (typeof pName == 'undefined' || pName.length == 0) {
            pName = "nil";
            $('#dName').val("Nil");
        }
        if (typeof pRegion == 'undefined') {
            pRegion = "ALL";
        }

        deletePlayer(selectedKey, pName, pRegion);

        return false;
    });

    // Change selected key
    $('#selectedKey').change(function () {
        if ($(this).find(":selected").val() == "Private") {
            selectedKey = privateKey;
        } else {
            selectedKey = publicKey;
        }

        setFields();
        return false;
    });

});

// Bind the scroll event to check page location
$(window).scroll(function () {
    var currPos = $(this).scrollTop() + 50;
    var currItem = scrollItems.map(function () {
        if ($(this).offset().top < currPos)
            return this;
    });

    if (currPos > 250 && !barShowing) {
        $("#topBar").removeClass("hidden");
        $("#topBar").slideDown("fast");
        $(".selectedKeyWrap").addClass("keyTop");
        barShowing = true;
    } else if(currPos < 250 && barShowing) {
        barShowing = false;
        $("#topBar").slideUp("fast", function () {
            $("#topBar").addClass("hidden")
            $(".selectedKeyWrap").removeClass("keyTop");
        });
    }

    // Get the curr items id and then update the GUI
    currItem = currItem[currItem.length - 1];
    var id = currItem && currItem.length ? currItem[0].id : "";

    if (lastId !== id)
    {
        lastId = id;
        // Set the new active state
        items.parent().removeClass("active").end().filter("[href=#" + id + "]").parent().addClass("active");
    }
});

// Populate the global variables 
function getKeys()
{
    $.ajax({
        type: "GET",
        contentType: "application/json",
        url: _HOST + _BASE + "/v1/key",
        cache: false,
        data: {},
        success: function (response) {
            if (response.keys[0].permission == 0) {
                publicKey  = response.keys[0].key;
                privateKey = response.keys[1].key;
            } else {
                publicKey  = response.keys[1].key;
                privateKey = response.keys[0].key;
            }

            if(selectedKey == "")
                selectedKey = publicKey;

            setFields();
        },
        error: function (response)
        {
        }
    });
}

// Delete the player object
function deletePlayer(key, pName, pRegion)
{
    var json = {
        player_name: pName,
        player_location: pRegion
    };

    // Pass the stringified JSON object to the server
    $.ajax({
        type: "DELETE",
        contentType: "application/json",
        dataType: "json",
        url: _HOST + _BASE + "/v1/player/" + pRegion + "/" + pName + "?api_key=" + key,
        data: JSON.stringify(json),
        success: function (response) {
            // Build the format string
            var outputString = "";
            outputString += "Request Type: 'DELETE',\n"
                          + "Content Type: 'application/json,'\n"
                          + "Request URL: " + _HOST + _BASE + "/v1/player/" + pRegion + "/" + pName + "?api_key=" + key + "\n\n";

            outputString += "{ \n    response: " + response.errCode + ", \n" +
                               "    message: '" + response.errMsg + "', \n" +
                               "    players: [ \n";

            for (var i = 0; i < response.players.length; i++) {
                outputString += "    {\n          player_name: " + response.players[i].player_name + ", \n"
                              + "          player_location: " + response.players[i].player_location + ", \n"
                              + "          player_wins: " + response.players[i].player_wins + ", \n"
                              + "          player_draws: " + response.players[i].player_draws + ", \n"
                              + "          player_losses: " + response.players[i].player_losses + ", \n"
                              + "          player_points: " + response.players[i].player_points + ", \n"
                              + "    }";

                if (i != response.players.length) {
                    outputString += ",\n";
                } else {
                    outputString += "\n";
                }
            }

            outputString += "}";

            $('#deleteCode').text(outputString);
            $("#deleteCode").parent().slideDown();
        },
        error: function (response) {
            // Set error - shouldnt happen
        }
    });
}

// Update the player object
function updatePlayer(key, pName, pRegion, pWins, pDraws, pLosses, pPoints) {
    var json = {
        player_name: pName,
        player_location: pRegion,
        player_wins: pWins,
        player_draws: pDraws,
        player_losses: pLosses,
        player_points: pPoints
    };

    // Pass the stringified JSON object to the server
    $.ajax({
        type: "PUT",
        contentType: "application/json",
        dataType: "json",
        url: _HOST + _BASE + "/v1/player/" + pRegion + "/" + pName + "?api_key=" + key,
        data: JSON.stringify(json),
        success: function (response) {
            // Build the format string
            var outputString = "";
            outputString += "Request Type: 'PUT',\n"
                          + "Content Type: 'application/json,'\n"
                          + "Request URL: " + _HOST + _BASE + "/v1/player/" + pRegion + "/" + pName + "?api_key=" + key + "\n\n";

            outputString += "{ \n    response: " + response.errCode + ", \n" +
                               "    message: '" + response.errMsg + "', \n" +
                               "    players: [ \n";

            for (var i = 0; i < response.players.length; i++) {
                outputString += "    {\n          player_name: " + response.players[i].player_name + ", \n"
                              + "          player_location: " + response.players[i].player_location + ", \n"
                              + "          player_wins: " + response.players[i].player_wins + ", \n"
                              + "          player_draws: " + response.players[i].player_draws + ", \n"
                              + "          player_losses: " + response.players[i].player_losses + ", \n"
                              + "          player_points: " + response.players[i].player_points + ", \n"
                              + "    }";

                if (i != response.players.length) {
                    outputString += ",\n";
                } else {
                    outputString += "\n";
                }
            }

            outputString += "}";

            $('#updateCode').text(outputString);
            $("#updateCode").parent().slideDown();
        },
        error: function (response) {
            // Set error - shouldnt happen
        }
    });
}

// Post the new player object
function postPlayer(key, pName, pRegion, pWins, pDraws, pLosses, pPoints)
{
    var json = {
        player_name: pName,
        player_location: pRegion,
        player_wins: pWins,
        player_draws: pDraws,
        player_losses: pLosses,
        player_points: pPoints
    };

    // Pass the stringified JSON object to the server
    $.ajax({
        type: "POST",
        contentType: "application/json",
        dataType: "json",
        url: _HOST + _BASE + "/v1/player?api_key=" + key,
        data: JSON.stringify(json),
        success: function(response)
        {
            // Build the format string
            var outputString = "";
            outputString += "Request Type: 'POST',\n"
                          + "Content Type: 'application/json,'\n"
                          + "Request URL: " + _HOST + _BASE + "/player" + "\n\n";

            outputString += "{ \n    response: " + response.errCode + ", \n" +
                               "    message: '" + response.errMsg + "', \n" +
                               "    players: [ \n";

            for (var i = 0; i < response.players.length; i++) {
                outputString += "    {\n          player_name: " + response.players[i].player_name + ", \n"
                              + "          player_location: " + response.players[i].player_location + ", \n"
                              + "          player_wins: " + response.players[i].player_wins + ", \n"
                              + "          player_draws: " + response.players[i].player_draws + ", \n"
                              + "          player_losses: " + response.players[i].player_losses + ", \n"
                              + "          player_points: " + response.players[i].player_points + ", \n"
                              + "    }";

                if (i != response.players.length) {
                    outputString += ",\n";
                } else {
                    outputString += "\n";
                }
            }

            outputString += "}";

            $('#postCode').text(outputString);
            $("#postCode").parent().slideDown();
        },
        error: function(response)
        {
            // Set error - shouldnt happen
        }
    });
}

// Gets a list of all players
function getPlayers(key, elem, name, country)
{
    // Define request URI
    var uri = "";

    // Set the URI
    if (typeof name != 'undefined' || typeof country != 'undefined')
    {
        uri = "/v1/player/" + country + "/" + name + "?api_key=" + key;
    }
    else
    {
        uri = "/v1/player?api_key=" + key;
    }

    $.ajax({
        type: "GET",
        contentType: "application/json",
        url: _HOST + _BASE + uri,
        cache: false,
        data: {},
        success: function (response)
        {
            // Build the format string
            var outputString = "";
            outputString += "Request Type: 'GET',\n"
                          + "Content Type: 'application/json,'\n"
                          + "Request URL: " + _HOST + _BASE + uri + "\n\n";

            outputString += "{ \n    response: " + response.errCode + ", \n" +
                               "    message: '" + response.errMsg + "', \n" +
                               "    players: [ \n";

            for (var i = 0; i < response.players.length; i++)
            {
                outputString += "    {\n          player_name: " + response.players[i].player_name + ", \n"
                              + "          player_location: " + response.players[i].player_location + ", \n"
                              + "          player_wins: " + response.players[i].player_wins + ", \n"
                              + "          player_draws: " + response.players[i].player_draws + ", \n"
                              + "          player_losses: " + response.players[i].player_losses + ", \n"
                              + "          player_points: " + response.players[i].player_points + ", \n"
                              + "    }";

                if (i != response.players.length) {
                    outputString += ",\n";
                } else {
                    outputString += "\n";
                }
            }
            
            outputString += "}";

            elem.text(outputString);
            elem.parent().slideDown();
        },
        error: function (response)
        {
            $("#error").text(response);
        }
    });
}

// Sets all fields in the api
function resetKey(key) 
{
    $.ajax({
        type: "PUT",
        contentType: "application/json",
        url: _HOST + _BASE + "/v1/key/reset?api_key=" + key,
        cache: false,
        success: function (response) {
            if (response.keys[0].permission == 0) {
                publicKey = response.keys[0].key;

                if (selectedKey == key)
                    selectedKey = publicKey;
            } else {
                privateKey = response.keys[0].key;

                if (selectedKey == key)
                    selectedKey = privateKey;
            }

            setFields();
        },
        error: function (response) {
        }
    });
}

// Sets all fields in the api
function setFields()
{
    $('#publicKey').text(publicKey);
    $('#privateKey').text(privateKey);
    $('#retrieveCode').text(_HOST + _BASE + "/v1/player?api_key=" + selectedKey);
    $('.selectedKey').text(selectedKey);
}

