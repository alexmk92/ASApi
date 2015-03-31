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
public class ASKey
{
    [DataMember]
    private string key;
    [DataMember]
    private Int32 permission;

    /// <summary>
    /// Creates the new key
    /// 
    /// <parm name="name">Name of the Player</parm>
    /// <parm name="location">Continent of the player</parm>
    /// 
    /// </summary>
    public ASKey(string key, Int32 permission)
    {
        // Perform a double validity check 
        if (!SetKey(key, permission) && !this.IsValid())
            throw new ArgumentException("There was an error when initialising ASPlayer, please ensure you pass positive integers and non-empty strings");
    }

    /// <summary>
    /// Sets the new key and checks if it is valid
    /// </summary>
    /// <param name="key"></param>
    /// <param name="permission"></param>
    /// <returns></returns>
    private bool SetKey(string key, Int32 permission)
    {
        // Check input validity
        if (string.IsNullOrEmpty(key) || key.Length < 24)
            return false;
        if (permission < 0)
            return false;

        // Set the objects now we know they are valid
        this.key        = key;
        this.permission = permission;

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
        if (string.IsNullOrEmpty(key) || key.Length < 24) 
            return false;
        if (permission < 0)
            return false;

        return true;
    }

    /// <summary>
    /// Returns the players name
    /// 
    /// @return string - the name of the player
    /// </summary>
    public string GetKey()
    {
        return key;
    }

    /// <summary>
    /// Returns the Permission
    /// </summary>
    /// <returns></returns>
    public Int32 GetPermission()
    {
        return permission;
    }
}