using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;
using System.IO;
using System.Runtime.Serialization.Json;

/// <summary>
/// Parent HTTP Handler used to handle requests to the API.  
/// This Handler is responsible for delegating calls to child handlers such
/// as ASPlayerHandler, ASTeamHandler and ASGameHandler.  Calls are delegated
/// by matching 
/// </summary>
public class ASHttpHandler : IHttpHandler
{
    // Environment configuration constants - used for testing and deployment
    const string _HOST   = "xserve.uopnet.plymouth.ac.uk";
    const string _LOCAL  = "localhost:50557";

    // Plug the configuration constant in here...
    protected Uri prefix = new Uri(_LOCAL);

    // Base URI Templates are defined here, a simple String.Contains() is performed here to
    // delegate the context to the appropriate controller.  Once in the controller the 
    // URI Template is matched
    protected UriTemplate playerTemplate = new UriTemplate("/modules/soft338/asims/v1/game");
    protected UriTemplate teamTemplate   = new UriTemplate("/modules/soft338/asims/v1/team");
    protected UriTemplate gameTemplate   = new UriTemplate("/modules/soft338/asims/v1/game");


    /// <summary>
    /// The constructor is not used to perform any additional initialisation in the
    /// scope of this program.
    /// </summary>
    public ASHttpHandler()
    { }

    /// <summary>
    /// Allows a single instance of this handler to deal with multiple concurrent
    /// calls.  The instance is cached instead of recreated.
    /// </summary>
    public bool IsReusable { get { return true; } }

    /// <summary>
    /// Recieves the inbound HttpContext and determines which UriTemplate it matches, upon a successful
    /// match the context is delegated to the appropriate HTTP handler
    /// </summary>
    public void ProcessRequest(HttpContext context)
    {
        // Construct the full request URI to be passed to the delegate controller
        Uri requestedUri = new Uri(prefix + context.Request.RawUrl);

        // Construct a URI String to check which controller to delegate to
        string URIString = requestedUri.ToString().ToLower();

        /*
         * ---------------------------------------------------------------------------
         * Player template matching
         * ---------------------------------------------------------------------------
         * Attempts to match any of the player templates, if successful then delegate 
         * a new request to the ASGameHandler
         * ---------------------------------------------------------------------------
         */
        if (URIString.Contains("/modules/soft338/asims/v1/player"))
            new ASPlayerHandler(requestedUri).HandleRequest(context);

        /*
        * ---------------------------------------------------------------------------
        * Team template matching
        * ---------------------------------------------------------------------------
        * Attempts to match any of the team templates, if successful then delegate 
        * a new request to the ASTeamHandler
        * ---------------------------------------------------------------------------
        */


        /*
        * ---------------------------------------------------------------------------
        * Player template matching
        * ---------------------------------------------------------------------------
        * Attempts to match any of the player templates, if successful then delegate 
        * a new request to the ASGameHandler
        * ---------------------------------------------------------------------------
        */

        ASUser.CreateKey(ASDatabase.ASPermission.ADMIN);
    }
}