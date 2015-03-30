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
    * @template playersByNameTemplate : used for GET or DELETE of specific players in a country
    * @template allPlayersTemplate    : used for GET or DELETE of all players in a country
    * @template basePlayerTemplate    : used to POST a new player to the API
    * 
    * ---------------------------------------------------------------------------
    */
    private UriTemplate playerByNameTemplate  = new UriTemplate("/modules/soft338/asims/v1/player/{country}/by-name/{name}");
    private UriTemplate playersByNameTemplate = new UriTemplate("/modules/soft338/asims/v1/player/{country}/by-names/{names}");
    private UriTemplate allPlayersTemplate    = new UriTemplate("/modules/soft338/asims/v1/player/{country}");
    private UriTemplate basePlayerTemplate    = new UriTemplate("/modules/soft338/asims/v1/player");

    private Uri requestedUri;

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
            ProcessPlayer(context);
        
        // Handles the retrieval of all players for a given country or all countries
        // This request is restricted to GET only
        res = allPlayersTemplate.Match(prefix, requestedUri);
        if (res != null)
            ProcessPlayers(context);

        res = playersByNameTemplate.Match(prefix, requestedUri);
        if (res != null)
       

        res = playerByNameTemplate.Match(prefix, requestedUri);
        if (res != null)
            ProcessPlayerByName(context);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="context"></param>
    private void ProcessPlayerByName(HttpContext context)
    { 
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="context"></param>
    private void ProcessPlayersByNames(HttpContext context)
    {
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="context"></param>
    private void ProcessPlayer(HttpContext context)
    {
        string verb = context.Request.HttpMethod.ToUpper();

        switch (verb)
        {
            case "POST": PostNewPlayer(context);
                break;
            default: RaiseError("Sorry, this URI can only perform POST operations.");
                break;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="context"></param>
    private void ProcessPlayers(HttpContext context)
    {
        string verb = context.Request.HttpMethod.ToUpper();

        switch (verb)
        {
            case "GET": GetAllPlayers(context);
                break;
            default:    RaiseError("Sorry, this URI can only perform GET operations.");
                break;
        }
    }

    /// <summary>
    /// Calls the interface in ASPlayerManager to retrieve a serialized list of all players
    /// </summary>
    /// <param name="context"></param>
    private void GetAllPlayers(HttpContext context)
    {
        // Define the output stream and set the Content Type for the header (ensure we recieve JSON back)
        Stream outputStream = context.Response.OutputStream;
        context.Response.ContentType = "application/json";

        string country = allPlayersTemplate.Segments[6];

        // Create the Serializer and then get a list of all ASPlayers that match this criteria
        DataContractJsonSerializer json = new DataContractJsonSerializer(typeof(IEnumerable<ASPlayer>));
        IEnumerable<ASPlayer> players = ASPlayerManager.GetAllPlayers("EUW");
        json.WriteObject(outputStream, players);
    }

    /// <summary>
    /// Calls the public interface in ASPlayerManager to insert the new Player to the
    /// database, this will only be done if a response was recieved from the Riot Games API
    /// indicating that the player is infact valid.
    /// </summary>
    /// <param name="context"></param>
    private void PostNewPlayer(HttpContext context)
    {
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
        HttpResponse Response = context.Response;
        Response.Write("<html>");
        Response.Write("<body>");
        Response.Write("<h1>SOFT338 Response<h1>");
        Response.Write("<p>This is the response from the module path and the POST verb</p>");
        Response.Write("<p>Player Name : " + p.GetName() + "; Player Wins : " + p.GetWins() + "; Player Losses : " + p.GetLosses() + "</p>");
        Response.Write("<p>Player Points : " + p.GetPoints() + ", Player ID is " + playerId + ".  Errors caught: " + ASDatabase._lastErr + "</p>");
        Response.Write("</body>");
        Response.Write("</html>");
        
    }

    private void RaiseError(string error)
    { }
}