using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// The Team class stores all information on the team, including
/// their roster which holda a list of all players in that team
/// </summary>
public class ASTeam
{
    // Private encapsulated variables
    private string   team_name;
    private Int32    team_id;
    private Int32    team_wins;
    private Int32    team_draws;
    private Int32    team_losses;
    private ASRoster team_roster; 

    /// <summary>
    /// Initialises all encapsulated variables, and check their validity to create
    /// the new team object.
    /// 
    /// @param string   - The name of the team
    /// @param Int32    - The teams unique id
    /// @param Int32    - The number of wins the team has had
    /// @param Int32    - The number of draws the team has had
    /// @param Int32    - The number of losses the team has had
    /// @param ASRoster - The teams roster, holding info on all players
    /// </summary>
	public ASTeam(string name, Int32 id, Int32 wins, Int32 draws, Int32 losses, ASRoster roster)
	{
        // Perform a double validity check 
        if (!SetTeam(name, id, wins, draws, losses, roster) && this.IsValid())
            throw new ArgumentException("There was an error when constructing ASTeam, please ensure you provided positive integers and a non empty name string");
	}

    /// <summary>
    /// Sets the new team object, replicates most of the checks performed
    /// in the public interface to check the objects internal state.
    /// 
    /// @param string   - The name to be checked
    /// @param Int32    - The id to be checked
    /// @param Int32    - The wins to be checked
    /// @param Int32    - The draws to be checked
    /// @param Int32    - The losses to be checked
    /// @param ASRoster - The roster to be checked
    /// 
    /// @return bool - True if a valid object, else false
    /// </summary>
    private bool SetTeam(string name, Int32 id, Int32 wins, Int32 draws, Int32 losses, ASRoster roster)
    {
        // Check input validity
        if (string.IsNullOrEmpty(name))
            return false;
        if (id == 0)
            return false;
        if (wins < 0)
            return false;
        if (draws < 0)
            return false;
        if (losses < 0)
            return false;
        if (!roster.IsValid())
            return false;

        // Set the objects now we know they are valid
        team_name   = name;
        team_id     = id;
        team_wins   = wins;
        team_draws  = draws;
        team_losses = losses;
        team_roster = roster;

        return true;
    }

    /// <summary>
    /// Determines whether or not this team object is valid,
    /// a valid team object is only present if the teams W,L,D
    /// is > 0 and the team has a valid name - this method is used
    /// as a public interface to check the internal state of the
    /// class
    /// 
    /// @return bool - True if a valid object, else false
    /// </summary>
    public bool IsValid()
    {

        if (string.IsNullOrEmpty(team_name))
            return false;
        if (team_id == 0)
            return false;
        if (team_wins < 0)
            return false;
        if (team_draws < 0)
            return false;
        if (team_losses < 0)
            return false;
        if (!team_roster.IsValid())
            return false;

        return true;
    }

    /// <summary>
    /// Returns the current team object 
    /// 
    /// @return bool - "this" if it is valid, else null
    /// </summary>
    public ASTeam GetTeam()
    {
        if (this.IsValid())
            return this;

        return null;
    }

    /// <summary>
    /// Returns the ID of the current Team
    /// 
    /// @return Int32 - The ID of the team, else 0
    /// </summary>
    public Int32 GetID()
    {
        if (team_id >= 0)
            return team_id;

        return 0;
    }

    /// <summary>
    /// Returns the name of the team object
    /// 
    /// @return string - the name of the team, else empty
    /// </summary>
    public string GetName()
    {
        if (!string.IsNullOrEmpty(team_name))
            return team_name;

        return "";
    }

    /// <summary>
    /// Returns the team Roster
    /// 
    /// @return ASRoster - the roster if valid, else null
    /// </summary>
    public ASRoster GetRoster()
    {
        if (team_roster.IsValid())
            return team_roster;

        return null;
    }

    /// <summary>
    /// Return number of wins
    /// 
    /// @return Int32 - Num wins if positive, else 0
    /// </summary>
    public Int32 GetNumWins()
    {
        if (team_wins >= 0)
            return team_wins;

        return 0;
    }

    /// <summary>
    /// Return number of draws
    /// 
    /// @return Int32 - Num draws if positive, else 0
    /// </summary>
    public Int32 GetNumDraws()
    {
        if (team_draws >= 0)
            return team_draws;

        return 0;
    }

    /// <summary>
    /// Return number of losses
    /// 
    /// @return Int32 - Num losses if positive, else 0
    /// </summary>
    public Int32 GetNumLosses()
    {
        if (team_losses >= 0)
            return team_losses;

        return 0;
    }
}