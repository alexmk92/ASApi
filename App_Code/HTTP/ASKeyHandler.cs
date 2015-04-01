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
public class ASKeyHandler : ASHttpHandler
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
    private UriTemplate adminKeyTemplate = new UriTemplate("/modules/soft338/asims/v1/key/admin");
    private UriTemplate guestKeyTemplate = new UriTemplate("/modules/soft338/asims/v1/key/guest");
    private UriTemplate resetKeyTemplate = new UriTemplate("/modules/soft338/asims/v1/key/reset?api_key={apiKey}");
    private UriTemplate baseKeyTemplate  = new UriTemplate("/modules/soft338/asims/v1/key");

    private Uri requestedUri;

    // As usual, this constructor does not require any inistialisation
    public ASKeyHandler(Uri requestedUri)
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

        // Retrieves all keys
        res = baseKeyTemplate.Match(prefix, requestedUri);
        if (res != null)
            ProcessKeys(context, res);

        // Resets the given key
        res = resetKeyTemplate.Match(prefix, requestedUri);
        if (res != null)
            ProcessKey(context, res, ASDatabase.ASPermission.ADMIN);

        // Handles manging of admin keys
        res = adminKeyTemplate.Match(prefix, requestedUri);
        if (res != null)
            ProcessKey(context, res, ASDatabase.ASPermission.ADMIN);

        // Handles managing of guest keys
        res = guestKeyTemplate.Match(prefix, requestedUri);
        if (res != null)
            ProcessKey(context, res, ASDatabase.ASPermission.GUEST);

    }

    /// <summary>
    /// Handles GET, PUT and DELETE of resources on the server. 
    /// A POST request will raise a 500 error to the client
    /// </summary>
    /// <param name="context"></param>
    private void ProcessKeys(HttpContext context, UriTemplateMatch template)
    {
        // Everything was ok - continue to process the request
        string verb = context.Request.HttpMethod.ToUpper();
        switch (verb)
        {
            case "GET": GetAllKeys(context, template);
                break;
            default: SetResponse(context, 500, true, null);
                break;
        }
    }

    /// <summary>
    /// Handles the POSTing and GETting of players from the system
    /// </summary>
    /// <param name="context"></param>
    private void ProcessKey(HttpContext context, UriTemplateMatch template, ASDatabase.ASPermission p)
    {
        // Everything was ok - continue to process the request
        string verb = context.Request.HttpMethod.ToUpper();
        switch (verb)
        {
            case "POST": PostNewKey(context, p);
                break;
            case "PUT": PutKey(context, template);
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
    private void GetAllKeys(HttpContext context, UriTemplateMatch template)
    {
        // Define the output stream and set the Content Type for the header (ensure we recieve JSON back)
        Stream outputStream = context.Response.OutputStream;
        context.Response.ContentType = "application/json";

        // Create the Serializer and then get a list of all ASPlayers that match this criteria
        DataContractJsonSerializer json = new DataContractJsonSerializer(typeof(IEnumerable<ASKey>));
        IEnumerable<ASKey> keys = ASKeyManager.GetKeys();

        if (keys.Count() > 0)
            SetResponse(context, 200, false, keys.ElementAt(0), 0, keys);
        else
            SetResponse(context, 404, true, null);
    }

    /// <summary>
    /// Calls the public interface in ASKeyManager to insert the new Player to the
    /// database, this will only be done if a response was recieved from the Riot Games API
    /// indicating that the player is infact valid.
    /// </summary>
    /// <param name="context"></param>
    private void PostNewKey(HttpContext context, ASDatabase.ASPermission permission)
    {
        // Create the new key
        ASKey k = ASKeyManager.CreateKey(permission);

        /*
         * Handle the response and write it to the client.
         */
        if (k == null || !k.IsValid())
        {
            SetResponse(context, 400, false, k);
            return;
        }

        // Everything succeeded
        SetResponse(context, 200, false, k);
    }

    /// <summary>
    /// Updates the API key by querying the ASKeyManager for the current set key set in the
    /// query param of the URI.  If an API key is set, the old key will be deleted and a new
    /// one shall be generated. This will return a response code use to trigger the output.
    /// </summary>
    /// <param name="context"></param>
    private void PutKey(HttpContext context, UriTemplateMatch template)
    {
        // Assume an internal error
        Int32 response = 500;
        IDictionary<ASKey, Int32> res = new Dictionary<ASKey, Int32>();
        List<ASKey> keys = new List<ASKey>();
        string key = "";

        // Check if a nation has been set
        if (template.BoundVariables.AllKeys.Length != 0)
            key = template.BoundVariables["apiKey"];

        // If no key has been specified then this is a bad request
        if (string.IsNullOrEmpty(key) || key.Length < 24)
        {
            SetResponse(context, 400);
            return;
        }

        // Attempt to get result
        res = ASKeyManager.ReplaceKey(key);

        // If no items were returned - this is an internal error
        if (res.Count == 0)
        {
            SetResponse(context, 500);
            return;
        }

        // Add the key to the data output
        keys.Add(res.ElementAt(0).Key);

        // Success - send the respnse from the 
        SetResponse(context, res.ElementAt(0).Value, false, res.ElementAt(0).Key, 0, keys);
    }

    /// <summary>
    /// Deletes the given Player from the system at the specified URI
    /// </summary>
    /// <param name="context"></param>
    /// <param name="template"></param>
    private void DeleteKey(HttpContext context, UriTemplateMatch template)
    {
        // If the user isn't a guest, carry on as usual
        ASKey k = null;
        Int32 response = 500;
        string key = "";

        // Check if a nation has been set
        if (template.BoundVariables.AllKeys.Length != 0)
            key = template.BoundVariables["apiKey"];

        // If no nation or player name has been specified then break out
        if (string.IsNullOrEmpty(key) || key.Length < 24)
            response = 400;

        // Get the ID of the requested key set in the URI, and then pass this to the update call in
        // the ASKeyManager handler.  The ID cannot be read from the input stream as it is unknown and therefore
        // needs to be requested seperately.
        DataContractJsonSerializer json = new DataContractJsonSerializer(typeof(IEnumerable<ASKey>));

        if (ASKeyManager.GetKeys().Count != 0)
            k = ASKeyManager.GetKeys().ElementAt(0);
        else
            response = 404;

        // Delete the player
        response = ASKeyManager.DeleteKey(k);

        // Handle the response code.
        SetResponse(context, response, false, k);
    }

    /// <summary>
    /// Writes a JSON response back to the client based on the error code that is recieved
    /// </summary>
    /// <param name="context"></param>
    /// <param name="errorCode">The error code returned</param>
    /// <param name="p">The player object to format the output string</param>
    /// <param name="playerId">The players ID (optional)</param>
    /// <param name="data">List of objects to be put out to the client, normally ASPlayer objects</param>
    private void SetResponse(HttpContext context, Int32 errorCode, bool uncaught = false, ASKey k = null, Int32 keyId = 0, IEnumerable<ASKey> data = null)
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
                        responses["500"] = "There was an internal server error when trying to retrieve the keys.";
                        responses["404"] = "No keys were found on the server.";
                        responses["200"] = "Success: the keys were retrieved from the system.";
                    }
                    else
                    {
                        responses["500"] = "There was an internal server error when trying to retrieve the user.";
                        responses["404"] = "The requested resource was not found on the server";
                        responses["200"] = "Success: the key was retrieved.";
                    }
                    break;
                case "PUT":
                    responses["500"] = "There was an internal server error when updating this key.";
                    responses["404"] = "The requested key was not found on the server";
                    responses["400"] = "Bad Request: Please ensure you specified the correct key";
                    responses["200"] = "Success: your new key is " + k.GetKey() + ", it has " + k.GetPermissionAsString() + " permissions.";
                    break;
                case "POST":
                    responses["500"] = "There was an internal server error when creating the new key.";
                    responses["400"] = "Bad Request: Please ensure the path you are POSTing too is valid: /v1/key";
                    responses["200"] = "Success: the new " + k.GetPermissionAsString() + " key: " + k.GetKey() + " was added to the system.";
                    break;
                case "DELETE":
                    responses["500"] = "There was an internal server error when deleting this key.";
                    responses["404"] = "The requested key was not found on the server";
                    responses["400"] = "Bad Request: Please ensure you specified the correct key";
                    responses["200"] = "Success: the key was deleted.";
                    break;
                default:
                    responses["500"] = "There was an uncaught internal server error.";
                    responses["404"] = "The resource you requested was not valid, please check the format of your URI, it should look something like: /v1/key";
                    responses["200"] = "Success!";
                    break;
            }
        }
        // Only set an internal error state here if an unauthorized access token has not been set.
        catch (Exception e)
        {
            if (!uncaught)
            {
                if(errorCode != 404 || errorCode != 400 || errorCode != 200)
                    errorCode = 500;
                responses["500"] = "There was an uncaught internal server error, please check your URI and try again.";
            }
        }

        // Uncaught exception occurs when the URI 
        if (uncaught)
        {
            responses["500"] = "Internal Server Error: This URI has insufficient request handlers, for example, this URI may be restricted to GET only.";
            responses["404"] = "Resource not found: No keys exist in the system";
        }

        // Unauthorized access token
        responses["401"] = "You are not authorised to access this resource, your API key is limited to GET requests.";

        // We know that the error code pertains to the key, therefore we convert it to string to produce the output
        var response = new ASKeyResponse(errorCode, responses[errorCode.ToString()], data);

        // Write the JSON back to the client
        var json = new DataContractJsonSerializer(typeof(ASKeyResponse));
        json.WriteObject(outputStream, response);
    }
}