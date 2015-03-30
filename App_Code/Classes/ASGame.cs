using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Enumerable to determine the outcome of the game.
/// A game has 3 states A_WIN_B_LOSS, B_WIN_A_LOSS and DRAW
/// </summary>
public enum ASResult { A_WIN_B_LOSS, B_WIN_A_LOSS, DRAW };

/// <summary>
/// The Game class holds all information on a Game.  In the 
/// context of this system it is fairly simplistic, a Game
/// shall only store information on the two teams and their
/// scores.  (Scores are based on the amount of kills and
/// objectives a team secured in the game)
/// </summary>
public class ASGame
{
    // Encapsulate private variables relating to the
    // teams who are in this game
    private ASTeam teamA;
    private ASTeam teamB;
    private bool   teamAWon = false;
    private bool   teamBWon = false;

    /// <summary>
    /// Perform any basic initialisation of the local
    /// variables, here we set scores to 0 and team to null, this method
    /// calls the private interfaces to set the inputs
    /// 
    /// @param ASTeam - Team A
    /// @param ASTeam - Team B
    /// @param bool   - Team A Won
    /// @param bool   - Team B Won
    /// 
    /// </summary>
    public ASGame(ASTeam ATeam, ASTeam BTeam, bool AWon, bool BWon)
	{
        if (!setTeams(ATeam, BTeam))
            throw new ArgumentException("The ASTeam objects sent to ASGame were invalid, please ensure the format of your objects");
        if (!setScores(AWon, BWon))
            throw new ArgumentException("Please ensure that Boolean values are passed to the ASGame Constructor");
	}

    /// <summary>
    /// Sets the teams who participated in this game, this method
    /// will return an error code based on whether the team was
    /// valid or not.  This is a private interface as the state
    /// of the object is immutable once in the system
    /// 
    /// @param  Team  - Team A
    /// @param  Team  - Team B
    /// 
    /// @return bool  - false if there was an error, else true
    /// </summary>
    private bool setTeams(ASTeam ATeam, ASTeam BTeam)
    {        
        // Check whether the new team objects are valid using the
        // Team class's validity check method
        if (!ATeam.IsValid() || !BTeam.IsValid())
            return false;

        // Set the teams as we are now sure that the input objects are
        // valid
        teamA = ATeam;
        teamB = BTeam;

        // This should not happen, but we initialise the team objects
        // as null pointers in the constructor.  Therefore its best to 
        // check whether they are still null, if they are return false
        if (teamA == null || teamB == null)
            return false;

        // Everything was successful
        return true;
    }

    /// <summary>
    /// Sets the outcome of Team's A and B game, this is a private interface
    /// as the state of the game is immutable once in the data structure.
    /// Games are decided by which team took down the other teams inhibitor,
    /// we can represent this state with a TRUE or FALSE toggle
    /// 
    /// @param bool  - Team A won
    /// @param bool  - Team B won
    /// 
    /// </summary>
    private bool setScores(bool AWon, bool BWon)
    {
        // Set the scores for the teams and return true as we can be sure that
        // the new values have been set
        teamAWon = AWon;
        teamBWon = BWon;

        return true;
    }

    /// <summary>
    /// Determines whether or not we are dealing with a valid Game object.
    /// A Game object is only valid if its team are valid and a positive
    /// integer has been set for both scores.  
    /// </summary>
    public bool IsValid()
    {
        if (teamA.IsValid() && teamB.IsValid())
            return true;

        return false;
    }

    /// <summary>
    /// Tests the results of the match based on the boolean values set.
    /// The following combinations exist:

    /// -- TRUE FALSE  = A_WIN_B_LOSS
    /// -- TRUE FALSE  = B_WIN_A_LOSS
    /// -- TRUE TRUE   = DRAW
    /// -- FALSE FALSE = DRAW
    /// 
    /// @return ASResult - Enumerated value dependent on the boolean outcomes
    /// </summary>
    public ASResult GetResult()
    {
        if (teamAWon && !teamBWon)
            return ASResult.A_WIN_B_LOSS;
        if (teamBWon && !teamAWon)
            return ASResult.B_WIN_A_LOSS;
        if (teamAWon && teamBWon || !teamAWon && !teamBWon)
            return ASResult.DRAW;

        // Default return value - should never happen
        return ASResult.DRAW;
    }

    /// <summary>
    /// Returns the current game object, so long as it is valid 
    /// 
    /// @return ASGame - "this" object if valid, else null
    /// </summary>
    public ASGame GetGame()
    {
        if (this.IsValid())
            return this;

        return null;
    }

    /// <summary>
    /// Public Interface to return Team A
    /// 
    /// @return ASTeam - "TeamA" if valid, else null
    /// </summary>
    public ASTeam GetTeamA()
    {
        if (teamA.IsValid())
            return teamA;

        return null;
    }

    /// <summary>
    /// Public Interface to return Team B
    /// 
    /// @return ASTeam - "TeamB" if valid, else null
    /// </summary>
    public ASTeam GetTeamB()
    {
        if (teamB.IsValid())
            return teamB;

        return null;
    }

    /// <summary>
    /// Returns the bit representation of the boolean value
    /// for score A, this is needed for inputting into the db
    /// 
    /// @return Int32 - 1 if true, else 0
    /// </summary>
    public Int32 GetResultAAsInt()
    {
        if (teamAWon)
            return 1;
        return 0;
    }

    /// <summary>
    /// Returns the bit representation of the boolean value
    /// for score B, this is needed for inputting into the db
    /// 
    /// @return Int32 - 1 if true, else 0
    /// </summary>
    public Int32 GetResultBAsInt()
    {
        if (teamBWon)
            return 1;
        return 0;
    }
   


}