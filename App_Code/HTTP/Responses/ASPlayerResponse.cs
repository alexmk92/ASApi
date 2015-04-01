using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;

/// <summary>
/// The ASResponse object formats input information and places it into
/// a JSON array. This can then be parsed by browser to output JSON objects
/// to the user
/// </summary>
[DataContract]
public class ASPlayerResponse : ASResponse
{
    [DataMember]
    private IEnumerable<ASPlayer> players;

    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="code"></param>
    /// <param name="msg"></param>
    /// <param name="data"></param>
    public ASPlayerResponse(Int32 code, string msg, IEnumerable<ASPlayer> data)
    {
        if (!SetResponseData(code, msg, data))
            throw new ArgumentException("Please ensure you specify a positive integer, non-empty string and some return data.");
    }

    /// <summary>
    /// Only code and msg constructor
    /// </summary>
    /// <param name="code"></param>
    /// <param name="msg"></param>
    public ASPlayerResponse(Int32 code, string msg)
    {
        if (!SetResponseData(code, msg, null))
            throw new ArgumentException("Please ensure you specify a positive integer, non-empty string and some return data.");
    }

    /// <summary>
    /// Checks if the response data sent to the ASReponse class was valid.
    /// If it was then true is returned, else false.
    /// Ideally this should never happen but its best to catch the event
    /// </summary>
    /// <param name="code">The error code</param>
    /// <param name="msg">The error message</param>
    /// <param name="data">Options parameter of al data returned</param>
    /// <returns>True if successful, else false</returns>
    public bool SetResponseData(Int32 code, string msg, IEnumerable<ASPlayer> data = null)
    {
        if (code < 0)
            return false;
        if (string.IsNullOrEmpty(msg))
            return false;
        if (data == null)
            data = new List<ASPlayer>();

        // All checks passed, set parameters
        this.errCode = code;
        this.errMsg  = msg;
        this.players = data;

        return true;
    }
}