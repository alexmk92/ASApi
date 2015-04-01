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
public class ASKeyManager : ASDatabase
{
    // Blank constructor for now.
    public ASKeyManager()
    { }

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
    public static ASKey CreateKey(ASPermission p)
    {
        Int32 permission = 0;
        Int32 responseId = 500;
        Int32 nextId = 0;

        /*
         * Generate a new random API Key
         */
        var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new Random();
        var result = new string(
            Enumerable.Repeat(chars, 24)
                      .Select(s => s[random.Next(s.Length)])
                      .ToArray());

        // Check that the api key is valid - should not happen
        if (string.IsNullOrEmpty(result) || result.Length != 24)
            responseId = 400;

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
        SqlParameter key = new SqlParameter("@key", SqlDbType.VarChar, 25);
        SqlParameter perm = new SqlParameter("@permission", SqlDbType.Int);

        // Set the parameter values
        key.Value = result;
        perm.Value = permission;

        // Bind the params to the query
        query.Parameters.Add(key);
        query.Parameters.Add(perm);

        /*
         * Execute the query on the connection, if this fails return 0 to indicate a bad Request,
         * else return the ID of the newly inserted key.
         */
        using (con)
        {
            try
            {
                con.Open();
                nextId = (Int32)query.ExecuteScalar();
                // Check that a key was inserted and then insert the new key
                if (nextId != 0)
                    responseId = 200;
                con.Close();
            }
            catch (Exception e)
            {
                _lastErr = e.Message;
            }
        }

        // Return the new key
        return new ASKey(result, permission);
    }

    /// <summary>
    /// Return a list of keys.
    /// </summary>
    /// <returns></returns>
    public static List<ASKey> GetKeys()
    {
        List<ASKey> keys = new List<ASKey>();

        // Set up connection params
        SqlConnection con = new SqlConnection(_conn);
        SqlCommand query = new SqlCommand("SELECT api_key, permissions FROM Keys", con);

        // Run the Query - if the returned value of permission is 1, then return 1 (ADMIN). Else return 0 (GUEST)
        using (con)
        {
            try
            {
                con.Open();
                SqlDataReader reader = query.ExecuteReader();
                while (reader.Read())
                {
                    var key = new ASKey((string)reader["api_key"], (Int32)reader["permissions"]);
                    keys.Add(key);
                }
                con.Close();
            }
            catch (Exception e)
            {
                ASDatabase._lastErr = e.Message;
                return keys;
            }
        }
        return keys;
    }

    /// <summary>
    /// Deletes the key from the system
    /// </summary>
    /// <param name="k"></param>
    /// <returns></returns>
    public static Int32 DeleteKey(ASKey k)
    {
        Int32 responseCode = 400;
        SqlCommand query;

        // Return a Bad Request if the key object is invalid
        if (k == null || !k.IsValid())
            return responseCode;

        // Assuming we have got this far, build the query on the connection
        SqlConnection con = new SqlConnection(_conn);

        // Build the query string
        query = new SqlCommand("DELETE FROM Keys WHERE api_key = @key", con);

        // Set the parameters
        var key = new SqlParameter("@key", SqlDbType.VarChar, 50);

        // Assign values to parameters
        key.Value = k.GetKey();

        // Bind the assigned parameters to the query
        query.Parameters.Add(key);

        // Attempt to update the player on the server
        using (con)
        {
            // If this passes, then the user has been updated - prompt user with a success code
            try
            {
                con.Open();
                query.ExecuteNonQuery();
                responseCode = 200;
                con.Close();
            }
            catch (Exception e)
            {
                ASDatabase._lastErr = e.Message;
                responseCode = 500;
            }
        }

        // Returns the server code.
        return responseCode;
    }

    /// <summary>
    /// Replaces they key with a new key of that type
    /// </summary>
    /// <param name="k"></param>
    /// <returns></returns>
    public static IDictionary<ASKey, Int32> ReplaceKey(string apiKey)
    {
        IDictionary<ASKey, Int32> response = new Dictionary<ASKey, Int32>();
        
        Int32 responseCode = 500;
        SqlCommand query;

        // Set a default prior key
        ASKey oldKey = null;
        ASKey newKey = null;

        // Return a Bad Request if the key srtring is invalid
        if (string.IsNullOrEmpty(apiKey) || apiKey.Length != 24)
        {
            response.Add(null, responseCode);
            return response;
        }

        // Assuming we have got this far, build the query on the connection
        SqlConnection con = new SqlConnection(_conn);

        // Build the query string
        query = new SqlCommand("SELECT api_key, permissions FROM Keys WHERE api_key = @key", con);

        // Set the parameters
        var key = new SqlParameter("@key", SqlDbType.VarChar, 50);

        // Assign values to parameters
        key.Value = apiKey;

        // Bind the assigned parameters to the query
        query.Parameters.Add(key);

        // Attempt to update the player on the server
        using (con)
        {
            // If this passes, then the user has been updated - prompt user with a success code
            try
            {
                con.Open();
                // Check that we found a record, else we the resource was not found
                SqlDataReader reader = query.ExecuteReader();
                if (reader.Read())
                {
                    oldKey = new ASKey((string)reader["api_key"], (Int32)reader["permissions"]);
                    responseCode = 200;
                }
                else
                {
                    responseCode = 404;
                }
                con.Close();
            }
            catch (Exception e)
            {
                ASDatabase._lastErr = e.Message;
                responseCode = 500;
            }
        }

        // Check if the query worked, we can test this by checking for a valid key, if true we do final processing, else
        // we will return the last response code
        if (oldKey != null && oldKey.IsValid())
        {
            responseCode = DeleteKey(oldKey);
            if (responseCode == 200)
            {
                newKey = CreateKey(oldKey.GetPermission());
                response.Add(newKey, responseCode);
            }
            // Check that we got a valid key back, else state a bad request was mad
            if (newKey != null && newKey.IsValid())
                responseCode = 200;
            else
                responseCode = 400;
        }

        // Returns the server code.
        return response;
    }

    /// <summary>
    /// Returns the permission levels associated with the given key
    /// </summary>
    /// <param name="key"></param>
    /// <returns>ADMIN or GUEST</returns>
    public static ASPermission GetPermission(string key)
    {
        // This is a guest key
        Int32 res = 0;

        // Set up connection params
        SqlConnection con = new SqlConnection(_conn);
        SqlCommand query = new SqlCommand("SELECT permissions FROM Keys WHERE api_key = @key", con);

        // Set up the bind parameters
        var apiKey = new SqlParameter("@key", SqlDbType.VarChar, 30);

        // Set the values for the bind parameters
        apiKey.Value = key;

        // Bind the parameters to the query
        query.Parameters.Add(apiKey);

        // Run the Query - if the returned value of permission is 1, then return 1 (ADMIN). Else return 0 (GUEST)
        using (con)
        {
            try
            {
                con.Open();
                SqlDataReader reader = query.ExecuteReader();
                if (reader.Read())
                    res = (Int32)reader["permissions"];
                if (res == 1)
                    return ASPermission.ADMIN;
                con.Close();
            }
            catch (Exception e)
            {
                ASDatabase._lastErr = e.Message;
                return ASPermission.GUEST;
            }
        }

        return ASPermission.GUEST;
    }
}