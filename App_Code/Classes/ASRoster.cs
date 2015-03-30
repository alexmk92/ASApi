using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// The Roster class holds all information of players who belong
/// to a specific team.  A team can have any number of players 
/// so long as it is no less than 5. 
/// 
/// @param List<ASPlayer> - Input list of players
/// 
/// @return bool - True if the list was valid, else false
/// </summary>
public class ASRoster
{
    private List<ASPlayer> players;

	public ASRoster(List<ASPlayer> playersIn)
	{
        // Set the players array, ensuring we have some data
        if (playersIn != null && playersIn.Count > 0)
            players = playersIn;

        // Ensure that we have a valid object
        if (!this.IsValid())
            throw new ArgumentException("The ASRoster object was invalid. Please ensure the list of players is > 4 and < 17");
	}

    /// <summary>
    /// Determines whether or not this roster object is valid,
    /// a valid roster object is only present if there are at 
    /// least 5 players in the team, and no more than 16 players
    /// assigned to the team
    /// 
    /// @return bool - True if a valid object, else false
    /// </summary>
    public bool IsValid()
    {
        if (players == null)
            return false;
        if (players.Count < 5 || players.Count > 16)
            return false;

        return true;
    }
}