# Friend Accepter Plugin for ArchiSteamFarm

ASF plugin for automatically accepting incoming friend requests (and group auto post).

## Installation

1. Download the .zip file from
   the [![GitHub Release](https://img.shields.io/github/v/release/JackieWaltRyan/FriendAccepter?display_name=tag&logo=github&label=latest%20release)](https://github.com/JackieWaltRyan/FriendAccepter/releases/latest).<br><br>
2. Locate the `plugins` folder inside your ASF folder. Create a new folder here and unpack the downloaded .zip file to
   that folder.<br><br>
3. (Re)start ASF, you should get a message indicating that the plugin loaded successfully.

## Usage

Default configuration. To change this feature, add the following parameter to your bot's config file:

```json
{
  "FriendAccepterConfig": {
    "AcceptFriends": false,
    "GroupAutoPost": false,
    "GroupAutoPostConfig": {
      "GroupID": 103582791432987389,
      "Comment": "Feel free to add me! Accept everyone!",
      "Timeout": 60
    }
  }
}
```

- `AcceptFriends` - `bool` type with default value of `false`. If `true`, automatically accept all incoming friend
  requests for the current bot.<br><br>
- `GroupAutoPost` - `bool` type with default value of `false`. If `true`, automatically post a comment to a group to
  find friends. For friend collectors only.
- #### GroupAutoPostConfig:
    - `GroupID` - `ulong` type with default value of
      `103582791432987389` ([Search for Friends](https://steamcommunity.com/groups/SearchForFriends) group). ID of the group to which the comment will be sent.<br><br>
    - `Comment` - `string` type with default value of `Feel free to add me! Accept everyone!`. The text of the comment
      that will be published in the group.<br><br>
    - `Timeout` - `uint` type with default value of `60`. The interval in minutes after which the comment will be sent
      again.
