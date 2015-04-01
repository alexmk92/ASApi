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
public class ASKeyResponse : ASResponse
{
    [DataMember]
    private IEnumerable<ASKey> keys;

    public ASKeyResponse(Int32 code, string msg, IEnumerable<ASKey> data)
    {
        if (!SetResponseData(code, msg, data))
            throw new ArgumentException("Please ensure you specify a positive integer, non-empty string and some return data.");
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