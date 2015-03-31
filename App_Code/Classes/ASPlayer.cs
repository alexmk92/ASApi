using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;

/// <summary>
/// Summary description for Team
/// </summary>
/// 
[DataContract]
public class ASPlayer
{
    [DataMember]
    private string player_name;
    [DataMember]
    private string player_location;
    [DataMember]
    private Int32 player_id;
    [DataMember]
    private Int32 player_wins;
    [DataMember]
    private Int32 player_draws;
    [DataMember]
    private Int32 player_losses;
    [DataMember]
    private Int32 player_points;

    /// <summary>
    /// Initialises all encapsulated variables, and check their validity to create
    /// the new team object.
    /// 
    /// <parm name="name">Name of the Player</parm>
    /// <parm name="location">Continent of the player</parm>
    /// <parm name="id">Id of the player</parm>
    /// <parm name="wins">Number of Wins for the player</parm>
    /// <parm name="draws">Number of Draws for the player</parm>
    /// <param name="losses">Number of losses for the player</param>
    /// <param name="points">Number of points for the player</param>
    /// 
    /// </summary>
    public ASPlayer(string name, string location, Int32 id, Int32 wins, Int32 draws, Int32 losses, Int32 points)
    {
        // Perform a double validity check 
        if (!SetPlayer(name, location, id, wins, draws, losses, points) && !this.IsValid())
            throw new ArgumentException("There was an error when initialising ASPlayer, please ensure you pass positive integers and non-empty strings");
    }

    /// <summary>
    /// Initialises all encapsulated variables, and check their validity to create
    /// the new player object. This object does not recieve an ID as it will be used to store new users
    /// 
    /// <parm name="name">Name of the Player</parm>
    /// <parm name="location">Continent of the player</parm>
    /// <parm name="id">Pass in a blank ID for object not retrieved from DB.</parm>
    /// <parm name="wins">Number of Wins for the player</parm>
    /// <parm name="draws">Number of Draws for the player</parm>
    /// <param name="losses">Number of losses for the player</param>
    /// <param name="points">Number of points for the player</param>
    /// 
    /// </summary>
    public ASPlayer(string name, string location, Int32 wins, Int32 draws, Int32 losses, Int32 points)
    {
        if (!SetPlayer(name, location, 0, wins, draws, losses, points) && !this.IsValid())
            throw new ArgumentException("There was an error when initialising ASPlayer, please ensure you pass positive integers and non-empty strings");
    }

    /// <summary>
    /// Sets the new team object, replicates most of the checks performed
    /// in the public interface to check the objects internal state. The
    /// setter method is private as the objects state is immutable once
    /// it has been created. 
    /// 
    /// @param string - Input player name to check
    /// @param string - Input player location to check
    /// @param Int32  - Input ID to check
    /// @param Int32  - Input win number to check
    /// @param Int32  - Input draw number to check
    /// @param Int32  - Input loss number to check
    /// @param Int32  - Input points number to check
    /// 
    /// @return bool - True if a valid object, else false
    /// </summary>
    private bool SetPlayer(string name, string location, Int32 id, Int32 wins, Int32 draws, Int32 losses, Int32 points)
    {
        // Check input validity
        if (string.IsNullOrEmpty(location))
            return false;
        if (string.IsNullOrEmpty(name))
            return false;
        if (wins < 0)
            return false;
        if (draws < 0)
            return false;
        if (losses < 0)
            return false;
        if (points < 0)
            return false;

        // ID's are not always provided
        if (id != 0)
            player_id = id;

        // Set the objects now we know they are valid
        player_name     = name;
        player_location = location.ToUpper();
        player_wins     = wins;
        player_draws    = draws;
        player_losses   = losses;
        player_points   = points;


        return true;
    }

    /// <summary>
    /// Determines whether or not this player object is valid,
    /// a valid player object is only present if the players W,L,D
    /// is > 0 and the player has a valid name and location - this method is used
    /// as a public interface to check the internal state of the
    /// class
    /// 
    /// @return bool - True if a valid object, else false
    /// </summary>
    public bool IsValid()
    {
        if (string.IsNullOrEmpty(player_location))
            return false;
        if (string.IsNullOrEmpty(player_name))
            return false;
        if (player_wins < 0)
            return false;
        if (player_draws < 0)
            return false;
        if (player_losses < 0)
            return false;
        if (player_points < 0)
            return false;

        return true;
    }

    /// <summary>
    /// Returns the players name
    /// 
    /// @return string - the name of the player
    /// </summary>
    public string GetName()
    {
        return player_name;
    }

    /// <summary>
    /// Returns the players location
    /// 
    /// @return string - the location of the player
    /// </summary>
    public string GetLocation()
    {
        return player_location;
    }

    /// <summary>
    /// Returns the players total wins
    /// 
    /// @return string - the name of the player
    /// </summary>
    public Int32 GetWins()
    {
        return player_wins;
    }

    /// <summary>
    /// Returns the players total wins
    /// 
    /// @return Int32 - total wins
    /// </summary>
    public Int32 GetDraws()
    {
        return player_draws;
    }

    /// <summary>
    /// Returns the players total losses
    /// 
    /// @return Int32 - total losses
    /// </summary>
    public Int32 GetLosses()
    {
        return player_losses;
    }

    /// <summary>
    /// Returns the players total points
    /// 
    /// @return Int32 - total points
    /// </summary>
    public Int32 GetPoints()
    {
        return player_points;
    }

    /// <summary>
    /// Returns the players ID
    /// 
    /// @return Int32 - the ID of the player
    /// </summary>
    public Int32 GetID()
    {
        return player_id;
    }

    /// <summary>
    /// Returns the current player object 
    /// 
    /// @return bool - "this" if it is valid, else null
    /// </summary>
    public ASPlayer GetPlayer()
    {
        if (this.IsValid())
            return this;

        return null;
    }
}