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

    public static List<ASPlayer> GetAllPlayers(string country)
    {
        List<ASPlayer> players = new List<ASPlayer>();



        return players;
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