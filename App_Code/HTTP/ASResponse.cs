using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// The ASResponse object formats input information and places it into
/// a JSON array. This can then be parsed by browser to output JSON objects
/// to the user
/// </summary>
public class ASResponse
{
    // Encapsulate variables
    private List<object> data;
    private Int32  errCode;
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
    public ASResponse(Int32 code, string msg, List<object> data)
	{
        if(!SetResponseData(code, msg, data))
            throw new ArgumentException("Please ensure you specify a positive integer, non-empty string and some return data.");
	}

    /// <summary>
    /// Checks if the response data sent to the ASReponse class was valid.
    /// If it was then true is returned, else false.
    /// Ideally this should never happen but its best to catch the event
    /// 
    /// @param Int32        - The error code returned from the server
    /// @param string       - The error message associated with the code
    /// @param List<object> - A list holding all response data
    /// 
    /// @return bool - True if passed, else false
    /// </summary>
    private bool SetResponseData(Int32 code, string msg, List<object> data)
    {
        if (code < 0)
            return false;
        if (string.IsNullOrEmpty(msg))
            return false;
        if (data == null)
            return false;

        // All checks passed, set parameters
        this.errCode = code;
        this.errMsg = msg;
        this.data = data;

        return true;
    }

    /// <summary>
    /// Serializes the input data into a JSON string which is sent back
    /// to the client.
    /// 
    /// @return string - We know the object is valid after initialisation, so 
    /// the return string will always contain the JSON string
    /// </summary>
    public List<Object> ResponseToJSON()
    {
        var response = new List<Object>();
        response.Add(errCode);
        response.Add(errMsg);
        response.Add(data);

        return response;
    }


}