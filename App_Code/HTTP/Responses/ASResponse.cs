using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;

/// <summary>
/// Private interface, implemented by child response objects.
/// </summary>
[DataContract]
public class ASResponse
{
    [DataMember]
    protected Int32  errCode;
    [DataMember]
    protected string errMsg;

    public ASResponse() 
    { }
}