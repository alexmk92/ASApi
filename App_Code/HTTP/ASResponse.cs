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
public class ASResponse
{
    // Encapsulate variables
    [DataMember]
    private IEnumerable<ASPlayer> data;
    [DataMember]
    private IEnumerable<ASKey> keys;
    [DataMember]
    private Int32  errCode;
    [DataMember]
    private string errMsg;

    /// <summary>
    /// Calls the private interface to validate and set the local variables
    /// If the validation check returns false an Argument Exception will be
    /// raised.
    /// 
    /// @param Int32        - The error code returned from the server
    /// @param string       - The error message associated with the code
    /// @param List<object> - A list holding all response data
    /// </summary>
    public ASResponse(Int32 code, string msg, IEnumerable<ASPlayer> data)
	{
        if(!SetResponseData(code, msg, data))
            throw new ArgumentException("Please ensure you specify a positive integer, non-empty string and some return data.");
	}

    public ASResponse(Int32 code, string msg, IEnumerable<ASKey> data)
    {
        if (!SetResponseData(code, msg, data))
            throw new ArgumentException("Please ensure you specify a positive integer, non-empty string and some return data.");
    }

    /*
    /// <summary>
    /// This 
    /// </summary>
    /// <param name="code"></param>
    /// <param name="msg"></param>
    public ASResponse(Int32 code, string msg)
    {
        if (!SetResponseData(code, msg, null))
            throw new ArgumentException("Please ensure you specify a positive integer, non-empty string and some return data.");
    }
    */

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
        this.errMsg = msg;
        this.data = data;

        return true;
    }

    /// <summary>
    /// Handles responses for ASKeys
    /// </summary>
    /// <param name="code"></param>
    /// <param name="msg"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    public bool SetResponseData(Int32 code, string msg, IEnumerable<ASKey> data = null)
    {
        if (code < 0)
            return false;
        if (string.IsNullOrEmpty(msg))
            return false;
        if (data == null)
            data = new List<ASKey>();

        // All checks passed, set parameters
        this.errCode = code;
        this.errMsg = msg;
        this.keys = data;

        return true;
    }
}