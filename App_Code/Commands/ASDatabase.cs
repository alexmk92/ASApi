using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Web.Configuration;
using System.Data.SqlClient;

/// <summary>
/// Parent class of ASGameManager, ASPlayerManager and ASTeamManager, ASUser. If all of the aforementioned
/// classes were bundled into one package then the DB class would become extremely 
/// cumbersome, therefore it makes sense to have base classes extend it.
/// </summary>
public class ASDatabase
{
    // Used for registering new users and checking permissions on method calls
    public enum ASPermission { GUEST, ADMIN }

    // Declare the connection string
    protected static string _conn    = WebConfigurationManager.ConnectionStrings["ASConnection"].ConnectionString;
    public    static string _lastErr = "";

    // Empty constructor as connections are handled on a per transaction basis, a 
    // connection is not persisted once a transaction has been made as this is a potential
    // security flaw.
	public ASDatabase()
	{}
}