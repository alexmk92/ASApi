using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
using System.IO;
using System.Runtime.Serialization.Json;

/// <summary>
/// Extends the ASHttpHandler class, this module is responsible for
/// handling any requests to the game path
/// </summary>
public class ASGameHandler : ASHttpHandler
{
	public ASGameHandler()
	{}
    /*
    private void GetAllGames(HttpContext context)
    {
        Stream outputStream = context.Response.OutputStream;
        context.Response.ContentType = "application/json";

        // Create the serializer
        DataContractJsonSerializer json = new DataContractJsonSerializer(typeof(IEnumerable<Object>));
        IEnumerable<Object> games = ASGameManager.GetAllGames();
        json.WriteObject(outputStream, games);
    }
     */
}