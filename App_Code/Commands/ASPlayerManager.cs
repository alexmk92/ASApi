using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Web.Configuration;
using System.Data.SqlClient;

/// <summary>
/// The ASPlayerManager extends the ASDatabase instance to provide a managed interface to 
/// transact with the Server.  If the application was larger many classes may exist, meaning
/// the ASDatabase namespace would become far too bloated
/// </summary>
public class ASPlayerManager : ASDatabase
{
    // Empty constructor as nothing needs to be assigned here.
	public ASPlayerManager()
	{}

    /// <summary>
    /// Retrieves a list of ASPlayer objects depending on whether the user is in 
    /// a specifc country or not.  If no string is specified or ALL is set, then all 
    /// users will be returned.
    /// </summary>
    /// <param name="country"></param>
    /// <returns></returns>
    public static List<ASPlayer> GetAllPlayers(string country)
    {
        List<ASPlayer> players = new List<ASPlayer>();

        SqlConnection con = new SqlConnection(_conn);
        SqlCommand query;

        /*
         * Check the input parameter and see which query we need to run
         */ 
        if (String.IsNullOrEmpty(country) || country == "ALL")
        {
            query = new SqlCommand("SELECT player_id, player_name, player_location, player_wins, player_draws, player_losses, player_points FROM Players", con);
        }
        else
        {
            query = new SqlCommand("SELECT player_id, player_name, player_location, player_wins, player_draws, player_losses, player_points FROM Players WHERE "
                                 + "player_location = @location", con);
            // Bind the parameters
            var location = new SqlParameter("@location", SqlDbType.VarChar, 50);

            location.Value = country;
            query.Parameters.Add(location);
        }
        /*
         * Try and access the connection to retrieve information from the server, the output
         * of this method will determine the response to the client
         */
        using (con)
        {
            try
            {
                con.Open();
                // After opening the connection, build a new player object for every value returned
                SqlDataReader reader = query.ExecuteReader();
                while (reader.Read())
                {
                    ASPlayer p = new ASPlayer((string)reader["player_name"], (string)reader["player_location"], (int)reader["player_id"], (int)reader["player_wins"],
                                              (int)reader["player_draws"], (int)reader["player_losses"], (int)reader["player_points"]);

                    players.Add(p);
                }
                con.Close();
            }
            catch (Exception e)
            {
                ASDatabase._lastErr = e.Message;
            }
        }

        return players;
    }

    /// <summary>
    /// Retrieves a list of ASPlayer objects depending on whether the user is in 
    /// a specifc country or not.  If no string is specified or ALL is set, then all 
    /// users will be returned.
    /// The reason that this method returns a List is because a user may specify no
    /// country or select ALL countries, this would then mean there is a possibility
    /// for multiple players with the same name to be returned.
    /// </summary>
    /// <param name="country"></param>
    /// <param name="pName"></param>
    /// <returns></returns>
    public static List<ASPlayer> GetPlayersByName(string country, string pName)
    {
        List<ASPlayer> players = new List<ASPlayer>();

        SqlConnection con = new SqlConnection(_conn);
        SqlCommand query;

        /*
         * Check the input parameter and see which query we need to run
         */
        if (String.IsNullOrEmpty(country) || country == "ALL")
        {
            query = new SqlCommand("SELECT player_id, player_name, player_location, player_wins, player_draws, player_losses, player_points FROM Players"
                                 + " WHERE player_name = @name", con);

            // Set and bind the parameters
            var name = new SqlParameter("@name", SqlDbType.VarChar, 50);
            name.Value = pName;
            query.Parameters.Add(name);
        }
        else
        {
            query = new SqlCommand("SELECT player_id, player_name, player_location, player_wins, player_draws, player_losses, player_points FROM Players WHERE "
                                 + "player_name = @name AND player_location = @location", con);
            // Bind the parameters
            var location = new SqlParameter("@location", SqlDbType.VarChar, 50);
            var name     = new SqlParameter("@name", SqlDbType.VarChar, 50);

            // Set the value of the parameters
            location.Value = country;
            name.Value     = pName;
           
            // Add the parameters
            query.Parameters.Add(name);
            query.Parameters.Add(location);

        }
        /*
         * Try and access the connection to retrieve information from the server, the output
         * of this method will determine the response to the client
         */
        using (con)
        {
            try
            {
                con.Open();
                // After opening the connection, build a new player object for every value returned
                SqlDataReader reader = query.ExecuteReader();
                while (reader.Read())
                {
                    ASPlayer p = new ASPlayer((string)reader["player_name"], (string)reader["player_location"], (int)reader["player_id"], (int)reader["player_wins"],
                                              (int)reader["player_draws"], (int)reader["player_losses"], (int)reader["player_points"]);

                    players.Add(p);
                }
                con.Close();
            }
            catch (Exception e)
            {
                ASDatabase._lastErr = e.Message;
            }
        }

        return players;
    }

    /// <summary>
    /// Update the player passed with the new information for that player.
    /// Return the response ID from the server so we can tell the user if anything
    /// bad has happened.
    /// </summary>
    /// <param name="player"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    public static Int32 UpdatePlayer(ASPlayer player, Int32 id)
    {
        Int32 responseCode = 400;
        SqlCommand query;

        // Return a Bad Request if the player object is invalid
        if (!player.IsValid() || id <= 0)
            return responseCode;

        // Assuming we have got this far, build the query on the connection
        SqlConnection con = new SqlConnection(_conn);

        // Build the query string
        query = new SqlCommand("UPDATE Players SET player_name = @pName, player_location = @pLocation, player_wins = @pWins, "
                             + " player_draws = @pDraws, player_losses = @pLosses, player_points = @pPoints WHERE player_id = @pID", con);
        

        // Set the parameters
        var pName     = new SqlParameter("@pName", SqlDbType.VarChar, 50);
        var pLocation = new SqlParameter("@pLocation", SqlDbType.VarChar, 50);
        var pWins     = new SqlParameter("@pWins", SqlDbType.Int);
        var pDraws    = new SqlParameter("@pDraws", SqlDbType.Int);
        var pLosses   = new SqlParameter("@pLosses", SqlDbType.Int);
        var pPoints   = new SqlParameter("@pPoints", SqlDbType.Int);
        var pID       = new SqlParameter("@pID", SqlDbType.Int);

        // Assign values to parameters
        pName.Value     = player.GetName();
        pLocation.Value = player.GetLocation();
        pWins.Value     = player.GetWins();
        pDraws.Value    = player.GetDraws();
        pLosses.Value   = player.GetLosses();
        pPoints.Value   = player.GetPoints();
        pID.Value       = id;

        // Bind the assigned parameters to the query
        query.Parameters.Add(pName);
        query.Parameters.Add(pLocation);
        query.Parameters.Add(pWins);
        query.Parameters.Add(pDraws);
        query.Parameters.Add(pLosses);
        query.Parameters.Add(pPoints);
        query.Parameters.Add(pID);

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
    /// Deletes the player from the system
    /// </summary>
    /// <param name="player"></param>
    /// <returns></returns>
    public static Int32 DeletePlayer(ASPlayer player)
    {
        Int32 responseCode = 400;
        SqlCommand query;

        // Return a Bad Request if the player object is invalid
        if (player == null || !player.IsValid())
            return responseCode;

        // Assuming we have got this far, build the query on the connection
        SqlConnection con = new SqlConnection(_conn);

        // Build the query string
        query = new SqlCommand("DELETE FROM Players WHERE player_name = @pName AND player_location = @pLocation ", con);

        // Set the parameters
        var pName = new SqlParameter("@pName", SqlDbType.VarChar, 50);
        var pLoc  = new SqlParameter("@pLocation", SqlDbType.VarChar, 50);

        // Assign values to parameters
        pName.Value = player.GetName();
        pLoc.Value  = player.GetLocation();

        // Bind the assigned parameters to the query
        query.Parameters.Add(pName);
        query.Parameters.Add(pLoc);

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
    /// Inserts a new player to the system only if the player object is valid, and if 
    /// the player does not already exist in the system.  This method makes a call to
    /// the third party API to check if the player is in fact real.
    /// </summary>
    /// <param name="player">The player we wish to insert</param>
    /// <returns></returns>
    public static Int32 InsertNewPlayer(ASPlayer player)
    {
        Int32 returnID = 0;

        // Check if the player exists on the 3rd party API


        // Check we are not placing duplicate data
        if (!DoesPlayerExist(player) && player.IsValid())
        {
            // Create a new connection to the database and populate query parameters with bind variables.
            // This will ensure that the input going to the server is secure.
            var con   = new SqlConnection(_conn);
            var query = new SqlCommand("INSERT INTO Players (player_name, player_wins, player_draws, player_losses, player_location, player_points) "
                                             + "VALUES(@playerName, @playerWins, @playerDraws, @playerLosses, @playerLocation, @playerPoints);"
                                             + "SELECT CAST(Scope_Identity() as int)", con);

            // Create the bind parameters
            var pName     = new SqlParameter("@playerName", SqlDbType.VarChar, 50);
            var pLocation = new SqlParameter("@playerLocation", SqlDbType.VarChar, 50);
            var pWins     = new SqlParameter("@playerWins", SqlDbType.Int);
            var pDraws    = new SqlParameter("@playerDraws", SqlDbType.Int);
            var pLosses   = new SqlParameter("@playerLosses", SqlDbType.Int);
            var pPoints   = new SqlParameter("@playerPoints", SqlDbType.Int);

            // Assign values to the object
            pName.Value = player.GetName();
            pLocation.Value = player.GetLocation();
            pWins.Value = player.GetWins();
            pDraws.Value = player.GetDraws();
            pLosses.Value = player.GetLosses();
            pPoints.Value = player.GetPoints();

            // Bind the set parameters to the query
            query.Parameters.Add(pName);
            query.Parameters.Add(pLocation);
            query.Parameters.Add(pWins);
            query.Parameters.Add(pDraws);
            query.Parameters.Add(pLosses);
            query.Parameters.Add(pPoints);

            // Try and open the connection, if successful INSERT the new player and then return their newly inserted ID.
            // we can then return this to the client.  If 0 is returned then there is a problem - we can log this as a
            // 400 Bad Request
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
        }

        return returnID;
    }

    /// <summary>
    /// Checks if a player already exists in the system, if it does this method returns True meaning that the
    /// parent call will send an error 400 back to the client. 
    /// </summary>
    /// <param name="player">The player we are checking</param>
    /// <returns></returns>
    private static bool DoesPlayerExist(ASPlayer player)
    {
        SqlConnection con = new SqlConnection(_conn);
        SqlCommand query = new SqlCommand("SELECT player_name, player_location FROM Players WHERE player_name = @playerName AND player_location = @playerLocation", con);

        // Set up the bind parameters
        var pName     = new SqlParameter("@playerName", SqlDbType.VarChar, 50);
        var pLocation = new SqlParameter("@playerLocation", SqlDbType.VarChar, 50);

        // Set the values for the bind parameters
        pName.Value     = player.GetName();
        pLocation.Value = player.GetLocation();

        // Bind the parameters to the query
        query.Parameters.Add(pName);
        query.Parameters.Add(pLocation);

        // Run the Query
        using (con)
        {
            try
            {
                con.Open();
                SqlDataReader reader = query.ExecuteReader();
                if (reader.Read())
                    return true;
                con.Close();
            }
            catch (Exception e)
            {
                ASDatabase._lastErr = e.Message;
                return false;
            }
        }

        return false;
    }
}