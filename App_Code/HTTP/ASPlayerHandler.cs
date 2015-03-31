using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;
using System.IO;
using System.Runtime.Serialization.Json;

/// <summary>
/// Summary description for ASPlayerHandler
/// </summary>
public class ASPlayerHandler : ASHttpHandler
{
    /*
    * ---------------------------------------------------------------------------
    * Template Definitions
    * ---------------------------------------------------------------------------
    * All templates that are to be handled by the API are defined here.  This allows
    * for specific template matching against the input parameters. 
    * 
    * @template playerByNameTemplate  : used for GET, PUT or DELETE of a specific player
    * @template allPlayersTemplate    : used for GET or DELETE of all players in a country
    * @template basePlayerTemplate    : used to POST a new player to the API
    * 
    * ---------------------------------------------------------------------------
    */
    private UriTemplate playerByNameTemplate = new UriTemplate("/modules/soft338/asims/v1/player/{country}/by-name/{name}?key={apiKey}");
    private UriTemplate allPlayersTemplate   = new UriTemplate("/modules/soft338/asims/v1/player/{country}?key={apiKey}");
    private UriTemplate basePlayerTemplate   = new UriTemplate("/modules/soft338/asims/v1/player?key={apiKey}");

    private Uri requestedUri;

    // Holds the permission state returned from the db
    private ASDatabase.ASPermission permission = new ASDatabase.ASPermission();

    // As usual, this constructor does not require any inistialisation
	public ASPlayerHandler(Uri requestedUri)
	{
        this.requestedUri = requestedUri;
    }

    /// <summary>
    /// Handles the inbound request delegated from ASHttpHandler.  Templates are then matched
    /// to check which template we are dealing with, allowing us to call the correct handle 
    /// method.
    /// </summary>
    public void HandleRequest(HttpContext context)
    {
        UriTemplateMatch res = null;

        // Base player template handles
        res = basePlayerTemplate.Match(prefix, requestedUri);
        if (res != null)
            ProcessPlayer(context, res);

        // Handles the retrieval of all players for a given country or all countries
        // This request is restricted to GET only
        res = allPlayersTemplate.Match(prefix, requestedUri);
        if (res != null)
            ProcessPlayers(context, res);

        // Handles the GET, PUT and DELETE of a user in the requested URI
        res = playerByNameTemplate.Match(prefix, requestedUri);
        if (res != null)
            ProcessPlayerByName(context, res);

    }

    /// <summary>
    /// Handles GET, PUT and DELETE of resources on the server. 
    /// A POST request will raise a 500 error to the client
    /// </summary>
    /// <param name="context"></param>
    private void ProcessPlayerByName(HttpContext context, UriTemplateMatch template)
    {
        // Prematurely break out of the loop and send an unauthorised request 
        // back to the client
        if (!CheckPermissions(template))
        {
            SetResponse(context, 401);
            return;
        }

        // Everything was ok - continue to process the request
        string verb = context.Request.HttpMethod.ToUpper();
        switch (verb)
        {
            case "GET": GetPlayer(context, template);
                break;
            case "PUT": PutPlayer(context, template);
                break;
            case "DELETE": DeletePlayer(context, template);
                break;
            default: SetResponse(context, 500, true, null);
                break;
        }
    }

    /// <summary>
    /// Handles the POSTing and GETting of players from the system
    /// </summary>
    /// <param name="context"></param>
    private void ProcessPlayer(HttpContext context, UriTemplateMatch template)
    {
        // Prematurely break out of the loop and send an unauthorised request 
        // back to the client
        if (!CheckPermissions(template))
        {
            SetResponse(context, 401);
            return;
        }

        // Everything was ok - continue to process the request
        string verb = context.Request.HttpMethod.ToUpper();
        switch (verb)
        {
            case "POST": PostNewPlayer(context);
                break;
            case "GET": GetAllPlayers(context, template);
                break;
            default: SetResponse(context, 500, true, null);
                break;
        }
    }

    /// <summary>
    /// Retrieves all players from the system - an invalid verb will trigger an internal server error
    /// </summary>
    /// <param name="context"></param>
    private void ProcessPlayers(HttpContext context, UriTemplateMatch template)
    {
        // Prematurely break out of the loop and send an unauthorised request 
        // back to the client
        if (!CheckPermissions(template))
        {
            SetResponse(context, 401);
            return;
        }

        // Everything was ok - continue to process the request
        string verb = context.Request.HttpMethod.ToUpper();
        switch (verb)
        {
            case "GET": GetAllPlayers(context, template);
                break;
            default: SetResponse(context, 500, true, null);
                break;
        }
    }

    /// <summary>
    /// Calls the interface in ASPlayerManager to retrieve a serialized list of all players 
    /// which are situated on a specific nation server
    /// </summary>
    /// <param name="context"></param>
    /// <param name="template">The matched URI Template we can extract the bind variable from</param>
    private void GetAllPlayers(HttpContext context, UriTemplateMatch template)
    {
        // Define the output stream and set the Content Type for the header (ensure we recieve JSON back)
        Stream outputStream = context.Response.OutputStream;
        context.Response.ContentType = "application/json";

        string nation = "";

        // Check if a nation has been set
        if (template.BoundVariables.AllKeys.Length != 1)
            nation = template.BoundVariables["country"].ToUpper();

        // Create the Serializer and then get a list of all ASPlayers that match this criteria
        DataContractJsonSerializer json = new DataContractJsonSerializer(typeof(IEnumerable<ASPlayer>));
        IEnumerable<ASPlayer> players = ASPlayerManager.GetAllPlayers(nation);
        
        if(players.Count() > 0)
            SetResponse(context, 200, false, players.ElementAt(0), 0, players);
        else
            SetResponse(context, 404, true, null);
    }


    /// <summary>
    /// Calls the interface in ASPlayerManager to retrieve a serialized list of the specific player
    /// who matches the given name.  If no country is set this method can return multiple players
    /// with the same name from different servers
    /// </summary>
    /// <param name="context"></param>
    /// <param name="template">The matched URI Template we can extract the bind variable from</param>
    private void GetPlayer(HttpContext context, UriTemplateMatch template)
    {
        // Define the output stream and set the Content Type for the header (ensure we recieve JSON back)
        Stream outputStream = context.Response.OutputStream;
        context.Response.ContentType = "application/json";

        string nation = "";
        string name   = "";

        // Check if a nation has been set
        if (template.BoundVariables.AllKeys.Length != 1)
        {
            nation = template.BoundVariables["country"].ToUpper();
            name   = template.BoundVariables["name"].ToUpper();
        }

        // Create the Serializer and then get a list of all ASPlayers that match this criteria
        DataContractJsonSerializer json = new DataContractJsonSerializer(typeof(IEnumerable<ASPlayer>));
        IEnumerable<ASPlayer> players = ASPlayerManager.GetPlayersByName(nation, name);

        SetResponse(context, 200, false, players.ElementAt(0), 0, players);
    }

    /// <summary>
    /// Updates the player who exists on a specific server
    /// </summary>
    /// <param name="context"></param>
    /// <param name="template"></param>
    private void PutPlayer(HttpContext context, UriTemplateMatch template)
    {
        // Check that the user is not a guest
        if (permission == ASDatabase.ASPermission.GUEST)
        {
            SetResponse(context, 401);
            return;
        }

        // If the user isn't a guest, carry on as usual
        Int32 response = 500;
        Int32 playerId = 0;
        string nation = "";
        string name = "";

        // Check if a nation has been set
        if (template.BoundVariables.AllKeys.Length != 1)
        {
            nation = template.BoundVariables["country"].ToUpper();
            name   = template.BoundVariables["name"].ToUpper();
        }

        // If no nation or player name has been specified then break out
        if (string.IsNullOrEmpty(nation) || string.IsNullOrEmpty(name))
            response = 400;

        // Get the ID of the requested player set in the URI, and then pass this to the update call in
        // the ASPlayerManager handler.  The ID cannot be read from the input stream as it is unknown and therefore
        // needs to be requested seperately.
        DataContractJsonSerializer json = new DataContractJsonSerializer(typeof(IEnumerable<ASPlayer>));

        if (ASPlayerManager.GetPlayersByName(nation, name).Count != 0)
            playerId = ASPlayerManager.GetPlayersByName(nation, name).ElementAt(0).GetID();
        else
            response = 404;

        // Build the serializer
        json = new DataContractJsonSerializer(typeof(ASPlayer));
        ASPlayer p = (ASPlayer)json.ReadObject(context.Request.InputStream);

        response = ASPlayerManager.UpdatePlayer(p, playerId);

        // Handle the response code.
        SetResponse(context, response, false, p, playerId);
    }

    /// <summary>
    /// Calls the public interface in ASPlayerManager to insert the new Player to the
    /// database, this will only be done if a response was recieved from the Riot Games API
    /// indicating that the player is infact valid.
    /// </summary>
    /// <param name="context"></param>
    private void PostNewPlayer(HttpContext context)
    {
        // Check that the user is not a guest
        if (permission == ASDatabase.ASPermission.GUEST)
        {
            SetResponse(context, 401);
            return;
        }

        // Set the Input Stream to 0, this is because sometimes it is not initialised here meaning not all of the JSON
        // object is read.
        context.Request.InputStream.Position = 0;

        /*
         * Create the Serializer and read the JSON stream into a new ASPlayer object.
         */
        DataContractJsonSerializer json = new DataContractJsonSerializer(typeof(ASPlayer));
        ASPlayer p = (ASPlayer)json.ReadObject(context.Request.InputStream);
        Int32 playerId = ASPlayerManager.InsertNewPlayer(p);

        /*
         * Handle the response and write it to the client.
         */
        SetResponse(context, 200, false, p, playerId, null);
        
    }

    /// <summary>
    /// Deletes the given Player from the system at the specified URI
    /// </summary>
    /// <param name="context"></param>
    /// <param name="template"></param>
    private void DeletePlayer(HttpContext context, UriTemplateMatch template)
    {
        // Check that the user is not a guest
        if (permission == ASDatabase.ASPermission.GUEST)
        {
            SetResponse(context, 401);
            return;
        }

        // If the user isn't a guest, carry on as usual
        ASPlayer p = null;
        Int32 response = 500;
        string nation = "";
        string name = "";

        // Check if a nation has been set
        if (template.BoundVariables.AllKeys.Length != 0)
        {
            nation = template.BoundVariables["country"].ToUpper();
            name = template.BoundVariables["name"].ToUpper();
        }

        // If no nation or player name has been specified then break out
        if (string.IsNullOrEmpty(nation) || string.IsNullOrEmpty(name))
            response = 400;

        // Get the ID of the requested player set in the URI, and then pass this to the update call in
        // the ASPlayerManager handler.  The ID cannot be read from the input stream as it is unknown and therefore
        // needs to be requested seperately.
        DataContractJsonSerializer json = new DataContractJsonSerializer(typeof(IEnumerable<ASPlayer>));

        if (ASPlayerManager.GetPlayersByName(nation, name).Count != 0)
            p = ASPlayerManager.GetPlayersByName(nation, name).ElementAt(0);
        else
            response = 404;

        // Delete the player
        response = ASPlayerManager.DeletePlayer(p);

        // Handle the response code.
        SetResponse(context, response, false, p);       
    }

    /// <summary>
    /// Writes a JSON response back to the client based on the error code that is recieved
    /// </summary>
    /// <param name="context"></param>
    /// <param name="errorCode">The error code returned</param>
    /// <param name="p">The player object to format the output string</param>
    /// <param name="playerId">The players ID (optional)</param>
    /// <param name="data">List of objects to be put out to the client, normally ASPlayer objects</param>
    private void SetResponse(HttpContext context, Int32 errorCode, bool uncaught = false, ASPlayer p = null, Int32 playerId = 0, IEnumerable<ASPlayer> data = null)
    {
        // Set the header type and build output stream
        Stream outputStream = context.Response.OutputStream;
        context.Response.ContentType = "application/json";

        // Response messages
        IDictionary<string, string> responses = new Dictionary<string, string>();

        /*
         * Set all of the necessary response messages
         */
        try
        {
            string verb = context.Request.HttpMethod.ToUpper();
            switch (verb)
            {
                case "GET":
                    if (data.Count() > 1)
                    {
                        responses["500"] = "There was an internal server error when trying to retrieve the users.";
                        responses["404"] = "No players were found for on the " + p.GetLocation() + " server.";
                        responses["400"] = "Bad Request: Please ensure the location you specified is correct.";
                        responses["200"] = "Success: the users were retrieved from the system.";
                    }
                    else
                    {
                        responses["500"] = "There was an internal server error when trying to retrieve the user.";
                        responses["404"] = "The requested resource was not found on the server, please ensure the name is spelt correctly and check that you are searching the right region: /v1/player/" + p.GetLocation() + " will produce different results to /v1/player/ALL";
                        responses["400"] = "Bad Request: Please ensure the location you specified is correct, and that the path is in valid format: /v1/player/" + p.GetLocation() + "/by-name/" + p.GetName();
                        responses["200"] = "Success: the user was retrieved.";
                    }
                    break;
                case "PUT":
                    responses["500"] = "There was an internal server error when updating this player.";
                    responses["404"] = "The requested resource was not found on the server, please ensure that " + p.GetName() + " has been spelt correctly, and ensure that " + p.GetLocation() + " is the correct server.";
                    responses["400"] = "Bad Request: Please ensure the players location and name in the URI match that of your parameters: /v1/player/" + p.GetLocation().ToUpper() + "/" + p.GetName();
                    responses["200"] = "Success: the Player with ID: " + playerId + " was updated!";
                    break;
                case "POST":
                    responses["500"] = "There was an internal server error when creating the new player.";
                    responses["400"] = "Bad Request: Please ensure the path you are POSTing too is valid: /v1/player";
                    responses["200"] = "Success: the Player " + p.GetName() + " on the " + p.GetLocation() + " server was created!";
                    break;
                case "DELETE":
                    responses["500"] = "There was an internal server error when deleting this player.";
                    responses["404"] = "The requested resource was not found on the server, please ensure that " + p.GetName() + " has been spelt correctly, and ensure that " + p.GetLocation() + " is the correct server.";
                    responses["400"] = "Bad Request: Please ensure the players location and name in the URI match that of your parameters: /v1/player/" + p.GetLocation().ToUpper() + "/" + p.GetName();
                    responses["200"] = "Success: " + p.GetName() + " on the " + p.GetLocation() + " server was deleted.";
                    break;
                default:
                    responses["500"] = "There was an uncaught internal server error.";
                    responses["404"] = "The resource you requested was not valid, please check the format of your URI, it should look something like: /v1/player/{country}";
                    responses["200"] = "Success!";
                    break;
            }
        }
        // Only set an internal error state here if an unauthorized access token has not been set.
        catch (Exception e)
        {
            if(!uncaught)
                errorCode = (errorCode == 401) ? 401 : 500;
            responses["500"] = "There was an uncaught internal server error, please check your URI and try again.";
        }

        // Uncaught exception occurs when the URI 
        if (uncaught)
        {
            responses["500"] = "Internal Server Error: This URI has insufficient request handlers, for example, this URI may be restricted to GET only.";
            responses["404"] = "Resource not found: No users exist in the system";
        }

        // Unauthorized access token
        responses["401"] = "You are not authorised to access this resource, your API key is limited to GET requests.";

        // We know that the error code pertains to the key, therefore we convert it to string to produce the output
        var response = new ASResponse(errorCode, responses[errorCode.ToString()], data);

        // Write the JSON back to the client
        var json = new DataContractJsonSerializer(typeof(ASResponse));
        json.WriteObject(outputStream, response);
    }

    /// <summary>
    /// Checks the permissions for the current API key - if the key is to
    /// short then an invalid key has been sent resulting in a failed request.
    /// Requests may also be denied if the key does not have sufficient access rights
    /// outlined in the ASPermissions struct located in ASDatabase. Currently the 
    /// API only holds GUEST and ADMIN permissions, but ACL could be introduced to
    /// improve this.
    /// </summary>
    /// <param name="template"></param>
    /// <returns></returns>
    private bool CheckPermissions(UriTemplateMatch template)
    {
        string key = template.BoundVariables["apiKey"];

        // An invalid key was set
        if (key == null || key.Length != 24)
            return false;

        // Check the permissions of the key and then return true
        permission = ASKeyManager.GetPermission(key);

        return true;
    }
}