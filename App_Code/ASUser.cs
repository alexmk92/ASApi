using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Web.Configuration;
using System.Data.SqlClient;

/// <summary>
/// The ASUser class is responsible for assigning keys to a user, users can reset their
/// Key by calling the CreateKey method. Users are able to retrieve their Key through
/// this interface to aid the documentation.
/// </summary>
public class ASUser : ASDatabase
{
    // Blank constructor for now.
	public ASUser()
	{}

    /// <summary>
    /// Creates a new API Key based on the permissions delegated to the user.
    /// By default the key generated will be a GUEST (PUBLIC) key, allowing anybody
    /// to make GET requests to the server.
    /// An ADMIN can escalate this permission later to allow users to have full control
    /// such as PUT, POST, GET, DELETE - I am thinking of adding ACL further down the
    /// line to restrict the amount of control
    /// </summary>
    /// <param name="p">Either ADMIN or GUEST, converts to 1 or 0</param>
    /// <returns>Int32 - The ID of the newly inserted key, else 0</returns>
    public static Int32 CreateKey(ASPermission p)
    {
        Int32 permission = 0;
        Int32 returnID   = 0;

        /*
         * Generate a new random API Key
         */
        var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new Random();
        var result = new string(
            Enumerable.Repeat(chars, 24)
                      .Select(s => s[random.Next(s.Length)])
                      .ToArray());

        /*
         * Determine the Permission of the User
         */
        switch (p)
        {
            case ASPermission.GUEST:
                permission = 0;
                break;
            case ASPermission.ADMIN:
                permission = 1;
                break;
            default:
                permission = 0;
                break;
        }

        /*
         * Transact with the DB to insert the new record, using bind params to ensure the
         * input info is safe.
         */ 
        SqlConnection con = new SqlConnection(_conn);
        SqlCommand query = new SqlCommand("INSERT INTO Keys (api_key, permissions) VALUES(@key, @permission);" + "SELECT CAST(Scope_Identity() as int)", con);

        // Create parameters
        SqlParameter key  = new SqlParameter("@key", SqlDbType.VarChar, 25);
        SqlParameter perm = new SqlParameter("@permission", SqlDbType.Int);

        // Set the parameter values
        key.Value  = result;
        perm.Value = permission;

        // Bind the params to the query
        query.Parameters.Add(key);
        query.Parameters.Add(permission);

        /*
         * Execute the query on the connection, if this fails return 0 to indicate a bad Request,
         * else return the ID of the newly inserted key.
         */
        using (con)
        {
            try
            {
                con.Open();
                returnID = (Int32)query.ExecuteScalar();
                con.Close();
            }
            catch (Exception e)
            {
                _lastErr = e.Message;
            }
        }

        return returnID;
    }
}